namespace TaskTracker.Services
{
    public record RemoteUserDto(string Id, string Name);

    public interface IRemoteService
    {
        Task<string> CreateProjectAsync(string name);
        Task UpdateProjectAsync(string remoteProjectId, string name);
        Task<List<RemoteUserDto>> GetUsersAsync();
        Task<string> CreateTaskAsync(string remoteProjectId, string taskName, string remoteUserId, decimal estimatedHours);
        Task UpdateTaskAsync(string remoteProjectId, string remoteTaskId, string taskName, string remoteUserId, decimal estimatedHours);
        Task<string> CreateTimeEntryAsync(string remoteProjectId, string remoteTaskId, string description, DateTime start, DateTime? end);
        Task UpdateTimeEntryAsync(string remoteTimeEntryId, string remoteProjectId, string remoteTaskId, string description, DateTime start, DateTime? end);
    }
}
