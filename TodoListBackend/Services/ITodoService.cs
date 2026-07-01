using TodoListBackend.Models;
using TodoListBackend.DTOs;

namespace TodoListBackend.Services
{
    public interface ITodoService
    {
        Task<IEnumerable<Todo>> GetAllTodosAsync();
        Task<Todo?> CreateTodoAsync(TodoCreateDto dto);
        Task<bool> SoftDeleteTodoAsync(int id);
        Task<Todo?> UpdateTodoAsync(int id, TodoUpdateDto dto);
    }
}