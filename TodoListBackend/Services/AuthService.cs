using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TodoListBackend.Data;
using TodoListBackend.DTOs.Auth;
using TodoListBackend.DTOs.User;
using TodoListBackend.Models;
using TodoListBackend.Options;
using TodoListBackend.Repositories;
using TodoListBackend.Security;

namespace TodoListBackend.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly AppDbContext _dbContext;
        private readonly JwtSettings _jwtSettings;
        private readonly RefreshTokenSettings _refreshTokenSettings;

        public AuthService(
            IUnitOfWork unitOfWork,
            IOptions<JwtSettings> jwtOptions,
            IOptions<RefreshTokenSettings> refreshTokenOptions,
            AppDbContext dbContext)
        {
            _unitOfWork = unitOfWork;
            _dbContext = dbContext;
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
                await RevokeActiveSessionsAsync(session.UserId, "refresh_replay");
                throw new UnauthorizedAccessException();
            }

            var tokenResult = CreateTokenResult(session.User);
            var replacementSession = BuildSession(
                session.User,
                tokenResult.RefreshToken,
                sessionContext);

            var now = DateTime.UtcNow;
            session.LastUsedAt = now;
            session.RevokedAt = now;
            session.RevocationReason = "rotated";
            session.ReplacedBySessionId = replacementSession.Id;
            session.ConcurrencyToken = Guid.NewGuid();
            await _unitOfWork.RefreshTokenSessions.AddAsync(replacementSession);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();
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
                    session.RevocationReason ??= "logout";
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

        public async Task LogoutAllAsync(int userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId, trackChanges: true);
            if (user is null)
            {
                throw new UnauthorizedAccessException();
            }

            var activeSessions = await _unitOfWork.RefreshTokenSessions.GetActiveByUserIdAsync(userId);
            var now = DateTime.UtcNow;

            foreach (var session in activeSessions)
            {
                session.RevokedAt = now;
                session.RevocationReason = "logout_all";
                session.ConcurrencyToken = Guid.NewGuid();
            }

            // Remove the transitional legacy token as well. This prevents an old
            // client from creating a new session after logout-all.
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;

            await _unitOfWork.SaveChangesAsync();
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
            var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenMinutes);
            return new AuthTokenResult(
                CreateJwtToken(user, expiresAt),
                GenerateRefreshToken(),
                expiresAt,
                ToResponseDto(user));
        }

        private async Task RevokeActiveSessionsAsync(int userId, string reason)
        {
            var sessions = await _unitOfWork.RefreshTokenSessions.GetActiveByUserIdAsync(userId);
            var now = DateTime.UtcNow;

            foreach (var activeSession in sessions)
            {
                activeSession.RevokedAt = now;
                activeSession.RevocationReason = reason;
                activeSession.ConcurrencyToken = Guid.NewGuid();
            }

            if (sessions.Count > 0)
            {
                await _unitOfWork.SaveChangesAsync();
            }
        }

        private string CreateJwtToken(User user, DateTime expiresAt)
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
                expires: expiresAt,
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static UserResponseDto ToResponseDto(User user) => new()
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            AvatarUrl = user.AvatarUrl,
            DisplayName = user.DisplayName,
            Bio = user.Bio,
            Timezone = user.Timezone ?? "Asia/Ho_Chi_Minh",
            Theme = user.Theme ?? "light",
            Language = user.Language ?? "vi",
            FirstDayOfWeek = user.FirstDayOfWeek ?? "Monday",
            CreatedAt = user.CreatedAt
        };

        private static string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
    }
}
