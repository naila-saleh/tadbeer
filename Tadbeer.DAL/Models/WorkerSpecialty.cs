namespace Tadbeer.DAL.Models;

public class WorkerSpecialty
{
    public Guid WorkerId { get; set; }
    public ApplicationUser Worker { get; set; } = null!;

    public Guid SpecialtyId { get; set; }
    public Specialty Specialty { get; set; } = null!;
}