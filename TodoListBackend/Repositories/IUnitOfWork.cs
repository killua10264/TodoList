namespace TodoListBackend.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        ITodoRepository Todos { get; }
        ICategoryRepository Categories { get; }
        IUserRepository Users { get; }
        ISubTaskRepository SubTasks { get; }
        Task<int> SaveChangesAsync();
    }
}
