using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskTracker.Models;
using TaskTracker.Services;
using TaskTracker.Repositories.Interfaces;

[Route("api/[controller]")]
[ApiController]
public class ProjectsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRemoteService _remoteService;

    public ProjectsController(IUnitOfWork unitOfWork, IRemoteService remoteService)
    {
        _unitOfWork = unitOfWork;
        _remoteService = remoteService;
    }

    // GET: api/Project
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetProjectDto>>> GetProject()
    {
        var projects = await _unitOfWork.Projects.GetAllWithDetailsAsync();
        return Ok(projects.Select(p => p.toGetProjectDto()));
    }

    // GET: api/Project/5
    [HttpGet("{id}")]
    public async Task<ActionResult<GetProjectDto>> GetProject(int id)
    {
        var project = await _unitOfWork.Projects.GetByIdWithDetailsAsync(id);
        if (project == null)
        {
            return NotFound();
        }

        return project.toGetProjectDto();
    }

    // PUT: api/Project/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutProject(int? id, UpdateProjectDto updateProjectDto)
    {
        if (!id.HasValue)
        {
            return BadRequest();
        }

        var projectEntity = await _unitOfWork.Projects.GetByIdAsync(id.Value);
        if (projectEntity == null)
        {
            return NotFound();
        }

        if (!string.IsNullOrEmpty(projectEntity.ClockifyId))
        {
            await _remoteService.UpdateProjectAsync(projectEntity.ClockifyId, updateProjectDto.Name);
        }

        updateProjectDto.update(projectEntity);
        _unitOfWork.Projects.Update(projectEntity);

        try
        {
            await _unitOfWork.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _unitOfWork.Projects.ExistsAsync(id.Value))
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

    // POST: api/Project
    [HttpPost]
    public async Task<ActionResult<GetProjectDto>> PostProject(CreateProjectDto project)
    {
        var remoteProjectId = await _remoteService.CreateProjectAsync(project.Name);

        var projectEntity = project.toProject();
        projectEntity.ClockifyId = remoteProjectId;

        await _unitOfWork.Projects.AddAsync(projectEntity);
        await _unitOfWork.SaveChangesAsync();

        return CreatedAtAction("GetProject", new { id = projectEntity.Id }, projectEntity.toGetProjectDto());
    }

    // DELETE: api/Project/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProject(int? id)
    {
        if (!id.HasValue)
        {
            return BadRequest();
        }

        var project = await _unitOfWork.Projects.GetByIdAsync(id.Value);
        if (project == null)
        {
            return NotFound();
        }

        _unitOfWork.Projects.Delete(project);
        await _unitOfWork.SaveChangesAsync();

        return NoContent();
    }
}
