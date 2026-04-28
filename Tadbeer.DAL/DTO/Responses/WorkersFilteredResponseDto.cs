namespace Tadbeer.DAL.DTO.Responses;
public class WorkersFilteredResponseDto
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public ICollection<WorkerPublicProfileResponseDto> Workers { get; set; } = new List<WorkerPublicProfileResponseDto>();
}
