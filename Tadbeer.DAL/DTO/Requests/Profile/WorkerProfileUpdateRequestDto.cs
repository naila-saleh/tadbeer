namespace Tadbeer.DAL.DTO.Requests.Profile;

public class WorkerProfileUpdateRequestDto : BaseProfileUpdateRequestDto
{
    public string? JobDescription { get; set; }
    public long? ExperienceYears { get; set; }
    // Comma-separated specialty names from UI select (e.g. "Plumbing,Electrical")
    public string? SpecialtyNamesCsv { get; set; }
    public ICollection<WorkingHoursRequestDto>? WorkingHours { get; set; }
}