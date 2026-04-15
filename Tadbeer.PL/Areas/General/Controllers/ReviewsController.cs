using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tadbeer.BLL.Services.Interfaces.Specifics;
using Tadbeer.DAL.DTO.Responses;

namespace Tadbeer.PL.Areas.General.Controllers;

[Route("api/[area]/[controller]")]
[ApiController]
[Area("General")]
[AllowAnonymous]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpGet]
    public async Task<ActionResult<ReviewPagedResponseDto>> GetByWorker(
        [FromQuery] Guid workerId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        if (workerId == Guid.Empty)
        {
            return BadRequest(new { message = "workerId is required." });
        }

        var reviews = await _reviewService.GetByWorkerPagedAsync(workerId, pageNumber, pageSize);
        return Ok(reviews);
    }
}

