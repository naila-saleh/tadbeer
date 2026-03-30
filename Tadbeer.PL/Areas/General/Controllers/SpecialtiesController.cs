using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tadbeer.BLL.Services.Interfaces.Specifics;
using Tadbeer.DAL.DTO.Responses;

namespace Tadbeer.PL.Areas.User.Controllers;

[Route("api/[area]/[controller]")]
[ApiController]
[Area("User")]
[AllowAnonymous]
public class SpecialtiesController : ControllerBase
{
    private readonly ISpecialtyService _specialtyService;

    public SpecialtiesController(ISpecialtyService specialtyService)
    {
        _specialtyService = specialtyService;
    }

    // GET api/user/specialties
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SpecialtyResponseDto>>> GetAll()
    {
        var specialties = await _specialtyService.GetAllAsync();
        return Ok(specialties);
    }

    // GET api/user/specialties/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SpecialtyResponseDto>> GetById(Guid id)
    {
        var specialty = await _specialtyService.GetByIdAsync(id);
        if (specialty is null) return NotFound();
        return Ok(specialty);
    }
}
