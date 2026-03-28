using Microsoft.AspNetCore.Identity;

namespace Tadbeer.DAL.Models;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string City { get; set; } = null!;

    public string ProfileImage { get; set; } = null!;

    //public UserRole Role { get; set; } = UserRole.User;
    public UserStatus Status { get; set; } = UserStatus.Existed;

    // for worker
    public string? JobDescription { get; set; }
    public long? ExperienceYears { get; set; }
    public long? AvgRating { get; set; }
    
    public string? CodeResetPassword { get; set; }
    public DateTime? ExpirationCodeResetPassword { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigations
    public ICollection<PhoneNumber> PhoneNumbers { get; set; } = new List<PhoneNumber>();
    public ICollection<WorkImage> WorkImages { get; set; } = new List<WorkImage>();
    public ICollection<WorkingHours> WorkingHours { get; set; } = new List<WorkingHours>();

    public ICollection<Booking> UserBookings { get; set; } = new List<Booking>();    // as customer
    public ICollection<Booking> WorkerBookings { get; set; } = new List<Booking>();  // as worker

    public ICollection<AIDetection> AIDetections { get; set; } = new List<AIDetection>();

    public ICollection<WorkerSpecialty> WorkerSpecialties { get; set; } = new List<WorkerSpecialty>();
}