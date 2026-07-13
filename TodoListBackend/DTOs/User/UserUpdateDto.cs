namespace TodoListBackend.DTOs.User
{
    public class UserUpdateDto
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? AvatarUrl { get; set; }
        public string? DisplayName { get; set; }
        public string? Bio { get; set; }
        public string? Timezone { get; set; }
        public string? Theme { get; set; }
        public string? Language { get; set; }
        public string? FirstDayOfWeek { get; set; }
    }
}