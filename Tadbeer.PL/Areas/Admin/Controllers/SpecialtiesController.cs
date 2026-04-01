using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tadbeer.BLL.Services.Interfaces.Specifics;
using Tadbeer.DAL.DTO.Requests;
using Tadbeer.DAL.DTO.Responses;

namespace Tadbeer.PL.Areas.Admin.Controllers;

[Route("api/[area]/[controller]")]
[ApiController]
[Area("Admin")]
[Authorize(Roles = "Admin, SuperAdmin")]
public class SpecialtiesController : ControllerBase
{
    private readonly ISpecialtyService _specialtyService;

    public SpecialtiesController(ISpecialtyService specialtyService)
    {
        _specialtyService = specialtyService;
    }

    // POST api/admin/specialties
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<SpecialtyResponseDto>> Create([FromForm] SpecialtyRequestDto dto)
    {
        var created = await _specialtyService.AddAsync(dto);
        return CreatedAtAction(nameof(Create), new { id = created.Id }, created);
    }

    // PUT api/admin/specialties/{id}
    [HttpPut("{id:guid}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update(Guid id, [FromForm] SpecialtyRequestDto dto)
    {
        var existing = await _specialtyService.GetByIdAsync(id);
        if (existing is null) return NotFound();

        await _specialtyService.UpdateAsync(dto, id);
        return NoContent();
    }

    // DELETE api/admin/specialties/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var existing = await _specialtyService.GetByIdAsync(id);
        if (existing is null) return NotFound();

        await _specialtyService.RemoveAsync(id);
        return NoContent();
    }
}
