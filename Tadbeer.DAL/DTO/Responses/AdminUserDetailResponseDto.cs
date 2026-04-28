using Tadbeer.DAL.Models;

namespace Tadbeer.DAL.DTO.Responses;

public class AdminUserDetailResponseDto
{
	public Guid Id { get; set; }
	public string FirstName { get; set; } = null!;
	public string LastName { get; set; } = null!;
	public string Email { get; set; } = null!;
	public string? PrimaryPhoneNumber { get; set; }
	public ICollection<PhoneNumberResponseDto> PhoneNumbers { get; set; } = new List<PhoneNumberResponseDto>();
	public DateOnly? DateOfBirth { get; set; }
	public double? Latitude { get; set; }
	public double? Longitude { get; set; }
	public string? City { get; set; }
	public string ProfileImage { get; set; } = null!;
	public UserRole Role { get; set; }
	public string Status { get; set; } = null!;
	public bool EmailConfirmed { get; set; }
	public string? JobDescription { get; set; }
	public long? ExperienceYears { get; set; }
	public decimal? AvgRating { get; set; }
	public ICollection<Guid>? SpecialtyIds { get; set; } = new List<Guid>();
	public ICollection<string>? SpecialtyNames { get; set; } = new List<string>();
	public ICollection<WorkingHoursResponseDto>? WorkingHours { get; set; } = new List<WorkingHoursResponseDto>();
	public DateTime CreatedAt { get; set; }
	public DateTime UpdatedAt { get; set; }
}
