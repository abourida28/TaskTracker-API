using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskTracker.Models;
using TaskTracker.Services;
using TaskTracker.Repositories.Interfaces;

[Route("api/[controller]")]
[ApiController]
public class TrackedTimesController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRemoteService _remoteService;

    public TrackedTimesController(IUnitOfWork unitOfWork, IRemoteService remoteService)
    {
        _unitOfWork = unitOfWork;
        _remoteService = remoteService;
    }

    // GET: api/TrackedTime
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TrackedTime>>> GetTrackedTime()
    {
        var trackedTimes = await _unitOfWork.TrackedTimes.GetAllAsync();
        return Ok(trackedTimes);
    }

    // PUT: api/TrackedTime/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutTrackedTime(int? id, UpdateTrackedTimeDto updateTrackedTimeDto)
    {
        if (!id.HasValue)
        {
            return BadRequest();
        }

        var trackedtime = await _unitOfWork.TrackedTimes.GetByIdWithTaskAndProjectAsync(id.Value);
        if (trackedtime == null)
        {
            return NotFound();
        }

        if (!string.IsNullOrEmpty(trackedtime.ClockifyId))
        {
            var task = trackedtime.Task ?? (trackedtime.TaskId.HasValue ? await _unitOfWork.Tasks.GetByIdWithProjectAsync(trackedtime.TaskId.Value) : null);
            var project = task?.Project ?? (task?.ProjectId.HasValue == true ? await _unitOfWork.Projects.GetByIdAsync(task.ProjectId.Value) : null);

            await _remoteService.UpdateTimeEntryAsync(
                trackedtime.ClockifyId,
                project?.ClockifyId ?? string.Empty,
                task?.ClockifyId ?? string.Empty,
                task?.Name ?? string.Empty,
                updateTrackedTimeDto.StartTime,
                updateTrackedTimeDto.EndTime);
        }

        updateTrackedTimeDto.applyToTrackedTime(trackedtime);
        _unitOfWork.TrackedTimes.Update(trackedtime);

        try
        {
            await _unitOfWork.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _unitOfWork.TrackedTimes.ExistsAsync(id.Value))
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

    // POST: api/TrackedTime
    [HttpPost]
    public async Task<ActionResult<GetTrackedTimeDto>> PostTrackedTime(CreateTrackedTimeDto createTrackedTimeDto)
    {
        var task = await _unitOfWork.Tasks.GetByIdWithProjectAsync(createTrackedTimeDto.TaskId);
        if (task == null)
        {
            return BadRequest($"Task with ID {createTrackedTimeDto.TaskId} not found.");
        }
        if (string.IsNullOrEmpty(task.ClockifyId))
        {
            return BadRequest($"Task '{task.Name}' does not have an associated Clockify ID.");
        }

        var project = task.Project ?? (task.ProjectId.HasValue ? await _unitOfWork.Projects.GetByIdAsync(task.ProjectId.Value) : null);
        if (project == null || string.IsNullOrEmpty(project.ClockifyId))
        {
            return BadRequest("Task's associated project does not have a valid Clockify ID.");
        }

        var remoteTimeEntryId = await _remoteService.CreateTimeEntryAsync(
            project.ClockifyId,
            task.ClockifyId,
            task.Name,
            createTrackedTimeDto.StartTime,
            createTrackedTimeDto.EndTime);

        var trackedtime = createTrackedTimeDto.toTrackedTime();
        trackedtime.ClockifyId = remoteTimeEntryId;

        await _unitOfWork.TrackedTimes.AddAsync(trackedtime);
        await _unitOfWork.SaveChangesAsync();

        return CreatedAtAction("GetTrackedTime", new { id = trackedtime.Id }, trackedtime.toGetTrackedTimeDto());
    }

    // DELETE: api/TrackedTime/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTrackedTime(int? id)
    {
        if (!id.HasValue)
        {
            return BadRequest();
        }

        var trackedtime = await _unitOfWork.TrackedTimes.GetByIdAsync(id.Value);
        if (trackedtime == null)
        {
            return NotFound();
        }

        _unitOfWork.TrackedTimes.Delete(trackedtime);
        await _unitOfWork.SaveChangesAsync();

        return NoContent();
    }
}
