namespace Tadbeer.DAL.DTO.Responses;

public class GlobalSearchResponseDto
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int WorkersTotalCount { get; set; }
    public int ServicesTotalCount { get; set; }
    public ICollection<WorkerPublicProfileResponseDto> Workers { get; set; } = new List<WorkerPublicProfileResponseDto>();
    public ICollection<SpecialtyResponseDto> Services { get; set; } = new List<SpecialtyResponseDto>();
}
