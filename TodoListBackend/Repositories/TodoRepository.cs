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

        public async Task<(IEnumerable<Todo> Items, int TotalCount)> GetAllTodosAsync(int userId, int page = 1, int pageSize = 20, string? filter = null, int? categoryId = null, string? status = null, string? sortBy = null, bool? isHidden = false, string? search = null, bool? isDeleted = false)
        {
            var query = _context.Todos
                .AsNoTracking()
                .Include(t => t.Category)
                .Where(t => t.UserId == userId);

            if (isDeleted.HasValue)
            {
                query = query.Where(t => t.IsDeleted == isDeleted.Value);
            }
            else
            {
                query = query.Where(t => !t.IsDeleted);
            }

            if (isHidden.HasValue)
            {
                query = query.Where(t => t.IsHidden == isHidden.Value);
            }

            if (!string.IsNullOrEmpty(filter))
            {
                var today = DateTime.UtcNow.Date;
                if (filter.ToLower() == "today")
                {
                    query = query.Where(t => t.DueDate.Date == today);
                }
                else if (filter.ToLower() == "upcoming")
                {
                    query = query.Where(t => t.DueDate.Date > today);
                }
            }

            // Tìm kiếm theo từ khóa trong Title và Description
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.Trim().ToLower();
                query = query.Where(t =>
                    t.Title.ToLower().Contains(searchLower) ||
                    t.Description.ToLower().Contains(searchLower)
                );
            }

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(t => t.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrEmpty(status) && status.ToLower() != "all")
            {
                if (status.ToLower() == "pending")
                {
                    query = query.Where(t => !t.IsCompleted);
                }
                else if (status.ToLower() == "completed")
                {
                    query = query.Where(t => t.IsCompleted);
                }
            }

            var totalCount = await query.CountAsync();

            if (!string.IsNullOrEmpty(sortBy))
            {
                if (sortBy.ToLower() == "duedate")
                {
                    query = query.OrderBy(t => t.DueDate).ThenByDescending(t => t.CreatedAt);
                }
                else if (sortBy.ToLower() == "prioritydesc")
                {
                    query = query.OrderByDescending(t => t.Priority).ThenByDescending(t => t.CreatedAt);
                }
                else
                {
                    query = query.OrderByDescending(t => t.CreatedAt);
                }
            }
            else
            {
                query = query.OrderByDescending(t => t.CreatedAt);
            }

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<Todo?> GetByIdAsync(int id, int userId, bool trackChanges = false, bool includeDeleted = false)
        {
            var query = _context.Todos.AsQueryable();
            
            if (!trackChanges) {
                query = query.AsNoTracking();
            }

            query = query.Include(t => t.Category).Where(t => t.Id == id && t.UserId == userId);

            if (!includeDeleted)
            {
                query = query.Where(t => !t.IsDeleted);
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task AddAsync(Todo todo)
        {
            await _context.Todos.AddAsync(todo);
        }

        public void Remove(Todo todo)
        {
            _context.Todos.Remove(todo);
        }
    }
}