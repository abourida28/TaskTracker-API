using Microsoft.EntityFrameworkCore;
using TaskTracker.Models;
using TaskTracker.Repositories.Interfaces;

namespace TaskTracker.Repositories.EFCore
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<User>> GetAllWithDetailsAsync()
        {
            return await _context.Users
                .AsNoTracking()
                .Include(u => u.Tasks)
                .ThenInclude(t => t.TrackedTimes)
                .ToListAsync();
        }

        public async Task<User?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Users
                .AsNoTracking()
                .Include(u => u.Tasks)
                .ThenInclude(t => t.TrackedTimes)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task AddRangeAsync(IEnumerable<User> users)
        {
            await _context.Users.AddRangeAsync(users);
        }
    }
}
