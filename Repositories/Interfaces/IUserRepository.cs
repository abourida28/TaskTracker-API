using TaskTracker.Models;

namespace TaskTracker.Repositories.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<IEnumerable<User>> GetAllWithDetailsAsync();
        Task<User?> GetByIdWithDetailsAsync(int id);
        Task AddRangeAsync(IEnumerable<User> users);
    }
}
