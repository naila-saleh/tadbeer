using Microsoft.AspNetCore.Identity;
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
    private readonly IEmailSender _emailSender;
    private readonly IGenerateJWTService _generateJwtService;

    public AuthService(UserManager<ApplicationUser> userManager, IEmailSender emailSender, IGenerateJWTService generateJWTService)
    {
        _userManager = userManager;
        _emailSender = emailSender;
        _generateJwtService = generateJWTService;
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

        var token = await _generateJwtService.GenerateJwtTokenAsync(user);

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

    public async Task<AuthResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                Message = "User not found."
            };
        }
        var random = new Random();
        var code = random.Next(100000, 999999).ToString();
        user.CodeResetPassword = code;
        user.ExpirationCodeResetPassword = DateTime.UtcNow.AddMinutes(15);

        await _userManager.UpdateAsync(user);

        await _emailSender.SendEmailAsync(user.Email!, "Reset your password",
            $"Your password reset code is: <strong>{code}</strong>. It expires in 15 minutes.");

        return new AuthResponseDto
        {
            IsSuccess = true,
            Message = "Reset password code sent to your email."
        };
    }

    public async Task<AuthResponseDto> ResetPasswordAsync(ResetPasswordRequestDto model)
    {
        if (model.NewPassword != model.ConfirmNewPassword)
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                Message = "Passwords do not match."
            };
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                Message = "User not found."
            };
        }

        if (user.CodeResetPassword != model.Code || user.ExpirationCodeResetPassword < DateTime.UtcNow)
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                Message = "Invalid or expired reset code."
            };
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

        if (result.Succeeded)
        {
            // Clear the reset code once used
            user.CodeResetPassword = null;
            user.ExpirationCodeResetPassword = null;
            await _userManager.UpdateAsync(user);

            await _emailSender.SendEmailAsync(model.Email, "Reset Password", "<h1>Password Changed Successfully</h1>");

            return new AuthResponseDto
            {
                IsSuccess = true,
                Message = "Password changed successfully."
            };
        }

        return new AuthResponseDto
        {
            IsSuccess = false,
            Message = "Password reset failed.",
            Errors = result.Errors.Select(e => e.Description)
        };
    }
}

