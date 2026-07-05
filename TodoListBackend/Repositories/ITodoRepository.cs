using TodoListBackend.Models;

namespace TodoListBackend.Repositories
{
    public interface ITodoRepository
    {
        // FIX 2.2: Pagination — trả kèm TotalCount
        Task<(IEnumerable<Todo> Items, int TotalCount)> GetAllTodosAsync(int userId, int page = 1, int pageSize = 20);
        Task<Todo?> GetByIdAsync(int id, int userId);
        Task AddAsync(Todo todo);
    }
}