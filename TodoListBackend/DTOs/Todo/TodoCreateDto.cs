namespace TodoListBackend.DTOs.Todo
{
    public class TodoCreateDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Priority { get; set; } = 1;
        public DateTime DueDate { get; set; }
        public int CategoryId { get; set; }
    }
}