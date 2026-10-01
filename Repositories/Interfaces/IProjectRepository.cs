using TaskTracker.Models;

namespace TaskTracker.Repositories.Interfaces
{
    public interface IProjectRepository : IRepository<Project>
    {
        Task<IEnumerable<Project>> GetAllWithDetailsAsync();
        Task<Project?> GetByIdWithDetailsAsync(int id);
    }
}
