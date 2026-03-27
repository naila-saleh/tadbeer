using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Tadbeer.BLL.Services.Interfaces.Specifics;
using Tadbeer.DAL.DTO.Requests;
using Tadbeer.DAL.DTO.Responses;
using Tadbeer.DAL.Models;

namespace Tadbeer.PL.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IApplicationUserService _userService;
    private readonly UserManager<ApplicationUser> _userManager;

    public UsersController(
        IApplicationUserService userService,
        UserManager<ApplicationUser> userManager)
    {
        _userService = userService;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ApplicationUserResponseDto>>> GetAll()
    {
        var users = await _userService.GetAllAsync();
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApplicationUserResponseDto>> GetById(Guid id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        return Ok(user);
    }

    [HttpPost]
    public async Task<ActionResult<ApplicationUserResponseDto>> Create([FromBody] ApplicationUserRequestDto dto)
    {
        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            PhoneNumber = dto.PhoneNumber,
            City = dto.City,
            Role = dto.Role,
            Status = dto.Status,
            JobDescription = dto.JobDescription,
            ExperienceYears = dto.ExperienceYears,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            ProfileImage = "default.png" // Default image
        };

        IdentityResult result;
        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            result = await _userManager.CreateAsync(user, dto.Password);
        }
        else
        {
            result = await _userManager.CreateAsync(user);
        }

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        var responseDto = user.Adapt<ApplicationUserResponseDto>();
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, responseDto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ApplicationUserRequestDto dto)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            return NotFound();
        }

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.PhoneNumber = dto.PhoneNumber;
        user.City = dto.City;
        user.Role = dto.Role;
        user.Status = dto.Status;
        user.JobDescription = dto.JobDescription;
        user.ExperienceYears = dto.ExperienceYears;
        user.UpdatedAt = DateTime.UtcNow;

        if (user.Email != dto.Email)
        {
            user.Email = dto.Email;
            user.UserName = dto.Email;
        }

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            // Reset the password if provided in the DTO
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            await _userManager.ResetPasswordAsync(user, token, dto.Password);
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _userService.RemoveAsync(id);
        return NoContent();
    }
}
