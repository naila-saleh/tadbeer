namespace Tadbeer.DAL.DTO.Responses.Profile;

public class WorkerProfileResponseDto : UserProfileResponseDto
{
    public string? JobDescription { get; set; }
    public long? ExperienceYears { get; set; }
    public long? AvgRating { get; set; }
    public ICollection<string> SpecialtyNames { get; set; } = new List<string>();
    public ICollection<WorkerWorkImageResponseDto> WorkImages { get; set; } = new List<WorkerWorkImageResponseDto>();
}

