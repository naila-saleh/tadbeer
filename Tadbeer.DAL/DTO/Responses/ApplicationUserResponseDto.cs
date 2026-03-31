using Tadbeer.DAL.Models;

namespace Tadbeer.DAL.DTO.Responses;

public class ApplicationUserResponseDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public string City { get; set; } = null!;
    public string ProfileImage { get; set; } = null!;

    public string Role { get; set; } = null!;
    public string Status { get; set; } = null!;
    public bool EmailConfirmed { get; set; }

    // Worker specific
    public string? JobDescription { get; set; }
    public long? ExperienceYears { get; set; }
    public long? AvgRating { get; set; }
    public ICollection<string>? SpecialtyNames { get; set; } = new List<string>();

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
