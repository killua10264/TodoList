namespace TodoListBackend.Options
{
    public sealed class RefreshTokenSettings
    {
        public const string SectionName = "RefreshToken";

        public int ExpiryDays { get; set; } = 7;
        public string CookieName { get; set; } = "__Host-todolist-refresh";
        public string CookiePath { get; set; } = "/";
        public string SameSite { get; set; } = "Lax";
        public bool Secure { get; set; } = true;
        public string? Domain { get; set; }
    }
}
