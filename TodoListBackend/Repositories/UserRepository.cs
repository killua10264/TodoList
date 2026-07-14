using TodoListBackend.Models;
using TodoListBackend.Data;
using Microsoft.EntityFrameworkCore;

namespace TodoListBackend.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        // FIX 2.4: Xóa UpdateAsync — EF Change Tracker tự detect changes

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            var lower = username.ToLower();
            return await _context.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == lower);
        }

        public async Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail)
        {
            var lower = usernameOrEmail.ToLower();
            return await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == lower || u.Username.ToLower() == lower);
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            var lower = email.ToLower();
            return await _context.Users.AnyAsync(u => u.Email.ToLower() == lower);
        }

        public async Task<bool> ExistsByUsernameAsync(string username)
        {
            var lower = username.ToLower();
            return await _context.Users.AnyAsync(u => u.Username.ToLower() == lower);
        }

        // FIX 3.7: Check email/username tồn tại nhưng loại trừ user hiện tại (cho update profile)
        public async Task<bool> ExistsByEmailAsync(string email, int excludeUserId)
        {
            var lower = email.ToLower();
            return await _context.Users.AnyAsync(u => u.Email.ToLower() == lower && u.Id != excludeUserId);
        }

        public async Task<bool> ExistsByUsernameAsync(string username, int excludeUserId)
        {
            var lower = username.ToLower();
            return await _context.Users.AnyAsync(u => u.Username.ToLower() == lower && u.Id != excludeUserId);
        }

        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
        }

        public async Task<User?> GetByRefreshTokenAsync(string refreshToken)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
        }
    }
}