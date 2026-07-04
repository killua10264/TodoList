using Microsoft.EntityFrameworkCore;
using TodoListBackend.Data;
using TodoListBackend.Models;
using TodoListBackend.DTOs.Todo; // Đừng quên using DTOs

namespace TodoListBackend.Repositories
{
    public class TodoRepository : ITodoRepository
    {
        private readonly AppDbContext _context;

        public TodoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TodoResponseDto>> GetAllTodosAsync(int userId)
        {
            return await _context.Todos
                .AsNoTracking()
                .Where(t => t.UserId == userId && !t.IsDeleted)
                .Select(t => new TodoResponseDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    IsCompleted = t.IsCompleted,
                    Priority = t.Priority,
                    DueDate = t.DueDate,
                    CategoryId = t.CategoryId,
                    CategoryName = t.Category != null ? t.Category.Name : string.Empty,
                    CategoryColor = t.Category != null ? t.Category.Color : string.Empty
                })
                .ToListAsync();
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

        public Task UpdateAsync(Todo todo)
        {
            _context.Todos.Update(todo);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}