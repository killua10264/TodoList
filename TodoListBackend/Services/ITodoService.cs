using TodoListBackend.Models;
using TodoListBackend.DTOs.Todo;

namespace TodoListBackend.Services
{
    public interface ITodoService
    {
        Task<IEnumerable<TodoResponseDto>> GetAllTodosAsync(int userId);
        Task<TodoResponseDto?> GetTodoByIdAsync(int id, int userId);
        Task<TodoResponseDto?> CreateTodoAsync(TodoCreateDto dto, int userId);
        Task<bool> DeleteTodoAsync(int id, int userId);
        Task<TodoResponseDto?> UpdateTodoAsync(int id, TodoUpdateDto dto, int userId);
    }
}