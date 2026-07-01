namespace TodoListBackend.Models
{
    public class User
    {
        public int Id { get; set; }

        //Khởi tạo giá trị mặc định để tránh lỗi null reference
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        //Navigation Property
        public ICollection<Todo> Todos { get; set; } = new List<Todo>();
    }
}