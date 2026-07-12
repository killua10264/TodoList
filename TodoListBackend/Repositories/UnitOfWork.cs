using TodoListBackend.Data;

namespace TodoListBackend.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public ITodoRepository Todos { get; }
        public IProjectRepository Projects { get; }
        public IUserRepository Users { get; }

        public UnitOfWork(
            AppDbContext context,
            ITodoRepository todoRepository,
            IProjectRepository projectRepository,
            IUserRepository userRepository)
        {
            _context = context;
            Todos = todoRepository;
            Projects = projectRepository;
            Users = userRepository;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
