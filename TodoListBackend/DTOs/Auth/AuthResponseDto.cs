using TodoListBackend.DTOs.User;

namespace TodoListBackend.DTOs.Auth;

public sealed class AuthResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserResponseDto? User { get; set; }
}
