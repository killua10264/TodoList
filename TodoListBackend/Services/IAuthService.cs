using TodoListBackend.DTOs.Auth;

namespace TodoListBackend.Services
{
    public interface IAuthService
    {
        Task<AuthTokenResult> RegisterAsync(RegisterDto dto, AuthSessionContext sessionContext);
        Task<AuthTokenResult> LoginAsync(LoginDto dto, AuthSessionContext sessionContext);
        Task<AuthTokenResult> RefreshTokenAsync(string? refreshToken, AuthSessionContext sessionContext);
        Task LogoutAsync(string? refreshToken, int? userId);
    }
}
