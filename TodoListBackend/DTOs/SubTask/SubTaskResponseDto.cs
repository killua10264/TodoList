namespace TodoListBackend.DTOs.SubTask
{
    public class SubTaskResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public int SortOrder { get; set; }
        public int LeafShape { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TodoId { get; set; }
    }
}
