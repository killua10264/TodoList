namespace TodoListBackend.DTOs.User
{
    public class UserResponseDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public string? DisplayName { get; set; }
        public string? Bio { get; set; }
        public string Timezone { get; set; } = "Asia/Ho_Chi_Minh";
        public string Theme { get; set; } = "light";
        public string Language { get; set; } = "vi";
        public string FirstDayOfWeek { get; set; } = "Monday";
        public DateTime CreatedAt { get; set; }
    }
}