using TaskTracker.Models;

namespace TaskTracker.Repositories.Interfaces
{
    public interface ITaskRepository : IRepository<TaskItem>
    {
        Task<IEnumerable<TaskItem>> GetAllWithDetailsAsync();
        Task<TaskItem?> GetByIdWithDetailsAsync(int id);
        Task<TaskItem?> GetByIdWithProjectAsync(int id);
        Task<IEnumerable<TaskItem>> GetAllForReportAsync();
    }
}
