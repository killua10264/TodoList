using TodoListBackend.Models;

namespace TodoListBackend.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id, bool trackChanges = false);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail);
        Task<bool> ExistsByEmailAsync(string email);
        Task<bool> ExistsByUsernameAsync(string username);

        Task<bool> ExistsByEmailAsync(string email, int excludeUserId);
        Task<bool> ExistsByUsernameAsync(string username, int excludeUserId);
        Task AddAsync(User user);
        Task<User?> GetByRefreshTokenAsync(string refreshToken);

    }
}