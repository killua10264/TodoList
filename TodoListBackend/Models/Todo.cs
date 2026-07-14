namespace TodoListBackend.Models
{
    public class Todo
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public bool IsDeleted { get; set; }
        public int Priority { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // 1 - N voi User
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        // 1 - N voi Category
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        // 1 - N voi SubTask
        public ICollection<SubTask> SubTasks { get; set; } = new List<SubTask>();
    }
}