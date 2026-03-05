namespace Tadbeer.DAL.Models;

public class WorkingHours
{
    public Guid Id { get; set; }

    public Guid WorkerId { get; set; }
    public ApplicationUser Worker { get; set; } = null!;

    public WeekDay DayOfWeek { get; set; }

    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}