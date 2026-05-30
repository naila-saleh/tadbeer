namespace Tadbeer.DAL.DTO.Responses.Profile;

public class WorkerProfileResponseDto : UserProfileResponseDto
{
    public string? IdentityImageUrl { get; set; }
    public bool IsIdentityVerified { get; set; }
    public string? JobDescription { get; set; }
    public long? ExperienceYears { get; set; }
    public decimal? AvgRating { get; set; }
    public ICollection<Guid> SpecialtyIds { get; set; } = new List<Guid>();
    public ICollection<string> SpecialtyNames { get; set; } = new List<string>();
    public ICollection<WorkingHoursResponseDto> WorkingHours { get; set; } = new List<WorkingHoursResponseDto>();
    public ICollection<WorkerWorkImageResponseDto> WorkImages { get; set; } = new List<WorkerWorkImageResponseDto>();
}

