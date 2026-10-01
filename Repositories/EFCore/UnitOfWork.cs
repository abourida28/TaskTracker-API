using TaskTracker.Models;
using TaskTracker.Repositories.Interfaces;

namespace TaskTracker.Repositories.EFCore
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IProjectRepository? _projects;
        private IUserRepository? _users;
        private ITaskRepository? _tasks;
        private ITrackedTimeRepository? _trackedTimes;
        private bool _disposed;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public IProjectRepository Projects => _projects ??= new ProjectRepository(_context);
        public IUserRepository Users => _users ??= new UserRepository(_context);
        public ITaskRepository Tasks => _tasks ??= new TaskRepository(_context);
        public ITrackedTimeRepository TrackedTimes => _trackedTimes ??= new TrackedTimeRepository(_context);

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
                _disposed = true;
            }
        }
    }
}
