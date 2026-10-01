using TaskTracker.Models;

namespace TaskTracker.Repositories.Interfaces
{
    public interface ITrackedTimeRepository : IRepository<TrackedTime>
    {
        Task<TrackedTime?> GetByIdWithTaskAndProjectAsync(int id);
    }
}
