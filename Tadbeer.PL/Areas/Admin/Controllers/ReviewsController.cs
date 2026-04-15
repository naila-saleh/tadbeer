using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tadbeer.BLL.Services.Interfaces.Specifics;
using Tadbeer.DAL.Models;

namespace Tadbeer.PL.Areas.Admin.Controllers;

[Route("api/[area]/[controller]")]
[ApiController]
[Area("Admin")]
[Authorize(Roles = nameof(UserRole.Admin) + ", " + nameof(UserRole.SuperAdmin))]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _reviewService.DeleteAnyAsync(id);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}

