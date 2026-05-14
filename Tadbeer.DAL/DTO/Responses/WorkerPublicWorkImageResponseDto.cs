namespace Tadbeer.DAL.DTO.Responses;

public class WorkerPublicWorkImageResponseDto
{
    public string ImageUrl { get; set; } = null!;
  public string Name { get; set; } = null!;
  public string? Description { get; set; }
    public ICollection<WorkerPublicWorkSubImageResponseDto> SubImages { get; set; } = new List<WorkerPublicWorkSubImageResponseDto>();
}

