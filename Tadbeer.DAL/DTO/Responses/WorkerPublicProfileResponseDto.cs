namespace Tadbeer.DAL.DTO.Responses;

public class WorkerPublicProfileResponseDto
{
	public Guid Id { get; set; }
	public string FirstName { get; set; } = null!;
	public string LastName { get; set; } = null!;
	public string City { get; set; } = null!;
	public string ProfileImage { get; set; } = null!;
	public string? JobDescription { get; set; }
	public long? ExperienceYears { get; set; }
	public long? AvgRating { get; set; }
	public ICollection<string> SpecialtyNames { get; set; } = new List<string>();
	public ICollection<WorkerPublicWorkImageResponseDto> WorkImages { get; set; } = new List<WorkerPublicWorkImageResponseDto>();
}