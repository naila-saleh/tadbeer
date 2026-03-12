namespace Tadbeer.DAL.Models;

public class PhoneNumber
{
    public Guid Id { get; set; }
    public string Number { get; set; } = null!;

    public Guid WorkerId { get; set; }
    public ApplicationUser Worker { get; set; } = null!;
}