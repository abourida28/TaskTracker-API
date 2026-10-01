using Microsoft.EntityFrameworkCore;
using TaskTracker.Models;
using TaskTracker.Repositories.Interfaces;

namespace TaskTracker.Repositories.EFCore
{
    public class TrackedTimeRepository : Repository<TrackedTime>, ITrackedTimeRepository
    {
        public TrackedTimeRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<TrackedTime?> GetByIdWithTaskAndProjectAsync(int id)
        {
            return await _context.TrackedTimes
                .Include(tt => tt.Task)
                    .ThenInclude(t => t!.Project)
                .FirstOrDefaultAsync(tt => tt.Id == id);
        }
    }
}
