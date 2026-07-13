namespace TodoListBackend.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public UserRole Role { get; set; } = UserRole.User;

        public string? RefreshToken { get; set; } 
        public DateTime? RefreshTokenExpiryTime { get; set; }

        public string? AvatarUrl { get; set; }
        public string? DisplayName { get; set; }
        public string? Bio { get; set; }

        public string Timezone { get; set; } = "Asia/Ho_Chi_Minh";
        public string Theme { get; set; } = "light";
        public string Language { get; set; } = "vi";
        public string FirstDayOfWeek { get; set; } = "Monday";

        //Navigation Property
        public ICollection<Todo> Todos { get; set; } = new List<Todo>();
    }
}