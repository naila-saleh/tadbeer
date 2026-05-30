using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using Tadbeer.BLL.Services.Interfaces.Specifics;
using Tadbeer.DAL.DTO.Requests;
using Tadbeer.DAL.DTO.Requests.Profile;
using Tadbeer.DAL.DTO.Requests.WorkImages;
using Tadbeer.DAL.Models;
using Tadbeer.DAL.DTO.Responses.Profile;
using Tadbeer.DAL.DTO.Responses.WorkImages;

namespace Tadbeer.PL.Areas.Worker.Controllers;

[Route("api/[area]/[controller]")]
[ApiController]
[Area("Worker")]
[Authorize(Roles = nameof(UserRole.Worker))]
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

        HydrateComplexWorkerFieldsFromForm(request);

        var profile = await _userService.UpdateWorkerProfileAsync(userId, request);
        if (profile == null)
        {
            return NotFound();
        }

        return Ok(profile);
    }

    [HttpPost("me/work-images")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<WorkImageCreatedResponseDto>> CreateWorkImages([FromForm] CreateWorkImagesRequestDto request)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        if (request.ImageFile == null)
        {
            request.ImageFile = CollectFiles("ImageFile").FirstOrDefault()
                ?? CollectFiles("ImageFiles").FirstOrDefault();
        }

        var createdImages = await _userService.CreateWorkerWorkImagesAsync(userId, request);
        if (createdImages == null)
        {
            return NotFound();
        }

        return Ok(createdImages);
    }

    [HttpPost("me/identity-picture")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<WorkerIdentityVerificationStatusResponseDto>> UploadIdentityPicture([FromForm] WorkerIdentityImageUploadRequestDto request)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        if (request.IdentityImage == null)
        {
            request.IdentityImage = CollectFiles("IdentityImage").FirstOrDefault()
                ?? CollectFiles("IdentityPicture").FirstOrDefault();
        }

        var status = await _userService.UploadWorkerIdentityImageAsync(userId, request);
        if (status == null)
        {
            return NotFound();
        }

        return Ok(status);
    }

    [HttpGet("me/identity-verification")]
    public async Task<ActionResult<WorkerIdentityVerificationStatusResponseDto>> GetIdentityVerificationStatus()
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var status = await _userService.GetWorkerIdentityVerificationStatusAsync(userId);
        if (status == null)
        {
            return NotFound();
        }

        return Ok(status);
    }

    [HttpPost("me/work-images/{mainImageId:guid}/sub-images")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<WorkerProfileResponseDto>> AddSubImagesToMainImage(Guid mainImageId, [FromForm] WorkerMainImageSubImagesRequestDto request)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        request.SubImageFiles = request.SubImageFiles.Count > 0
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
    
    private void HydrateComplexWorkerFieldsFromForm(WorkerProfileUpdateRequestDto request)
    {
        if (!Request.HasFormContentType)
        {
            return;
        }

        if (request.SpecialtyIds == null || request.SpecialtyIds.Count == 0)
        {
            request.SpecialtyIds = ParseSpecialtyIdsFromForm("SpecialtyIds");
        }

        if (request.WorkingHours == null || request.WorkingHours.Count == 0)
        {
            request.WorkingHours = ParseWorkingHoursFromForm("WorkingHours");
        }
    }

    private ICollection<Guid>? ParseSpecialtyIdsFromForm(string fieldName)
    {
        if (!Request.Form.TryGetValue(fieldName, out var values) || values.Count == 0)
        {
            return null;
        }

        var ids = new List<Guid>();
        foreach (var raw in values)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                continue;
            }

            var trimmed = raw.Trim();

            if (trimmed.StartsWith("["))
            {
                try
                {
                    using var doc = JsonDocument.Parse(trimmed);
                    if (doc.RootElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in doc.RootElement.EnumerateArray())
                        {
                            if (item.ValueKind == JsonValueKind.String && Guid.TryParse(item.GetString(), out var fromArray))
                            {
                                ids.Add(fromArray);
                            }
                        }
                    }
                }
                catch
                {
                    // Ignore malformed JSON and continue with other values.
                }

                continue;
            }

            if (Guid.TryParse(trimmed, out var single))
            {
                ids.Add(single);
                continue;
            }

            foreach (var token in trimmed.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (Guid.TryParse(token, out var parsed))
                {
                    ids.Add(parsed);
                }
            }
        }

        return ids.Count > 0 ? ids.Distinct().ToList() : null;
    }

    private ICollection<WorkingHoursRequestDto>? ParseWorkingHoursFromForm(string fieldName)
    {
        if (!Request.Form.TryGetValue(fieldName, out var values) || values.Count == 0)
        {
            return null;
        }

        var raw = values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
        if (string.IsNullOrWhiteSpace(raw) || !raw.TrimStart().StartsWith("["))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(raw);
            if (doc.RootElement.ValueKind != JsonValueKind.Array)
            {
                return null;
            }

            var result = new List<WorkingHoursRequestDto>();
            foreach (var entry in doc.RootElement.EnumerateArray())
            {
                if (entry.ValueKind != JsonValueKind.Object)
                {
                    continue;
                }

                if (!TryGetDayOfWeek(entry, out var day))
                {
                    continue;
                }

                if (!TryGetTime(entry, "startTime", out var startTime) || !TryGetTime(entry, "endTime", out var endTime))
                {
                    continue;
                }

                result.Add(new WorkingHoursRequestDto
                {
                    DayOfWeek = day,
                    StartTime = startTime,
                    EndTime = endTime
                });
            }

            return result.Count > 0 ? result : null;
        }
        catch
        {
            return null;
        }
    }

    private static bool TryGetDayOfWeek(JsonElement entry, out WeekDay day)
    {
        day = default;
        if (!TryGetJsonPropertyIgnoreCase(entry, "dayOfWeek", out var dayProperty))
        {
            return false;
        }

        if (dayProperty.ValueKind == JsonValueKind.Number && dayProperty.TryGetInt32(out var dayIndex) && Enum.IsDefined(typeof(WeekDay), dayIndex))
        {
            day = (WeekDay)dayIndex;
            return true;
        }

        if (dayProperty.ValueKind == JsonValueKind.String && Enum.TryParse<WeekDay>(dayProperty.GetString(), true, out var parsedDay))
        {
            day = parsedDay;
            return true;
        }

        return false;
    }

    private static bool TryGetTime(JsonElement entry, string propertyName, out TimeOnly value)
    {
        value = default;

        if (!TryGetJsonPropertyIgnoreCase(entry, propertyName, out var timeProperty) || timeProperty.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        var text = timeProperty.GetString();
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var allowedFormats = new[] { "HH:mm:ss", "H:mm:ss", "HH:mm", "H:mm", "h:mm tt", "hh:mm tt" };

        return TimeOnly.TryParseExact(text.Trim(), allowedFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out value)
               || TimeOnly.TryParse(text.Trim(), out value);
    }

    private static bool TryGetJsonPropertyIgnoreCase(JsonElement entry, string propertyName, out JsonElement propertyValue)
    {
        foreach (var property in entry.EnumerateObject())
        {
            if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                propertyValue = property.Value;
                return true;
            }
        }

        propertyValue = default;
        return false;
    }
}

