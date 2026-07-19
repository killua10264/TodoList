using TodoListBackend.Models;

namespace TodoListBackend.Repositories
{
    public interface ISubTaskRepository
    {
        Task<IEnumerable<SubTask>> GetByTodoIdAsync(int todoId, int userId);
        Task<int> GetCountByTodoIdAsync(int todoId);
        Task<int> GetMaxSortOrderByTodoIdAsync(int todoId);
        Task<SubTask?> GetByIdAsync(int id, int userId, bool trackChanges = false);
        Task AddAsync(SubTask subTask);
        Task DeleteAsync(SubTask subTask);
    }
}
