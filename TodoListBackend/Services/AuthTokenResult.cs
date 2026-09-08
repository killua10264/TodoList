using TodoListBackend.DTOs.User;

namespace TodoListBackend.Services;

public sealed record AuthTokenResult(string AccessToken, string RefreshToken)
{
    public DateTime ExpiresAt { get; init; }
    public UserResponseDto? User { get; init; }

    public AuthTokenResult(
        string accessToken,
        string refreshToken,
        DateTime expiresAt,
        UserResponseDto user)
        : this(accessToken, refreshToken)
    {
        ExpiresAt = expiresAt;
        User = user;
    }
}
