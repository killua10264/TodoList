namespace TodoListBackend.DTOs.Todo
{
    public class TodoResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public int Priority { get; set; }
        public DateTime DueDate { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string ProjectColor { get; set; } = string.Empty;
    }
}