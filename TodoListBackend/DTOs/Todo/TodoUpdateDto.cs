namespace TodoListBackend.DTOs.Todo
{
    public class TodoUpdateDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public bool? IsCompleted { get; set; }
        public bool? IsHidden { get; set; }
        public int? Priority { get; set; }
        public DateOnly? DueDate { get; set; }
        public int? CategoryId { get; set; }
        public uint? Version { get; set; }
    }
}
