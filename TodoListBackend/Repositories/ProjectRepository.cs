using TodoListBackend.Models;
using TodoListBackend.Data;
using Microsoft.EntityFrameworkCore;

namespace TodoListBackend.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly AppDbContext _context;

        public ProjectRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Project>> GetAllProjectsAsync(int userId)
        {
            return await _context.Projects
                .AsNoTracking()
                .Where(p => p.UserId == userId)
                .ToListAsync();
        }

        public async Task<Project?> GetByIdAsync(int id, int userId)
        {
            return await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
        }

        public async Task<Project?> GetByNameAsync(string name)
        {
            return await _context.Projects.FirstOrDefaultAsync(p => p.Name == name);
        }

        public async Task AddAsync(Project project)
        {
            await _context.Projects.AddAsync(project);
        }

        public Task DeleteAsync(Project project)
        {
            _context.Projects.Remove(project);
            return Task.CompletedTask;
        }
    }
}
