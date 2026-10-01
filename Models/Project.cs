using System.ComponentModel.DataAnnotations;

namespace TaskTracker.Models
{
    public class Project
    {
        public int Id { get; set; }
        [MaxLength(250)]
        public string Name { get; set; } = null!;
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
        public string ClockifyId { get; set; } = string.Empty;

        public GetProjectDto toGetProjectDto()
        {
            return new GetProjectDto
            {
                Id = this.Id,
                Name = this.Name,
                ClockifyId = this.ClockifyId,
                Tasks = this.Tasks.Select(t => t.toGetTaskDto()).ToList()
            };
        }
    }

    public class CreateProjectDto
    {
        [MaxLength(250)]
        public string Name { get; set; } = null!;
        public Project toProject()
        {
            return new Project
            {
                Name = this.Name
            };
        }
    }
    public class UpdateProjectDto
    {
        [MaxLength(250)]
        public string Name { get; set; } = null!;
        public void update(Project project)
        {
            project.Name = this.Name;
        }
    }

    public class GetProjectDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string ClockifyId { get; set; } = string.Empty;
        public ICollection<GetTaskDto> Tasks { get; set; } = new List<GetTaskDto>();
    }
}
