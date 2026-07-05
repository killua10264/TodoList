using TodoListBackend.Data;

namespace TodoListBackend.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public ITodoRepository Todos { get; }
        public ICategoryRepository Categories { get; }
        public IUserRepository Users { get; }

        public UnitOfWork(
            AppDbContext context,
            ITodoRepository todoRepository,
            ICategoryRepository categoryRepository,
            IUserRepository userRepository)
        {
            _context = context;
            Todos = todoRepository;
            Categories = categoryRepository;
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
