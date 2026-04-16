using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tadbeer.BLL.Services.Interfaces.Specifics;
using Tadbeer.DAL.DTO.Requests;
using Tadbeer.DAL.DTO.Responses;

namespace Tadbeer.PL.Areas.General.Controllers;

[Route("api/[area]/[controller]")]
[ApiController]
[Area("General")]
public class PhoneNumbersController : ControllerBase
{
    private readonly IPhoneNumberService _phoneNumberService;

    public PhoneNumbersController(IPhoneNumberService phoneNumberService)
    {
        _phoneNumberService = phoneNumberService;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<PhoneNumberPagedResponseDto>> GetWorkerNumbers(
        [FromQuery] Guid workerId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        if (workerId == Guid.Empty)
        {
            return BadRequest(new { message = "workerId is required." });
        }

        var result = await _phoneNumberService.GetByWorkerPagedAsync(workerId, pageNumber, pageSize);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<IEnumerable<PhoneNumberResponseDto>>> GetMine()
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var result = await _phoneNumberService.GetOwnAsync(userId);
        return Ok(result);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<PhoneNumberResponseDto>> Create([FromBody] PhoneNumberRequestDto dto)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var created = await _phoneNumberService.CreateOwnAsync(userId, dto);
        return Ok(created);
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PhoneNumberResponseDto>> Update(Guid id, [FromBody] PhoneNumberRequestDto dto)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var updated = await _phoneNumberService.UpdateOwnAsync(userId, id, dto);
        if (updated == null)
        {
            return NotFound();
        }

        return Ok(updated);
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var deleted = await _phoneNumberService.DeleteOwnAsync(userId, id);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    private bool TryGetCurrentUserId(out Guid userId)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userIdValue, out userId);
    }
}


