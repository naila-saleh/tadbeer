namespace Tadbeer.DAL.DTO.Responses.WorkImages;

public class WorkImageCreatedResponseDto
{
    public Guid Id { get; set; }
    public string ImageUrl { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

