using Microsoft.EntityFrameworkCore;
using TaskTracker.Models;
using TaskTracker.Repositories.Interfaces;

namespace TaskTracker.Repositories.EFCore
{
    public class TaskRepository : Repository<TaskItem>, ITaskRepository
    {
        public TaskRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<TaskItem>> GetAllWithDetailsAsync()
        {
            return await _context.Tasks
                .AsNoTracking()
                .Include(t => t.TrackedTimes)
                .ToListAsync();
        }

        public async Task<TaskItem?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Tasks
                .AsNoTracking()
                .Include(t => t.TrackedTimes)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<TaskItem?> GetByIdWithProjectAsync(int id)
        {
            return await _context.Tasks
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<TaskItem>> GetAllForReportAsync()
        {
            return await _context.Tasks
                .AsNoTracking()
                .Include(t => t.User)
                .Include(t => t.Project)
                .Include(t => t.TrackedTimes)
                .ToListAsync();
        }
    }
}
