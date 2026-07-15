namespace TodoListBackend.DTOs.Category
{
    public class CategoryResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public int TodoCount { get; set; }
        public int CompletedTodoCount { get; set; }
    }
}
