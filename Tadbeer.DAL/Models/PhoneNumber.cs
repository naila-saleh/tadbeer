namespace Tadbeer.DAL.Models;

public class PhoneNumber
{
    public Guid Id { get; set; }
    public string Number { get; set; } = null!;

    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}