using Microsoft.EntityFrameworkCore;
using TodoListBackend.Data;
using TodoListBackend.Models;

namespace TodoListBackend.Repositories
{
    public class RefreshTokenSessionRepository : IRefreshTokenSessionRepository
    {
        private readonly AppDbContext _context;

        public RefreshTokenSessionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<RefreshTokenSession?> GetByTokenHashAsync(
            string tokenHash,
            bool trackChanges = false)
        {
            var query = _context.RefreshTokenSessions
                .Include(session => session.User)
                .AsQueryable();

            if (!trackChanges)
            {
                query = query.AsNoTracking();
            }

            return await query.FirstOrDefaultAsync(session => session.TokenHash == tokenHash);
        }

        public async Task AddAsync(RefreshTokenSession session)
        {
            await _context.RefreshTokenSessions.AddAsync(session);
        }

        public async Task<List<RefreshTokenSession>> GetActiveByUserIdAsync(int userId)
        {
            return await _context.RefreshTokenSessions
                .Where(session => session.UserId == userId && session.RevokedAt == null)
                .ToListAsync();
        }
    }
}
