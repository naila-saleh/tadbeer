using Tadbeer.DAL.Models;

namespace Tadbeer.DAL.DTO.Responses.Profile;

public class BaseProfileResponseDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? City { get; set; }
    public string ProfileImage { get; set; } = null!;
    public UserRole Role { get; set; }
}
