using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Tadbeer.BLL.Services.Interfaces;
using Tadbeer.DAL.DTO.Requests;
using Tadbeer.DAL.DTO.Responses;
using Tadbeer.DAL.Models;

namespace Tadbeer.BLL.Services.Classes;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly IEmailSender _emailSender;

    public AuthService(UserManager<ApplicationUser> userManager, IConfiguration configuration, IEmailSender emailSender)
    {
        _userManager = userManager;
        _configuration = configuration;
        _emailSender = emailSender;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto model, HttpRequest request)
    {
        var existingUser = await _userManager.FindByEmailAsync(model.Email);
        if (existingUser != null)
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                Message = "User already exists with this email.",
                Errors = new[] { "Email already in use." }
            };
        }

        var names = model.FullName.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var firstName = names.Length > 0 ? names[0] : "";
        var lastName = names.Length > 1 ? names[1] : "";

        var user = new ApplicationUser
        {
            Email = model.Email,
            UserName = model.Email, // Identity requires UserName, we'll use Email
            FirstName = firstName,
            LastName = lastName,
            City = "", // Setting default, could be added to DTO if needed
            ProfileImage = "",
            Status = UserStatus.Existed,
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        if (!string.IsNullOrEmpty(model.PhoneNumber))
        {
            user.PhoneNumber = model.PhoneNumber;
        }

        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var escapeToken = Uri.EscapeDataString(token);
            var emailUrl = $"{request.Scheme}://{request.Host}/api/identity/auth/confirm-email?token={escapeToken}&userId={user.Id}";
            await _emailSender.SendEmailAsync(user.Email, "Confirm your email",
            $"Please confirm your email by clicking here: <a href='{emailUrl}'>Confirm Email</a>");
            return new AuthResponseDto
            {
                IsSuccess = true,
                Message = "User created successfully."
            };
        }

        return new AuthResponseDto
        {
            IsSuccess = false,
            Message = "User creation failed.",
            Errors = result.Errors.Select(e => e.Description)
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                Message = "Invalid credentials."
            };
        }

        var isEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
        if (!isEmailConfirmed)
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                Message = "Email not confirmed."
            };
        }
        
        var isPasswordValid = await _userManager.CheckPasswordAsync(user, model.Password);
        if (!isPasswordValid)
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                Message = "Invalid credentials."
            };
        }

        var token = await GenerateJwtTokenAsync(user);

        return new AuthResponseDto
        {
            IsSuccess = true,
            Message = "Login successful.",
            Token = token
        };
    }

    public async Task<AuthResponseDto> ConfirmEmailAsync(string userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                Message = "User not found."
            };
        }

        var result = await _userManager.ConfirmEmailAsync(user, token);
        if (result.Succeeded)
        {
            return new AuthResponseDto
            {
                IsSuccess = true,
                Message = "Email confirmed successfully."
            };
        }

        return new AuthResponseDto()
        {
            IsSuccess = false,
            Message = "Email confirmation failed."
        };
    }
    
    private async Task<string> GenerateJwtTokenAsync(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
    
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
    
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddDays(Convert.ToDouble(_configuration["Jwt:ExpireYears"]));
    
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );
    
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    
}
