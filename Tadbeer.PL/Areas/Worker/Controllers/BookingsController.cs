using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tadbeer.BLL.Services.Interfaces.Specifics;
using Tadbeer.DAL.DTO.Responses;
using Tadbeer.DAL.Models;

namespace Tadbeer.PL.Areas.Worker.Controllers;

[Route("api/[area]/[controller]")]
[ApiController]
[Area("Worker")]
[Authorize(Roles = nameof(UserRole.Worker))]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpGet]
    public async Task<ActionResult<BookingPagedResponseDto>> GetIncoming([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        if (!TryGetCurrentWorkerId(out var workerId))
        {
            return Unauthorized();
        }

        var result = await _bookingService.GetForWorkerPagedAsync(workerId, pageNumber, pageSize);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BookingResponseDto>> GetById(Guid id)
    {
        if (!TryGetCurrentWorkerId(out var workerId))
        {
            return Unauthorized();
        }

        var booking = await _bookingService.GetForWorkerByIdAsync(workerId, id);
        if (booking == null)
        {
            return NotFound();
        }

        return Ok(booking);
    }

    [HttpPatch("{id:guid}/accept")]
    public async Task<ActionResult<BookingResponseDto>> Accept(Guid id)
    {
        if (!TryGetCurrentWorkerId(out var workerId))
        {
            return Unauthorized();
        }

        var accepted = await _bookingService.AcceptForWorkerAsync(workerId, id);
        if (accepted == null)
        {
            return NotFound();
        }

        return Ok(accepted);
    }

    [HttpPatch("{id:guid}/cancel")]
    public async Task<ActionResult<BookingResponseDto>> Cancel(Guid id)
    {
        if (!TryGetCurrentWorkerId(out var workerId))
        {
            return Unauthorized();
        }

        var cancelled = await _bookingService.CancelForWorkerAsync(workerId, id);
        if (cancelled == null)
        {
            return NotFound();
        }

        return Ok(cancelled);
    }

    private bool TryGetCurrentWorkerId(out Guid workerId)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userIdValue, out workerId);
    }
}

