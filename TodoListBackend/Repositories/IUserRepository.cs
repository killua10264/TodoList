using TodoListBackend.Models;

namespace TodoListBackend.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByEmailAsync(string email);
        Task<bool> ExistsByEmailAsync(string email);
        // FIX 3.7: Check email tồn tại nhưng loại trừ user hiện tại
        Task<bool> ExistsByEmailAsync(string email, int excludeUserId);
        Task AddAsync(User user);
        Task<User?> GetByRefreshTokenAsync(string refreshToken);
        // FIX 2.4: Xóa UpdateAsync — EF Change Tracker tự detect changes
    }
}