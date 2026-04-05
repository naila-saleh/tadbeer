using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tadbeer.BLL.Services.Interfaces.Specifics;
using Tadbeer.DAL.DTO.Requests.Profile;
using Tadbeer.DAL.DTO.Responses.Profile;
using Tadbeer.DAL.Models;

namespace Tadbeer.PL.Areas.User.Controllers;

[Route("api/[area]/[controller]")]
[ApiController]
[Area("User")]
[Authorize(Roles = nameof(UserRole.User))]
public class ProfileController : ControllerBase
{
    private readonly IApplicationUserService _userService;

    public ProfileController(IApplicationUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserProfileResponseDto>> GetMyProfile()
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var profile = await _userService.GetUserProfileAsync(userId);
        if (profile == null)
        {
            return NotFound();
        }

        return Ok(profile);
    }

    [HttpPut("me")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<UserProfileResponseDto>> UpdateMyProfile([FromForm] UserProfileUpdateRequestDto request)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var profile = await _userService.UpdateUserProfileAsync(userId, request);
        if (profile == null)
        {
            return NotFound();
        }

        return Ok(profile);
    }

    [HttpPatch("me/status-toggle")]
    public async Task<IActionResult> ToggleMyStatus()
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var newStatus = await _userService.ToggleUserOrWorkerStatusAsync(userId);
        if (newStatus == null)
        {
            return NotFound();
        }

        return Ok(new { status = newStatus });
    }

    private bool TryGetCurrentUserId(out Guid userId)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userIdValue, out userId);
    }
}

