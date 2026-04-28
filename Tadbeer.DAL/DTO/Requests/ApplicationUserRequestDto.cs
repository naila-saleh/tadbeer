using System.ComponentModel.DataAnnotations;
using Tadbeer.DAL.Models;

namespace Tadbeer.DAL.DTO.Requests;

public class ApplicationUserRequestDto
{
    [Required]
    public string FirstName { get; set; } = null!;
    [Required]
    public string LastName { get; set; } = null!;
    [Required, EmailAddress]
    public string Email { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    [DataType(DataType.Date)]
    public DateOnly? DateOfBirth { get; set; }
    [Range(-90, 90)]
    public double? Latitude { get; set; }

    [Range(-180, 180)]
    public double? Longitude { get; set; }
    
    // Optional password field (useful during creation)
    public string? Password { get; set; }

    public UserRole Role { get; set; } = UserRole.User;
    public UserStatus Status { get; set; } = UserStatus.Existed;

    // Worker specific
    public string? JobDescription { get; set; }
    public long? ExperienceYears { get; set; }
}
