namespace TodoListBackend.DTOs.Auth
{
    public class LoginDto
    {
        public string? Email { get; set; }
        public string? UsernameOrEmail { get; set; }
        public string? Username { get; set; }
        public string Password { get; set; } = string.Empty;

        public string GetIdentifier()
        {
            if (!string.IsNullOrWhiteSpace(UsernameOrEmail)) return UsernameOrEmail.Trim();
            if (!string.IsNullOrWhiteSpace(Email)) return Email.Trim();
            if (!string.IsNullOrWhiteSpace(Username)) return Username.Trim();
            return string.Empty;
        }
    }
}