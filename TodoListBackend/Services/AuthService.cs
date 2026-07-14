using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TodoListBackend.DTOs.Auth;
using TodoListBackend.Models;
using TodoListBackend.Repositories;
using TodoListBackend.Security;

namespace TodoListBackend.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public AuthService(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            if (await _unitOfWork.Users.ExistsByEmailAsync(dto.Email))
            {
                throw new ArgumentException("Email này đã tồn tại.");
            }

            if (await _unitOfWork.Users.ExistsByUsernameAsync(dto.Username))
            {
                throw new ArgumentException("Tên đăng nhập (Username) này đã tồn tại.");
            }

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var newUser = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                Password = passwordHash,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Users.AddAsync(newUser);
            await _unitOfWork.SaveChangesAsync(); // Save để lấy User.Id

            // Tự động login sau khi đăng ký
            var accessToken = CreateJwtToken(newUser);
            var rawRefreshToken = GenerateRefreshToken();
            var refreshToken = HashHelper.ComputeSha256Hash(rawRefreshToken);

            newUser.RefreshToken = refreshToken;
            newUser.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            await _unitOfWork.SaveChangesAsync();

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = rawRefreshToken
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            string identifier = dto.GetIdentifier();
            var user = await _unitOfWork.Users.GetByUsernameOrEmailAsync(identifier);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
            {
                throw new UnauthorizedAccessException("Tên đăng nhập/Email hoặc mật khẩu không đúng.");
            }

            var accessToken = CreateJwtToken(user);
            var rawRefreshToken = GenerateRefreshToken();
            var refreshToken = HashHelper.ComputeSha256Hash(rawRefreshToken);

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            await _unitOfWork.SaveChangesAsync();

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = rawRefreshToken
            };
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
        {
            string hashedIncomingToken = HashHelper.ComputeSha256Hash(refreshToken);
            var user = await _unitOfWork.Users.GetByRefreshTokenAsync(hashedIncomingToken);

            if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                throw new UnauthorizedAccessException("Refresh Token không hợp lệ hoặc đã hết hạn. Vui lòng đăng nhập lại.");
            }

            var newAccessToken = CreateJwtToken(user);
            var rawNewRefreshToken = GenerateRefreshToken();
            var newRefreshToken = HashHelper.ComputeSha256Hash(rawNewRefreshToken);

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            await _unitOfWork.SaveChangesAsync();

            return new AuthResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = rawNewRefreshToken
            };
        }

        public async Task LogoutAsync(int userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null) return;

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;

            await _unitOfWork.SaveChangesAsync();
        }

        private string CreateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.GivenName, user.Username),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var keyString = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("JWT signing key is not configured. Set 'Jwt:Key' in environment variables or user secrets.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: credentials
            );

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
