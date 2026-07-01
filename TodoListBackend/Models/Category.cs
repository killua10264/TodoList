namespace TodoListBackend.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;

        // Navigation Property
        public ICollection<Todo> Todo { get; set; } = new List<Todo>();
    }
}