using TodoListBackend.Models;

namespace TodoListBackend.Repositories
{
    public interface IRefreshTokenSessionRepository
    {
        Task<RefreshTokenSession?> GetByTokenHashAsync(string tokenHash, bool trackChanges = false);
        Task AddAsync(RefreshTokenSession session);
        Task<List<RefreshTokenSession>> GetActiveByUserIdAsync(int userId);
    }
}
