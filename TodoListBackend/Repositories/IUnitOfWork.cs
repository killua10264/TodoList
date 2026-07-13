namespace TodoListBackend.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        ITodoRepository Todos { get; }
        IProjectRepository Projects { get; }
        IUserRepository Users { get; }
        ISubTaskRepository SubTasks { get; }
        Task<int> SaveChangesAsync();
    }
}
