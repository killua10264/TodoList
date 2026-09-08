using TodoListBackend.Models;
using TodoListBackend.DTOs.Todo;

namespace TodoListBackend.Repositories
{
    public interface ITodoRepository
    {
        Task<(IEnumerable<Todo> Items, int TotalCount)> GetAllTodosAsync(int userId, TodoQueryDto query);
        Task<Todo?> GetByIdAsync(int id, int userId, bool trackChanges = false, bool includeDeleted = false);
        Task AddAsync(Todo todo);
        void Remove(Todo todo);
    }
}
