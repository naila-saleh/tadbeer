using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
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
        var workers = users.Where(u => string.Equals(u.Role, "Worker", StringComparison.OrdinalIgnoreCase));
        return Ok(workers);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApplicationUserResponseDto>> GetWorkerById(Guid id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null || !string.Equals(user.Role, "Worker", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound();
        }

        return Ok(user);
    }
}