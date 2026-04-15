using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tadbeer.BLL.Services.Interfaces.Specifics;
using Tadbeer.DAL.DTO.Requests;
using Tadbeer.DAL.DTO.Responses;
using Tadbeer.DAL.Models;

namespace Tadbeer.PL.Areas.Worker.Controllers;

[Route("api/[area]/[controller]")]
[ApiController]
[Area("Worker")]
[Authorize(Roles = nameof(UserRole.Worker))]
public class WorkingHoursController : ControllerBase
{
    private readonly IWorkingHoursService _workingHoursService;

    public WorkingHoursController(IWorkingHoursService workingHoursService)
    {
        _workingHoursService = workingHoursService;
    }

    [HttpPost]
    public async Task<ActionResult<WorkingHoursResponseDto>> Create([FromBody] WorkingHoursRequestDto dto)
    {
        if (!TryGetCurrentWorkerId(out var workerId))
        {
            return Unauthorized();
        }

        var created = await _workingHoursService.CreateForWorkerAsync(workerId, dto);
        return Ok(created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<WorkingHoursResponseDto>> Update(Guid id, [FromBody] WorkingHoursRequestDto dto)
    {
        if (!TryGetCurrentWorkerId(out var workerId))
        {
            return Unauthorized();
        }

        var updated = await _workingHoursService.UpdateOwnAsync(workerId, id, dto);
        if (updated == null)
        {
            return NotFound();
        }

        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (!TryGetCurrentWorkerId(out var workerId))
        {
            return Unauthorized();
        }

        var deleted = await _workingHoursService.DeleteOwnAsync(workerId, id);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    private bool TryGetCurrentWorkerId(out Guid workerId)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userIdValue, out workerId);
    }
}

