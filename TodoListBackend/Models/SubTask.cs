namespace TodoListBackend.Models
{
    public class SubTask
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public int SortOrder { get; set; }
        public int LeafShape { get; set; } // 0-4: các hình dạng lá khác nhau
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int TodoId { get; set; }
        public Todo Todo { get; set; } = null!;
    }
}

