using TodoListBackend.DTOs.Todo;

namespace TodoListBackend.Services
{
    public interface ITodoService
    {
        Task<DTOs.PaginatedResponse<TodoResponseDto>> GetAllTodosAsync(int userId, int page = 1, int pageSize = 20, string? filter = null, int? categoryId = null, string? status = null, string? sortBy = null);
        Task<TodoResponseDto?> GetTodoByIdAsync(int id, int userId);
        Task<TodoResponseDto> CreateTodoAsync(TodoCreateDto dto, int userId);
        Task DeleteTodoAsync(int id, int userId);
        Task<TodoResponseDto> UpdateTodoAsync(int id, TodoUpdateDto dto, int userId);
    }
}