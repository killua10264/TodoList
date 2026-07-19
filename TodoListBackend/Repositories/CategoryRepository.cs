using TodoListBackend.Models;
using TodoListBackend.Data;
using Microsoft.EntityFrameworkCore;

namespace TodoListBackend.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly AppDbContext _context;

        public CategoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync(int userId)
        {
            return await _context.Categories
                .AsNoTracking()
                .Include(p => p.Todos)
                .Where(p => p.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetAllLightAsync(int userId)
        {
            return await _context.Categories
                .AsNoTracking()
                .Where(p => p.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<TodoListBackend.DTOs.Category.CategoryResponseDto>> GetAllCategoriesProjectedAsync(int userId)
        {
            return await _context.Categories
                .AsNoTracking()
                .Where(p => p.UserId == userId)
                .Select(p => new TodoListBackend.DTOs.Category.CategoryResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Color = p.Color,
                    TodoCount = p.Todos.Count(t => !t.IsDeleted),
                    CompletedTodoCount = p.Todos.Count(t => t.IsCompleted && !t.IsDeleted)
                })
                .ToListAsync();
        }

        public async Task<Category?> GetByIdAsync(int id, int userId, bool trackChanges = false)
        {
            var query = _context.Categories.AsQueryable();

            if (!trackChanges){
                query = query.AsNoTracking();
            }

            return await query
                .Include(p => p.Todos)
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
        }

        public async Task<Category?> GetByNameAsync(string name, int userId)
        {
            return await _context.Categories
                .FirstOrDefaultAsync(p => p.Name.ToLower() == name.ToLower() && p.UserId == userId);
        }

        public async Task<bool> ExistsByNameAsync(string name, int userId, int? excludeId = null)
        {
            var query = _context.Categories
                .Where(p => p.Name.ToLower() == name.ToLower() && p.UserId == userId);

            if (excludeId.HasValue)
            {
                query = query.Where(p => p.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task AddAsync(Category category)
        {
            await _context.Categories.AddAsync(category);
        }

        public Task DeleteAsync(Category category)
        {
            _context.Categories.Remove(category);
            return Task.CompletedTask;
        }
    }
}
