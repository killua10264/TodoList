using Microsoft.EntityFrameworkCore;
using TodoListBackend.Data;
using TodoListBackend.Models;

namespace TodoListBackend.Repositories
{
    public class TodoRepository : ITodoRepository
    {
        private readonly AppDbContext _context;

        public TodoRepository(AppDbContext context)
        {
            _context = context;
        }

        // FIX 2.2: Pagination — chỉ load một trang kết quả
        public async Task<(IEnumerable<Todo> Items, int TotalCount)> GetAllTodosAsync(int userId, int page = 1, int pageSize = 20)
        {
            var query = _context.Todos
                .AsNoTracking()
                .Include(t => t.Category)
                .Where(t => t.UserId == userId && !t.IsDeleted);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<Todo?> GetByIdAsync(int id, int userId)
        {
            return await _context.Todos
                .Include(t => t.Category)
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId && !t.IsDeleted);
        }

        public async Task AddAsync(Todo todo)
        {
            await _context.Todos.AddAsync(todo);
        }

        // FIX 2.4: Xóa UpdateAsync() — EF Core Change Tracker tự detect changes
        // Khi entity được load bởi GetByIdAsync (có tracking), chỉ cần sửa property
        // rồi gọi SaveChangesAsync() → EF chỉ UPDATE cột thực sự thay đổi.
    }
}