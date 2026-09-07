namespace TodoListBackend.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        ITodoRepository Todos { get; }
        ICategoryRepository Categories { get; }
        IUserRepository Users { get; }
        ISubTaskRepository SubTasks { get; }
        IRefreshTokenSessionRepository RefreshTokenSessions { get; }
        Task<int> SaveChangesAsync();
    }
}
