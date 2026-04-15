using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tadbeer.BLL.Services.Interfaces.Specifics;
using Tadbeer.DAL.DTO.Requests;
using Tadbeer.DAL.DTO.Responses;
using Tadbeer.DAL.Models;

namespace Tadbeer.PL.Areas.User.Controllers;

[Route("api/[area]/[controller]")]
[ApiController]
[Area("User")]
[Authorize(Roles = nameof(UserRole.User))]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpPost]
    public async Task<ActionResult<BookingResponseDto>> Create([FromBody] BookingRequestDto dto)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var created = await _bookingService.CreateForUserAsync(userId, dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<BookingResponseDto>> Update(Guid id, [FromBody] BookingUpdateRequestDto dto)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var updated = await _bookingService.UpdateOwnAsync(userId, id, dto);
        if (updated == null)
        {
            return NotFound();
        }

        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var deleted = await _bookingService.DeleteOwnAsync(userId, id);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPatch("{id:guid}/cancel")]
    public async Task<ActionResult<BookingResponseDto>> Cancel(Guid id)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var cancelled = await _bookingService.CancelOwnAsync(userId, id);
        if (cancelled == null)
        {
            return NotFound();
        }

        return Ok(cancelled);
    }

    [HttpGet]
    public async Task<ActionResult<BookingPagedResponseDto>> GetMine([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var result = await _bookingService.GetOwnPagedAsync(userId, pageNumber, pageSize);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BookingResponseDto>> GetById(Guid id)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var booking = await _bookingService.GetOwnByIdAsync(userId, id);
        if (booking == null)
        {
            return NotFound();
        }

        return Ok(booking);
    }

    private bool TryGetCurrentUserId(out Guid userId)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userIdValue, out userId);
    }
}

