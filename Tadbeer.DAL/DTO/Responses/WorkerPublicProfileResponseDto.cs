namespace Tadbeer.DAL.DTO.Responses;

public class WorkerPublicProfileResponseDto
{
	public Guid Id { get; set; }
	public string FirstName { get; set; } = null!;
	public string LastName { get; set; } = null!;
	public double? Latitude { get; set; }
	public double? Longitude { get; set; }
	public string? City { get; set; }
	public double? DistanceKm { get; set; }
	public string ProfileImage { get; set; } = null!;
	public string? JobDescription { get; set; }
	public long? ExperienceYears { get; set; }
	public decimal? AvgRating { get; set; }
	public ICollection<Guid> SpecialtyIds { get; set; } = new List<Guid>();
	public ICollection<string> SpecialtyNames { get; set; } = new List<string>();
	public ICollection<WorkingHoursResponseDto> WorkingHours { get; set; } = new List<WorkingHoursResponseDto>();
	public ICollection<WorkerPublicWorkImageResponseDto> WorkImages { get; set; } = new List<WorkerPublicWorkImageResponseDto>();
}