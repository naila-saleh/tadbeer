namespace Tadbeer.DAL.DTO.Responses.Profile;

public class WorkerWorkImageResponseDto
{
    public Guid Id { get; set; }
    public string ImageUrl { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ICollection<WorkerWorkSubImageResponseDto> SubImages { get; set; } = new List<WorkerWorkSubImageResponseDto>();
}

