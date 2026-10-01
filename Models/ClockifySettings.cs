namespace TaskTracker.Models
{
    public class ClockifySettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string WorkspaceId { get; set; } = string.Empty;
    }

    public class ClockifyProjectResponse
    {
        public string Id { get; set; } = string.Empty;
    }

    public class ClockifyUserResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public class ClockifyTaskResponse
    {
        public string Id { get; set; } = string.Empty;
    }

    public class ClockifyTimeEntryResponse
    {
        public string Id { get; set; } = string.Empty;
    }
}
