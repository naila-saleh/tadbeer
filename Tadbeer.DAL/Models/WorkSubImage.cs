namespace Tadbeer.DAL.Models;

public class WorkSubImage
{
    public Guid Id { get; set; }

    public Guid MainImageId { get; set; }
    public WorkImage MainImage { get; set; } = null!;

    public string ImageUrl { get; set; } = null!;
}