using System.ComponentModel.DataAnnotations;

namespace TaskTracker.Models
{
    public class TaskItem
    {
        public int Id { get; set; }
        [MaxLength(1000)]
        public string Name { get; set; } = null!;
        public int? UserId { get; set; }
        public User? User { get; set; } = null!;
        public int? ProjectId { get; set; }
        public Project? Project { get; set; } = null!;
        public decimal EstimatedTime { get; set; } // in hours
        public ICollection<TrackedTime> TrackedTimes { get; set; } = new List<TrackedTime>();
        public string ClockifyId { get; set; } = string.Empty;

        public GetTaskDto toGetTaskDto()
        {
            return new GetTaskDto
            {
                Id = this.Id,
                Name = this.Name,
                EstimatedTime = this.EstimatedTime,
                UserId = this.UserId,
                ProjectId = this.ProjectId,
                ClockifyId = this.ClockifyId,
                TrackedTimes = this.TrackedTimes.Select(tt => tt.toGetTrackedTimeDto()).ToList()
            };
        }
    }
    public class GetTaskDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal EstimatedTime { get; set; }
        public int? UserId { get; set; }
        public int? ProjectId { get; set; }
        public string ClockifyId { get; set; } = string.Empty;
        public ICollection<GetTrackedTimeDto> TrackedTimes { get; set; } = new List<GetTrackedTimeDto>();
    }

    public class CreateTaskDto
    {
        [MaxLength(1000)]
        public string Name { get; set; } = null!;
        public int UserId { get; set; }
        public int ProjectId { get; set; }
        public decimal EstimatedTime { get; set; }
        public TaskItem toTask()
        {
            return new TaskItem
            {
                Name = this.Name,
                EstimatedTime = this.EstimatedTime,
                UserId = this.UserId,
                ProjectId = this.ProjectId
            };
        }
    }

    public class UpdateTaskDto
    {
        [MaxLength(1000)]
        public string Name { get; set; } = null!;
        public decimal EstimatedTime { get; set; }
        public int UserId { get; set; }
        public void applyToTask(TaskItem task)
        {
            task.Name = this.Name;
            task.EstimatedTime = this.EstimatedTime;
            task.UserId = this.UserId;
        }
    }
}
