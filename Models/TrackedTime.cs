namespace TaskTracker.Models
{
    public class TrackedTime
    {
        public int Id { get; set; }
        public int? TaskId { get; set; }
        public TaskItem? Task { get; set; } = null!;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string ClockifyId { get; set; } = string.Empty;
        public GetTrackedTimeDto toGetTrackedTimeDto()
        {
            return new GetTrackedTimeDto
            {
                Id = this.Id,
                TaskId = this.TaskId,
                StartTime = this.StartTime,
                EndTime = this.EndTime,
                ClockifyId = this.ClockifyId
            };
        }
    }

    public class GetTrackedTimeDto
    {
        public int Id { get; set; }
        public int? TaskId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string ClockifyId { get; set; } = string.Empty;
    }
    
    public class CreateTrackedTimeDto
    {
        public int TaskId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public TrackedTime toTrackedTime()
        {
            return new TrackedTime
            {
                TaskId = this.TaskId,
                StartTime = this.StartTime,
                EndTime = this.EndTime
            };
        }
    }

    public class UpdateTrackedTimeDto
    {
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public void applyToTrackedTime(TrackedTime trackedTime)
        {
            trackedTime.StartTime = this.StartTime;
            trackedTime.EndTime = this.EndTime;
        }
    }
}
