namespace TodoListBackend.Services
{
    public sealed record AuthTokenResult(string AccessToken, string RefreshToken);
}
