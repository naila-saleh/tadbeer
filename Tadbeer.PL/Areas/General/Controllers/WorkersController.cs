using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tadbeer.BLL.Services.Interfaces.Specifics;
using Tadbeer.DAL.DTO.Responses;
using Tadbeer.DAL.Models;

namespace Tadbeer.PL.Areas.General.Controllers;

[Route("api/[area]/[controller]")]
[ApiController]
[Area("General")]
[AllowAnonymous]
public class WorkersController : ControllerBase
{
    private readonly IApplicationUserService _userService;

    public WorkersController(IApplicationUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("")]
    public async Task<ActionResult<IEnumerable<ApplicationUserResponseDto>>> GetWorkers()
    {
        var users = await _userService.GetAllAsync();
        var workers = users.Where(u => u.Role == UserRole.Worker);
        return Ok(workers);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApplicationUserResponseDto>> GetWorkerById(Guid id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null || user.Role != UserRole.Worker)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpGet("{id:guid}/profile")]
    public async Task<ActionResult<WorkerPublicProfileResponseDto>> GetWorkerPublicProfile(Guid id)
    {
        var profile = await _userService.GetWorkerPublicProfileAsync(id);
        if (profile == null)
        {
            return NotFound();
        }

        return Ok(profile);
    }
}