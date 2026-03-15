using Microsoft.AspNetCore.Mvc;
using Tadbeer.BLL.Services.Interfaces;
using Tadbeer.DAL.DTO.Requests;

namespace Tadbeer.PL.Areas.Identity.Controllers;

[Route("api/[area]/[controller]")]
[ApiController]
[Area("Identity")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _authService.RegisterAsync(model, Request);

        if (result.IsSuccess)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _authService.LoginAsync(model);

        if (result.IsSuccess)
        {
            return Ok(result);
        }

        return Unauthorized(result);
    }

    [HttpGet("confirm-email")]
    public async Task<ActionResult<string>> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _authService.ConfirmEmailAsync(userId, token);
        if (result.IsSuccess)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult<string>> ForgotPassword([FromBody] ForgotPasswordRequestDto model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        var result = await _authService.ForgotPasswordAsync(model);
        if (result.IsSuccess)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpPatch("reset-password")]
    public async Task<ActionResult<string>> ResetPassword([FromBody] ResetPasswordRequestDto model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        var result = await _authService.ResetPasswordAsync(model);
        if (result.IsSuccess)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }
}
