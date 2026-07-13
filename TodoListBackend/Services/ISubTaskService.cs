using TodoListBackend.DTOs.SubTask;

namespace TodoListBackend.Services
{
    public interface ISubTaskService
    {
        Task<IEnumerable<SubTaskResponseDto>> GetSubTasksByTodoIdAsync(int todoId, int userId);
        Task<SubTaskResponseDto> CreateSubTaskAsync(SubTaskCreateDto dto, int userId);
        Task<SubTaskResponseDto> UpdateSubTaskAsync(int id, SubTaskUpdateDto dto, int userId);
        Task DeleteSubTaskAsync(int id, int userId);
    }
}
