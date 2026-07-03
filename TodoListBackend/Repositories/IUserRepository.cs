using TodoListBackend.Models;

namespace TodoListBackend.Repositories
{
    public interface IUserRepository
    {
        Task UpdateAsync(User user);
        Task SaveChangesAsync();
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByEmailAsync(string email);
        Task<bool> ExistsByEmailAsync(string email);
        Task<User> CreateUserAsync(User user);
        Task<User?> GetByRefreshTokenAsync(string refreshToken);
    }
}