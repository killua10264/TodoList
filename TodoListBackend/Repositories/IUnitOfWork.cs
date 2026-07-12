namespace TodoListBackend.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        ITodoRepository Todos { get; }
        IProjectRepository Projects { get; }
        IUserRepository Users { get; }
        Task<int> SaveChangesAsync();
    }
}
