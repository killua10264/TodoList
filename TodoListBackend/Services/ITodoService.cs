using TodoListBackend.DTOs.Todo;

namespace TodoListBackend.Services
{
    public interface ITodoService
    {
        Task<DTOs.PaginatedResponse<TodoResponseDto>> GetAllTodosAsync(int userId, TodoQueryDto query);
        Task<TodoResponseDto?> GetTodoByIdAsync(int id, int userId);
        Task<TodoResponseDto> CreateTodoAsync(TodoCreateDto dto, int userId);
        Task DeleteTodoAsync(int id, int userId, uint expectedVersion);
        Task RestoreTodoAsync(int id, int userId, uint expectedVersion);
        Task HardDeleteTodoAsync(int id, int userId, uint expectedVersion);
        Task<TodoResponseDto> UpdateTodoAsync(int id, TodoUpdateDto dto, int userId);
    }
}
