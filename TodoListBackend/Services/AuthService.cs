using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TodoListBackend.DTOs.Auth;
using TodoListBackend.Models;
using TodoListBackend.Options;
using TodoListBackend.Repositories;
using TodoListBackend.Security;

namespace TodoListBackend.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly JwtSettings _jwtSettings;
        private readonly RefreshTokenSettings _refreshTokenSettings;

        public AuthService(
            IUnitOfWork unitOfWork,
            IOptions<JwtSettings> jwtOptions,
            IOptions<RefreshTokenSettings> refreshTokenOptions)
        {
            _unitOfWork = unitOfWork;
            _jwtSettings = jwtOptions.Value;
            _refreshTokenSettings = refreshTokenOptions.Value;
        }

        public async Task<AuthTokenResult> RegisterAsync(
            RegisterDto dto,
            AuthSessionContext sessionContext)
        {
            if (await _unitOfWork.Users.ExistsByEmailAsync(dto.Email))
            {
                throw new ArgumentException("Email này đã tồn tại.");
            }

            if (await _unitOfWork.Users.ExistsByUsernameAsync(dto.Username))
            {
                throw new ArgumentException("Tên đăng nhập (Username) này đã tồn tại.");
            }

            var newUser = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Users.AddAsync(newUser);
            await _unitOfWork.SaveChangesAsync();

            var tokenResult = CreateTokenResult(newUser);
            await SaveSessionAsync(newUser, tokenResult.RefreshToken, sessionContext);
            return tokenResult;
        }

        public async Task<AuthTokenResult> LoginAsync(
            LoginDto dto,
            AuthSessionContext sessionContext)
        {
            var identifier = dto.GetIdentifier();
            var user = await _unitOfWork.Users.GetByUsernameOrEmailAsync(identifier);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
            {
                throw new UnauthorizedAccessException();
            }

            var tokenResult = CreateTokenResult(user);
            await SaveSessionAsync(user, tokenResult.RefreshToken, sessionContext);
            return tokenResult;
        }

        public async Task<AuthTokenResult> RefreshTokenAsync(
            string? refreshToken,
            AuthSessionContext sessionContext)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new UnauthorizedAccessException();
            }

            var hashedIncomingToken = HashHelper.ComputeSha256Hash(refreshToken);
            var session = await _unitOfWork.RefreshTokenSessions
                .GetByTokenHashAsync(hashedIncomingToken, trackChanges: true);

            if (session is null)
            {
                return await RefreshLegacyTokenAsync(hashedIncomingToken, sessionContext);
            }

            if (session.ExpiresAt <= DateTime.UtcNow)
            {
                throw new UnauthorizedAccessException();
            }

            if (session.RevokedAt is not null)
            {
                await RevokeActiveSessionsAsync(session.UserId);
                throw new UnauthorizedAccessException();
            }

            var tokenResult = CreateTokenResult(session.User);
            var replacementSession = BuildSession(
                session.User,
                tokenResult.RefreshToken,
                sessionContext);

            session.RevokedAt = DateTime.UtcNow;
            session.ReplacedBySessionId = replacementSession.Id;
            session.ConcurrencyToken = Guid.NewGuid();
            await _unitOfWork.RefreshTokenSessions.AddAsync(replacementSession);

            try
            {
                await _unitOfWork.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new UnauthorizedAccessException();
            }

            return tokenResult;
        }

        public async Task LogoutAsync(string? refreshToken, int? userId)
        {
            if (string.IsNullOrWhiteSpace(refreshToken)) return;

            var hash = HashHelper.ComputeSha256Hash(refreshToken);
            var session = await _unitOfWork.RefreshTokenSessions
                .GetByTokenHashAsync(hash, trackChanges: true);

            if (session is not null)
            {
                if (userId is null || session.UserId == userId)
                {
                    session.RevokedAt ??= DateTime.UtcNow;
                    await _unitOfWork.SaveChangesAsync();
                }

                return;
            }

            // Compatibility path for users who still hold a legacy refresh token
            // before the session-table migration has completed.
            var legacyUser = await _unitOfWork.Users.GetByRefreshTokenAsync(hash);
            if (legacyUser is not null && (userId is null || legacyUser.Id == userId))
            {
                legacyUser.RefreshToken = null;
                legacyUser.RefreshTokenExpiryTime = null;
                await _unitOfWork.SaveChangesAsync();
            }
        }

        private async Task<AuthTokenResult> RefreshLegacyTokenAsync(
            string hashedIncomingToken,
            AuthSessionContext sessionContext)
        {
            var user = await _unitOfWork.Users.GetByRefreshTokenAsync(hashedIncomingToken);

            if (user is null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                throw new UnauthorizedAccessException();
            }

            var tokenResult = CreateTokenResult(user);
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            await SaveSessionAsync(user, tokenResult.RefreshToken, sessionContext);
            return tokenResult;
        }

        private async Task SaveSessionAsync(
            User user,
            string rawRefreshToken,
            AuthSessionContext sessionContext)
        {
            await _unitOfWork.RefreshTokenSessions.AddAsync(
                BuildSession(user, rawRefreshToken, sessionContext));
            await _unitOfWork.SaveChangesAsync();
        }

        private RefreshTokenSession BuildSession(
            User user,
            string rawRefreshToken,
            AuthSessionContext sessionContext)
        {
            return new RefreshTokenSession
            {
                UserId = user.Id,
                TokenHash = HashHelper.ComputeSha256Hash(rawRefreshToken),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(_refreshTokenSettings.ExpiryDays),
                UserAgent = sessionContext.UserAgent,
                IpAddress = sessionContext.IpAddress
            };
        }

        private AuthTokenResult CreateTokenResult(User user)
        {
            return new AuthTokenResult(CreateJwtToken(user), GenerateRefreshToken());
        }

        private async Task RevokeActiveSessionsAsync(int userId)
        {
            var sessions = await _unitOfWork.RefreshTokenSessions.GetActiveByUserIdAsync(userId);
            var now = DateTime.UtcNow;

            foreach (var activeSession in sessions)
            {
                activeSession.RevokedAt = now;
                activeSession.ConcurrencyToken = Guid.NewGuid();
            }

            if (sessions.Count > 0)
            {
                await _unitOfWork.SaveChangesAsync();
            }
        }

        private string CreateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.GivenName, user.Username),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
    }
}
