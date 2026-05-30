using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tadbeer.BLL.Services.Interfaces.Specifics;
using Tadbeer.DAL.DTO.Requests;
using Tadbeer.DAL.DTO.Requests.Profile;
using Tadbeer.DAL.DTO.Responses;
using Tadbeer.DAL.DTO.Responses.Profile;
using Tadbeer.DAL.Models;

namespace Tadbeer.PL.Areas.Admin.Controllers;

[Route("api/[area]/[controller]")]
[ApiController]
[Area("Admin")]
[Authorize(Roles = nameof(UserRole.Admin) + ", " + nameof(UserRole.SuperAdmin))]
public class UsersController : ControllerBase
{
    private readonly IApplicationUserService _userService;

    public UsersController(
        IApplicationUserService userService)
    {
        _userService = userService;
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AdminUserListResponseDto>>> GetAll()
    {
        var users = await _userService.GetAdminUsersListAsync();
        return Ok(users);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<AdminUserDetailResponseDto>> GetById(Guid id)
    {
        var user = await _userService.GetAdminUserDetailAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        return Ok(user);
    }

    [HttpGet("{id}/identity-verification")]
    public async Task<ActionResult<AdminUserIdentityVerificationResponseDto>> GetIdentityVerification(Guid id)
    {
        var user = await _userService.GetAdminUserIdentityVerificationAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpGet("identity-verification/pending")]
    public async Task<ActionResult<IEnumerable<AdminUserIdentityVerificationResponseDto>>> GetPendingIdentityVerifications()
    {
        var users = await _userService.GetPendingWorkerIdentityVerificationsAsync();
        return Ok(users);
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _userService.RemoveAsync(id);
        return NoContent();
    }

    [HttpPost("{id}/block")]
    public async Task<IActionResult> BlockUser(Guid id, [FromQuery] int minutes = 15)
    {
        var result = await _userService.BlockUserAsync(id, minutes);
        if (!result) return BadRequest(new { message = "Operation failed." });
        return Ok(new { message = "User blocked successfully." });
    }

    [HttpPost("{id}/unblock")]
    public async Task<IActionResult> UnBlockUser(Guid id)
    {
        var result = await _userService.UnBlockUserAsync(id);
        if (!result) return BadRequest(new { message = "Operation failed." });
        return Ok(new { message = "User unblocked successfully." });
    }

    [HttpGet("{id}/is-blocked")]
    public async Task<IActionResult> IsBlocked(Guid id)
    {
        var result = await _userService.IsBlockedAsync(id);
        return Ok(new { isBlocked = result });
    }

    [Authorize(Roles = nameof(UserRole.SuperAdmin))]
    [HttpPost("{id}/change-role")]
    public async Task<IActionResult> ChangeUserRole(Guid id, [FromBody] ChangeRoleRequest request)
    {
        var result = await _userService.ChangeUserRoleAsync(id, request);
        if (!result) return BadRequest(new { message = "Operation failed. Make sure the provided role is valid." });
        return Ok(new { message = "User role updated successfully." });
    }

    [HttpPost("{id}/identity-verification/approve")]
    public async Task<ActionResult<WorkerIdentityVerificationStatusResponseDto>> ApproveWorkerIdentity(Guid id)
    {
        var result = await _userService.ApproveWorkerIdentityImageAsync(id);
        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpPost("{id}/identity-verification/reject")]
    public async Task<ActionResult<WorkerIdentityVerificationStatusResponseDto>> RejectWorkerIdentity(Guid id, [FromBody] AdminRejectWorkerIdentityRequestDto request)
    {
        var result = await _userService.RejectWorkerIdentityImageAsync(id, request);
        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }
}
