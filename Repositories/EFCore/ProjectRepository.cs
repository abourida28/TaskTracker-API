using Microsoft.EntityFrameworkCore;
using TaskTracker.Models;
using TaskTracker.Repositories.Interfaces;

namespace TaskTracker.Repositories.EFCore
{
    public class ProjectRepository : Repository<Project>, IProjectRepository
    {
        public ProjectRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Project>> GetAllWithDetailsAsync()
        {
            return await _context.Projects
                .AsNoTracking()
                .Include(p => p.Tasks)
                .ThenInclude(t => t.TrackedTimes)
                .ToListAsync();
        }

        public async Task<Project?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Projects
                .AsNoTracking()
                .Include(p => p.Tasks)
                .ThenInclude(t => t.TrackedTimes)
                .FirstOrDefaultAsync(p => p.Id == id);
        }
    }
}
