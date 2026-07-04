using TodoListBackend.Models;
using TodoListBackend.DTOs.Todo;

namespace TodoListBackend.Repositories
{
    public interface ITodoRepository
    {
        Task<IEnumerable<TodoResponseDto>> GetAllTodosAsync(int userId);
        Task<Todo?> GetByIdAsync(int id, int userId);
        Task AddAsync(Todo todo);
        Task UpdateAsync(Todo todo);
        Task SaveChangesAsync();
    }
}