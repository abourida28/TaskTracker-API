using System.ComponentModel.DataAnnotations;
namespace TaskTracker.Models
{
    public class User
    {
        public int Id { get; set; }
        [MaxLength(100)]
        public string Name { get; set; } = null!;
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
        public string ClockifyId { get; set; } = string.Empty;
        public GetUserDto toGetUserDto()
        {
            return new GetUserDto
            {
                Id = this.Id,
                Name = this.Name,
                ClockifyId = this.ClockifyId,
                Tasks = this.Tasks.Select(t => t.toGetTaskDto()).ToList()
            };
        }
    }

    public class UpdateUserDto
    {
        [MaxLength(100)]
        public string Name { get; set; } = null!;
        public void applyToUser(User user)
        {
            user.Name = this.Name;
        }
    }

    public class GetUserDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string ClockifyId { get; set; } = string.Empty;
        public ICollection<GetTaskDto> Tasks { get; set; } = new List<GetTaskDto>();
    }
}
