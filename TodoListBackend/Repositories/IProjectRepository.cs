using TodoListBackend.Models;

namespace TodoListBackend.Repositories
{
    public interface IProjectRepository
    {
        Task<IEnumerable<Project>> GetAllProjectsAsync(int userId);
        Task<Project?> GetByIdAsync(int id, int userId);
        Task<Project?> GetByNameAsync(string name);
        Task AddAsync(Project project);
        Task DeleteAsync(Project project);
    }
}
