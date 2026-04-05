using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Tadbeer.DAL.DTO.Requests.Profile;

public class WorkerProfileUpdateRequestDto : BaseProfileUpdateRequestDto
{
    public string? JobDescription { get; set; }
    public long? ExperienceYears { get; set; }
    
    // Optional list of specialty ids from UI multi-select.
    // Suppressed automatic binding to allow manual hydration from JSON in controller.
    [BindNever]
    public ICollection<Guid>? SpecialtyIds { get; set; }
    
    [BindNever]
    public ICollection<WorkingHoursRequestDto>? WorkingHours { get; set; }
}