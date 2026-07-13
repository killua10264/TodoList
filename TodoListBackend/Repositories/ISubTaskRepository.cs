using TodoListBackend.Models;

namespace TodoListBackend.Repositories
{
    public interface ISubTaskRepository
    {
        Task<IEnumerable<SubTask>> GetByTodoIdAsync(int todoId, int userId);
        Task<SubTask?> GetByIdAsync(int id, int userId);
        Task AddAsync(SubTask subTask);
        Task DeleteAsync(SubTask subTask);
    }
}
