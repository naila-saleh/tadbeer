using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tadbeer.BLL.Services.Interfaces.Specifics;
using Tadbeer.DAL.DTO.Responses;

namespace Tadbeer.PL.Areas.General.Controllers;

[Route("api/[area]/[controller]")]
[ApiController]
[Area("General")]
[AllowAnonymous]
public class SearchController : ControllerBase
{
    private readonly IApplicationUserService _applicationUserService;
    private readonly ISpecialtyService _specialtyService;

    public SearchController(IApplicationUserService applicationUserService, ISpecialtyService specialtyService)
    {
        _applicationUserService = applicationUserService;
        _specialtyService = specialtyService;
    }

    // GET api/general/search?q=clean&page=1&pageSize=10
    [HttpGet]
    public async Task<ActionResult<GlobalSearchResponseDto>> Search([FromQuery] string? q, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (!string.IsNullOrWhiteSpace(q) && q.Length > 100)
        {
            return BadRequest("Search query must be 100 characters or less.");
        }

        if (page < 1)
        {
            return BadRequest("Page must be greater than or equal to 1.");
        }

        if (pageSize < 1 || pageSize > 50)
        {
            return BadRequest("PageSize must be between 1 and 50.");
        }

        // Keep EF operations sequential because all repositories share one scoped DbContext.
        var workers = await _applicationUserService.SearchWorkersAsync(q, page, pageSize);
        var services = await _specialtyService.SearchServicesAsync(q, page, pageSize);
        var workersTotalCount = await _applicationUserService.CountWorkersAsync(q);
        var servicesTotalCount = await _specialtyService.CountServicesAsync(q);

        return Ok(new GlobalSearchResponseDto
        {
            Page = page,
            PageSize = pageSize,
            WorkersTotalCount = workersTotalCount,
            ServicesTotalCount = servicesTotalCount,
            Workers = workers.ToList(),
            Services = services.ToList()
        });
    }
}
