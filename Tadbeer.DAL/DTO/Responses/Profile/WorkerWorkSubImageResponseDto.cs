namespace Tadbeer.DAL.DTO.Responses.Profile;

public class WorkerWorkSubImageResponseDto
{
    public Guid Id { get; set; }
    public Guid MainImageId { get; set; }
    public string ImageUrl { get; set; } = null!;
}

