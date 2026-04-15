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
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpPost]
    public async Task<ActionResult<ReviewResponseDto>> Create([FromBody] ReviewRequestDto dto)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var created = await _reviewService.CreateForUserAsync(userId, dto);
        return Ok(created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ReviewResponseDto>> Update(Guid id, [FromBody] ReviewUpdateRequestDto dto)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var updated = await _reviewService.UpdateOwnAsync(userId, id, dto);
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

        var deleted = await _reviewService.DeleteOwnAsync(userId, id);
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

