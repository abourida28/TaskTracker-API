using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskTracker.Models;
using TaskTracker.Services;
using TaskTracker.Repositories.Interfaces;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRemoteService _remoteService;

    public UsersController(IUnitOfWork unitOfWork, IRemoteService remoteService)
    {
        _unitOfWork = unitOfWork;
        _remoteService = remoteService;
    }

    // GET: api/User
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetUserDto>>> GetUser()
    {
        var users = await _unitOfWork.Users.GetAllWithDetailsAsync();
        return Ok(users.Select(u => u.toGetUserDto()));
    }

    // GET: api/User/5
    [HttpGet("{id}")]
    public async Task<ActionResult<GetUserDto>> GetUser(int id)
    {
        var user = await _unitOfWork.Users.GetByIdWithDetailsAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        return user.toGetUserDto();
    }

    // PUT: api/User/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutUser(int? id, UpdateUserDto updateUserDto)
    {
        if (!id.HasValue)
        {
            return BadRequest();
        }

        var user = await _unitOfWork.Users.GetByIdAsync(id.Value);
        if (user == null)
        {
            return NotFound();
        }

        updateUserDto.applyToUser(user);
        _unitOfWork.Users.Update(user);

        try
        {
            await _unitOfWork.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _unitOfWork.Users.ExistsAsync(id.Value))
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

    // POST: api/Users/sync or POST: api/Users
    [HttpPost("sync")]
    [HttpPost]
    public async Task<ActionResult<IEnumerable<GetUserDto>>> SyncUsers()
    {
        var clockifyUsers = await _remoteService.GetUsersAsync();
        if (clockifyUsers == null)
        {
            return StatusCode(StatusCodes.Status502BadGateway, "Failed to retrieve users from Clockify.");
        }

        var existingUsers = (await _unitOfWork.Users.GetAllAsync()).ToList();
        var existingClockifyIds = existingUsers
            .Where(u => !string.IsNullOrEmpty(u.ClockifyId))
            .Select(u => u.ClockifyId)
            .ToHashSet();

        var usersToAdd = new List<User>();

        foreach (var clockifyUser in clockifyUsers)
        {
            if (string.IsNullOrEmpty(clockifyUser.Id))
            {
                continue;
            }

            if (!existingClockifyIds.Contains(clockifyUser.Id))
            {
                var newUser = new User
                {
                    Name = clockifyUser.Name,
                    ClockifyId = clockifyUser.Id
                };
                usersToAdd.Add(newUser);
                existingClockifyIds.Add(clockifyUser.Id);
            }
            else
            {
                var existingUser = existingUsers.FirstOrDefault(u => u.ClockifyId == clockifyUser.Id);
                if (existingUser != null && existingUser.Name != clockifyUser.Name && !string.IsNullOrEmpty(clockifyUser.Name))
                {
                    existingUser.Name = clockifyUser.Name;
                    _unitOfWork.Users.Update(existingUser);
                }
            }
        }

        if (usersToAdd.Any())
        {
            await _unitOfWork.Users.AddRangeAsync(usersToAdd);
        }

        await _unitOfWork.SaveChangesAsync();

        var allUsers = await _unitOfWork.Users.GetAllWithDetailsAsync();
        return Ok(allUsers.Select(u => u.toGetUserDto()));
    }

    // DELETE: api/User/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int? id)
    {
        if (!id.HasValue)
        {
            return BadRequest();
        }

        var user = await _unitOfWork.Users.GetByIdAsync(id.Value);
        if (user == null)
        {
            return NotFound();
        }

        _unitOfWork.Users.Delete(user);
        await _unitOfWork.SaveChangesAsync();

        return NoContent();
    }
}
