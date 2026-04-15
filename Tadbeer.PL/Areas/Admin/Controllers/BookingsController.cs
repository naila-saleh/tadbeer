using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tadbeer.BLL.Services.Interfaces.Specifics;
using Tadbeer.DAL.DTO.Responses;
using Tadbeer.DAL.Models;

namespace Tadbeer.PL.Areas.Admin.Controllers;

[Route("api/[area]/[controller]")]
[ApiController]
[Area("Admin")]
[Authorize(Roles = nameof(UserRole.Admin) + ", " + nameof(UserRole.SuperAdmin))]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpGet]
    public async Task<ActionResult<BookingPagedResponseDto>> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _bookingService.GetAllPagedAsync(pageNumber, pageSize);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BookingResponseDto>> GetById(Guid id)
    {
        var booking = await _bookingService.GetAnyByIdAsync(id);
        if (booking == null)
        {
            return NotFound();
        }

        return Ok(booking);
    }
}

