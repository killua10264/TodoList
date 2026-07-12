namespace TodoListBackend.DTOs.Todo
{
    public class TodoUpdateDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public bool? IsCompleted { get; set; }
        public int? Priority { get; set; }
        public DateTime? DueDate { get; set; }
        public int? ProjectId { get; set; }
    }
}