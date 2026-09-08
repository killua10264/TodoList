namespace TodoListBackend.DTOs.User;

public sealed class ProfileUpdateDto
{
    public string Username { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string Timezone { get; set; } = "Asia/Ho_Chi_Minh";
    public string Theme { get; set; } = "light";
    public string Language { get; set; } = "vi";
    public string FirstDayOfWeek { get; set; } = "Monday";
}
