namespace Tadbeer.DAL.Models;

public class Booking
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public Guid WorkerId { get; set; }
    public ApplicationUser Worker { get; set; } = null!;

    public DateTime BookingDate { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Pending;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Guid WorkingHourId { get; set; }
    public WorkingHours WorkingHour { get; set; } = null!;

    public Review? Review { get; set; }
}