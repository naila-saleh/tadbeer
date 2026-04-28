using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tadbeer.BLL.Services.Interfaces.Specifics;
using Tadbeer.DAL.DTO.Requests;
using Tadbeer.DAL.DTO.Responses;
using Tadbeer.DAL.Models;

namespace Tadbeer.PL.Areas.General.Controllers;

[Route("api/[area]/[controller]")]
[ApiController]
[Area("General")]
[AllowAnonymous]
public class WorkersController : ControllerBase
{
    private readonly IApplicationUserService _userService;

    public WorkersController(IApplicationUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("")]
    public async Task<ActionResult<WorkersFilteredResponseDto>> GetWorkers([FromQuery] WorkerFiltersRequestDto request)
    {
        if ((request.SortByNearest || request.MaxDistanceKm.HasValue)
            && (!request.Latitude.HasValue || !request.Longitude.HasValue))
        {
            return BadRequest("Latitude and longitude are required when using nearest or distance filters.");
        }

        // Model binding for decimals may fail in some client locales (comma vs dot).
        // If MinRating was not bound, try to parse it manually from the raw query string.
        if (!request.MinRating.HasValue && Request.Query.ContainsKey("minRating"))
        {
            var raw = Request.Query["minRating"].ToString();
            if (!string.IsNullOrWhiteSpace(raw))
            {
                raw = raw.Replace(',', '.');
                if (double.TryParse(raw, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var parsed))
                {
                    request.MinRating = parsed;
                }
            }
        }

        var workers = await _userService.GetWorkersByFiltersAsync(request);
        return Ok(workers);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApplicationUserResponseDto>> GetWorkerById(Guid id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null || user.Role != UserRole.Worker)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpGet("{id:guid}/profile")]
    public async Task<ActionResult<WorkerPublicProfileResponseDto>> GetWorkerPublicProfile(Guid id)
    {
        var profile = await _userService.GetWorkerPublicProfileAsync(id);
        if (profile == null)
        {
            return NotFound();
        }

        return Ok(profile);
    }
}