namespace TodoListBackend.DTOs.SubTask
{
    public class SubTaskCreateDto
    {
        public string Title { get; set; } = string.Empty;
        public int TodoId { get; set; }
    }
}
