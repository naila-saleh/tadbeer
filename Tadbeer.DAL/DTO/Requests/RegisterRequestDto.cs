using System.ComponentModel.DataAnnotations;
using Tadbeer.DAL.Models;

namespace Tadbeer.DAL.DTO.Requests;

public class RegisterRequestDto
{
    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = null!;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;

    [Required]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = null!;

    [Required]
    public UserRole Role { get; set; } = UserRole.User;
}
