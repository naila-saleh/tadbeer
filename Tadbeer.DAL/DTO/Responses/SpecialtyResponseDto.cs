namespace Tadbeer.DAL.DTO.Responses;

public class SpecialtyResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
}
