namespace TodoListBackend.DTOs.SubTask
{
    public class SubTaskUpdateDto
    {
        public string Title { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public int SortOrder { get; set; }
    }
}
