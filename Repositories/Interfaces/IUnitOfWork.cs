namespace TaskTracker.Repositories.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IProjectRepository Projects { get; }
        IUserRepository Users { get; }
        ITaskRepository Tasks { get; }
        ITrackedTimeRepository TrackedTimes { get; }
        Task<int> SaveChangesAsync();
    }
}
