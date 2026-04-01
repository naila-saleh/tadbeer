namespace Tadbeer.DAL.DTO.Requests.Profile;

public class WorkerProfileUpdateRequestDto : BaseProfileUpdateRequestDto
{
    public string? JobDescription { get; set; }
    public long? ExperienceYears { get; set; }
    // Optional list of specialty ids from UI multi-select.
    public ICollection<Guid>? SpecialtyIds { get; set; }
    public ICollection<WorkingHoursRequestDto>? WorkingHours { get; set; }
}