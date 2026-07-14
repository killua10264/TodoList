using TodoListBackend.Models;

namespace TodoListBackend.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail);
        Task<bool> ExistsByEmailAsync(string email);
        Task<bool> ExistsByUsernameAsync(string username);
        // FIX 3.7: Check email/username tồn tại nhưng loại trừ user hiện tại
        Task<bool> ExistsByEmailAsync(string email, int excludeUserId);
        Task<bool> ExistsByUsernameAsync(string username, int excludeUserId);
        Task AddAsync(User user);
        Task<User?> GetByRefreshTokenAsync(string refreshToken);
        // FIX 2.4: Xóa UpdateAsync — EF Change Tracker tự detect changes
    }
}