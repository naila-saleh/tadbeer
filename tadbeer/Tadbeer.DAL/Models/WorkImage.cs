namespace Tadbeer.DAL.Models;

public class WorkImage
{
    public Guid Id { get; set; }
    public string ImageUrl { get; set; } = null!;

    public Guid WorkerId { get; set; }
    public ApplicationUser Worker { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<WorkSubImage> SubImages { get; set; } = new List<WorkSubImage>();
}