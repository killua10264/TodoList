namespace TodoListBackend.Services
{
    public sealed record AuthSessionContext(string? UserAgent, string? IpAddress);
}
