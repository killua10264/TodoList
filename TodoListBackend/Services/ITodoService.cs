using TodoListBackend.Models;
using TodoListBackend.DTOs.Todo;

namespace TodoListBackend.Services
{
    public interface ITodoService
    {
        Task<IEnumerable<Todo>> GetAllTodosAsync();
        Task<Todo?> CreateTodoAsync(TodoCreateDto dto);
        Task<bool> DeleteTodoAsync(int id);
        Task<Todo?> UpdateTodoAsync(int id, TodoUpdateDto dto);
    }
}