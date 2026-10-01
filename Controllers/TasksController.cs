using System.Text;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskTracker.Models;
using TaskTracker.Services;
using TaskTracker.Repositories.Interfaces;

[Route("api/[controller]")]
[ApiController]
public class TasksController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRemoteService _remoteService;

    public TasksController(IUnitOfWork unitOfWork, IRemoteService remoteService)
    {
        _unitOfWork = unitOfWork;
        _remoteService = remoteService;
    }

    // GET: api/Tasks
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetTaskDto>>> GetTask()
    {
        var tasks = await _unitOfWork.Tasks.GetAllWithDetailsAsync();
        return Ok(tasks.Select(t => t.toGetTaskDto()));
    }

    // GET: api/Tasks/export-csv
    [HttpGet("export-csv")]
    public async Task<IActionResult> ExportCsv()
    {
        var tasks = await _unitOfWork.Tasks.GetAllForReportAsync();

        var sb = new StringBuilder();
        sb.AppendLine("User,Project,Task,OriginalEstimate (hr),TimeSpent (hr)");

        foreach (var task in tasks)
        {
            var userName = task.User?.Name ?? string.Empty;
            var projectName = task.Project?.Name ?? string.Empty;
            var taskName = task.Name ?? string.Empty;
            var originalEstimate = task.EstimatedTime.ToString("0.##", CultureInfo.InvariantCulture);

            var totalHoursSpent = task.TrackedTimes
                .Where(tt => tt.EndTime.HasValue && tt.EndTime.Value >= tt.StartTime)
                .Sum(tt => (tt.EndTime!.Value - tt.StartTime).TotalHours);
            var timeSpent = totalHoursSpent.ToString("0.00", CultureInfo.InvariantCulture);

            sb.AppendLine($"{EscapeCsv(userName)},{EscapeCsv(projectName)},{EscapeCsv(taskName)},{originalEstimate},{timeSpent}");
        }

        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        return File(bytes, "text/csv", "tasks_report.csv");
    }

    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        if (value.Contains(',') || value.Contains('"') || value.Contains('\r') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
        return value;
    }

    // GET: api/Task/5
    [HttpGet("{id}")]
    public async Task<ActionResult<GetTaskDto>> GetTask(int id)
    {
        var task = await _unitOfWork.Tasks.GetByIdWithDetailsAsync(id);
        if (task == null)
        {
            return NotFound();
        }

        return task.toGetTaskDto();
    }

    // PUT: api/Task/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutTask(int? id, UpdateTaskDto task)
    {
        if (!id.HasValue)
        {
            return BadRequest();
        }

        var taskEntity = await _unitOfWork.Tasks.GetByIdWithProjectAsync(id.Value);
        if (taskEntity == null)
        {
            return NotFound();
        }

        var user = await _unitOfWork.Users.GetByIdAsync(task.UserId);
        if (user == null)
        {
            return BadRequest($"User with ID {task.UserId} not found.");
        }

        var project = taskEntity.Project ?? (taskEntity.ProjectId.HasValue ? await _unitOfWork.Projects.GetByIdAsync(taskEntity.ProjectId.Value) : null);
        if (!string.IsNullOrEmpty(taskEntity.ClockifyId) && project != null && !string.IsNullOrEmpty(project.ClockifyId))
        {
            if (string.IsNullOrEmpty(user.ClockifyId))
            {
                return BadRequest($"User '{user.Name}' does not have an associated Clockify ID.");
            }

            await _remoteService.UpdateTaskAsync(project.ClockifyId, taskEntity.ClockifyId, task.Name, user.ClockifyId, task.EstimatedTime);
        }

        task.applyToTask(taskEntity);
        _unitOfWork.Tasks.Update(taskEntity);

        try
        {
            await _unitOfWork.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _unitOfWork.Tasks.ExistsAsync(id.Value))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // POST: api/Task
    [HttpPost]
    public async Task<ActionResult<GetTaskDto>> PostTask(CreateTaskDto task)
    {
        var project = await _unitOfWork.Projects.GetByIdAsync(task.ProjectId);
        if (project == null)
        {
            return BadRequest($"Project with ID {task.ProjectId} not found.");
        }
        if (string.IsNullOrEmpty(project.ClockifyId))
        {
            return BadRequest($"Project '{project.Name}' does not have an associated Clockify ID.");
        }

        var user = await _unitOfWork.Users.GetByIdAsync(task.UserId);
        if (user == null)
        {
            return BadRequest($"User with ID {task.UserId} not found.");
        }
        if (string.IsNullOrEmpty(user.ClockifyId))
        {
            return BadRequest($"User '{user.Name}' does not have an associated Clockify ID.");
        }

        var remoteTaskId = await _remoteService.CreateTaskAsync(project.ClockifyId, task.Name, user.ClockifyId, task.EstimatedTime);

        var taskEntity = task.toTask();
        taskEntity.ClockifyId = remoteTaskId;

        await _unitOfWork.Tasks.AddAsync(taskEntity);
        await _unitOfWork.SaveChangesAsync();

        return CreatedAtAction("GetTask", new { id = taskEntity.Id }, taskEntity.toGetTaskDto());
    }

    // DELETE: api/Task/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(int? id)
    {
        if (!id.HasValue)
        {
            return BadRequest();
        }

        var task = await _unitOfWork.Tasks.GetByIdAsync(id.Value);
        if (task == null)
        {
            return NotFound();
        }

        _unitOfWork.Tasks.Delete(task);
        await _unitOfWork.SaveChangesAsync();

        return NoContent();
    }
}
