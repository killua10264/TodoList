using TodoListBackend.Models;

namespace TodoListBackend.Repositories
{
    public interface ITodoRepository
    {
        Task<(IEnumerable<Todo> Items, int TotalCount)> GetAllTodosAsync(int userId, int page = 1, int pageSize = 20, string? filter = null, int? categoryId = null, string? status = null, string? sortBy = null);
        Task<Todo?> GetByIdAsync(int id, int userId);
        Task AddAsync(Todo todo);
    }
}