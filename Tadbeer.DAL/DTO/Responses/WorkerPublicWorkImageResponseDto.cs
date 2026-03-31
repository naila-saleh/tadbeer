namespace Tadbeer.DAL.DTO.Responses;

public class WorkerPublicWorkImageResponseDto
{
    public string ImageUrl { get; set; } = null!;
    public ICollection<WorkerPublicWorkSubImageResponseDto> SubImages { get; set; } = new List<WorkerPublicWorkSubImageResponseDto>();
}

