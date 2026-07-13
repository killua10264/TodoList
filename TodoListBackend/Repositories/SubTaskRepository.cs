using TodoListBackend.Models;
using TodoListBackend.Data;
using Microsoft.EntityFrameworkCore;

namespace TodoListBackend.Repositories
{
    public class SubTaskRepository : ISubTaskRepository
    {
        private readonly AppDbContext _context;

        public SubTaskRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SubTask>> GetByTodoIdAsync(int todoId, int userId)
        {
            return await _context.SubTasks
                .Include(s => s.Todo)
                .Where(s => s.TodoId == todoId && s.Todo.UserId == userId && !s.Todo.IsDeleted)
                .OrderBy(s => s.SortOrder)
                .ThenBy(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<SubTask?> GetByIdAsync(int id, int userId)
        {
            return await _context.SubTasks
                .Include(s => s.Todo)
                .FirstOrDefaultAsync(s => s.Id == id && s.Todo.UserId == userId && !s.Todo.IsDeleted);
        }

        public async Task AddAsync(SubTask subTask)
        {
            await _context.SubTasks.AddAsync(subTask);
        }

        public Task DeleteAsync(SubTask subTask)
        {
            _context.SubTasks.Remove(subTask);
            return Task.CompletedTask;
        }
    }
}
