namespace TodoListBackend.Models
{
    public class RefreshTokenSession
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public int UserId { get; set; }
        public string TokenHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
        public DateTime? RevokedAt { get; set; }
        public Guid? ReplacedBySessionId { get; set; }
        public string? UserAgent { get; set; }
        public string? IpAddress { get; set; }
        public Guid ConcurrencyToken { get; set; } = Guid.NewGuid();

        public User User { get; set; } = null!;
    }
}
