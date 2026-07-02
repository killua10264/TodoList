using TodoListBackend.Models;

namespace TodoListBackend.Repositories
{
    public interface IUserRepository
    {
        Task UpdateAsync(User user);
        Task SaveChangesAsync();
        Task<User?> GetByIdAsync(int id);
    }
}