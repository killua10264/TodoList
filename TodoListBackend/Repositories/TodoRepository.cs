using Microsoft.EntityFrameworkCore;
using TodoListBackend.Data;
using TodoListBackend.Models;
using TodoListBackend.DTOs.Todo;

namespace TodoListBackend.Repositories
{
    public class TodoRepository : ITodoRepository
    {
        private readonly AppDbContext _context;

        public TodoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<Todo> Items, int TotalCount)> GetAllTodosAsync(int userId, TodoQueryDto queryOptions)
        {
            var query = _context.Todos
                .AsNoTracking();

            if (queryOptions.IsDeleted == true)
            {
                query = query.IgnoreQueryFilters();
            }

            query = query
                .Include(t => t.Category)
                .Where(t => t.UserId == userId);

            if (queryOptions.IsDeleted.HasValue)
            {
                query = query.Where(t => t.IsDeleted == queryOptions.IsDeleted.Value);
            }
            else
            {
                query = query.Where(t => !t.IsDeleted);
            }

            if (queryOptions.IsHidden.HasValue)
            {
                query = query.Where(t => t.IsHidden == queryOptions.IsHidden.Value);
            }

            if (queryOptions.Filter.HasValue)
            {
                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                if (queryOptions.Filter == TodoFilter.Today)
                {
                    query = query.Where(t => t.DueDate == today);
                }
                else if (queryOptions.Filter == TodoFilter.Upcoming)
                {
                    query = query.Where(t => t.DueDate > today);
                }
            }

            // Tìm kiếm theo từ khóa trong Title và Description
            if (!string.IsNullOrWhiteSpace(queryOptions.Search))
            {
                var searchLower = queryOptions.Search.Trim().ToLower();
                query = query.Where(t =>
                    t.Title.ToLower().Contains(searchLower) ||
                    t.Description.ToLower().Contains(searchLower)
                );
            }

            if (queryOptions.CategoryId.HasValue && queryOptions.CategoryId.Value > 0)
            {
                query = query.Where(t => t.CategoryId == queryOptions.CategoryId.Value);
            }

            if (queryOptions.Status.HasValue && queryOptions.Status != TodoStatus.All)
            {
                if (queryOptions.Status == TodoStatus.Pending)
                {
                    query = query.Where(t => !t.IsCompleted);
                }
                else if (queryOptions.Status == TodoStatus.Completed)
                {
                    query = query.Where(t => t.IsCompleted);
                }
            }

            var totalCount = await query.CountAsync();

            if (queryOptions.SortBy.HasValue)
            {
                if (queryOptions.SortBy == TodoSortBy.DueDate)
                {
                    query = query.OrderBy(t => t.DueDate).ThenByDescending(t => t.CreatedAt);
                }
                else if (queryOptions.SortBy == TodoSortBy.PriorityDesc)
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
                .Skip((queryOptions.Page - 1) * queryOptions.PageSize)
                .Take(queryOptions.PageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<Todo?> GetByIdAsync(int id, int userId, bool trackChanges = false, bool includeDeleted = false)
        {
            var query = _context.Todos.AsQueryable();

            if (includeDeleted)
            {
                query = query.IgnoreQueryFilters();
            }
            
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
