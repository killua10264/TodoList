namespace TodoListBackend.DTOs
{
    public class TodoUpdateDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public int Priority { get; set; }
        public DateTime DueDate { get; set; }
        public int UserId { get; set; }
        public int CategoryId { get; set; }
    }
}