using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tadbeer.BLL.Services.Interfaces.Specifics;
using Tadbeer.DAL.DTO.Requests.Profile;
using Tadbeer.DAL.DTO.Requests.WorkImages;
using Tadbeer.DAL.DTO.Responses.Profile;
using Tadbeer.DAL.DTO.Responses.WorkImages;

namespace Tadbeer.PL.Areas.Worker.Controllers;

[Route("api/[area]/[controller]")]
[ApiController]
[Area("Worker")]
[Authorize(Roles = "Worker")]
public class ProfileController : ControllerBase
{
    private readonly IApplicationUserService _userService;

    public ProfileController(IApplicationUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("me")]
    public async Task<ActionResult<WorkerProfileResponseDto>> GetMyProfile()
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var profile = await _userService.GetWorkerProfileAsync(userId);
        if (profile == null)
        {
            return NotFound();
        }

        return Ok(profile);
    }

    [HttpPut("me")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<WorkerProfileResponseDto>> UpdateMyProfile([FromForm] WorkerProfileUpdateRequestDto request)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var profile = await _userService.UpdateWorkerProfileAsync(userId, request);
        if (profile == null)
        {
            return NotFound();
        }

        return Ok(profile);
    }

    [HttpPost("me/work-images")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<IEnumerable<WorkImageCreatedResponseDto>>> CreateWorkImages([FromForm] CreateWorkImagesRequestDto request)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        request.MainImageFiles = request.MainImageFiles?.Count > 0
            ? request.MainImageFiles
            : CollectFiles("ImageFiles");

        var createdImages = await _userService.CreateWorkerWorkImagesAsync(userId, request);
        if (createdImages == null)
        {
            return NotFound();
        }

        return Ok(createdImages);
    }

    [HttpPost("me/work-images/{mainImageId:guid}/sub-images")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<WorkerProfileResponseDto>> AddSubImagesToMainImage(Guid mainImageId, [FromForm] WorkerMainImageSubImagesRequestDto request)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        request.SubImageFiles = request.SubImageFiles?.Count > 0
            ? request.SubImageFiles
            : CollectFiles("SubImageFiles");

        var profile = await _userService.AddWorkerSubImagesToMainImageAsync(userId, mainImageId, request);
        if (profile == null)
        {
            return NotFound();
        }

        return Ok(profile);
    }
    
    [HttpGet("me/work-images")]
    public async Task<ActionResult<IEnumerable<WorkerWorkImageResponseDto>>> GetMyMainImages()
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var images = await _userService.GetWorkerMainImagesAsync(userId);
        if (images == null)
        {
            return NotFound();
        }

        return Ok(images);
    }

    [HttpGet("me/work-images/{mainImageId:guid}/sub-images")]
    public async Task<ActionResult<IEnumerable<WorkerWorkSubImageResponseDto>>> GetMySubImages(Guid mainImageId)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var subImages = await _userService.GetWorkerSubImagesAsync(userId, mainImageId);
        if (subImages == null)
        {
            return NotFound();
        }

        return Ok(subImages);
    }

    [HttpDelete("me/work-images/{mainImageId:guid}")]
    public async Task<ActionResult<WorkerProfileResponseDto>> DeleteMainImage(Guid mainImageId)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var profile = await _userService.DeleteWorkerMainImageAsync(userId, mainImageId);
        if (profile == null)
        {
            return NotFound();
        }

        return Ok(profile);
    }

    [HttpDelete("me/work-sub-images/{subImageId:guid}")]
    public async Task<ActionResult<WorkerProfileResponseDto>> DeleteSubImage(Guid subImageId)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var profile = await _userService.DeleteWorkerSubImageAsync(userId, subImageId);
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

    private List<IFormFile> CollectFiles(string fieldName)
    {
        var files = Request.Form.Files
            .Where(f =>
                string.Equals(f.Name, fieldName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(f.Name, $"{fieldName}[]", StringComparison.OrdinalIgnoreCase))
            .ToList();

        // Fallback to all posted files if client omits field names.
        return files.Count > 0 ? files : Request.Form.Files.ToList();
    }
}

