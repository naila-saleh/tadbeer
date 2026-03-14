using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Tadbeer.DAL.Models;

namespace Tadbeer.DAL.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Design-time fallback: only runs when EF Tools create migrations
    // and no options have been injected (IsConfigured = false).
    // At runtime this block is NEVER reached because Program.cs always passes options.
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(
                "Server=.;Database=TadbeerDb;Trusted_Connection=True;TrustServerCertificate=True;");
        }
    }

    public DbSet<Specialty> Specialties => Set<Specialty>();
    public DbSet<WorkerSpecialty> WorkerSpecialties => Set<WorkerSpecialty>();
    public DbSet<PhoneNumber> PhoneNumbers => Set<PhoneNumber>();
    public DbSet<WorkImage> WorkImages => Set<WorkImage>();
    public DbSet<WorkSubImage> WorkSubImages => Set<WorkSubImage>();
    public DbSet<WorkingHours> WorkingHours => Set<WorkingHours>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<AIDetection> AIDetections => Set<AIDetection>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(e =>
        {
            e.Property(x => x.FirstName).HasMaxLength(255).IsRequired();
            e.Property(x => x.LastName).HasMaxLength(255).IsRequired();
            e.Property(x => x.City).HasMaxLength(255).IsRequired();
            e.Property(x => x.ProfileImage).HasMaxLength(255).IsRequired();

            e.Property(x => x.Role)
                .HasConversion<string>()
                .HasMaxLength(255)
                .HasDefaultValue(UserRole.User);

            e.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(255)
                .HasDefaultValue(UserStatus.Existed);

            e.HasIndex(x => new { x.Role, x.Status });
            e.HasIndex(x => new { x.City, x.Role });
            e.HasIndex(x => x.AvgRating);
            e.HasIndex(x => x.City);

            e.HasCheckConstraint(
                "CK_ApplicationUser_Role",
                "[Role] IN ('Admin','Worker','User')"
            );

            e.HasCheckConstraint(
                "CK_ApplicationUser_Status",
                "[Status] IN ('Existed','Deleted')"
            );
        });

        builder.Entity<Specialty>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(255).IsRequired();
        });

        builder.Entity<WorkerSpecialty>(e =>
        {
            e.HasKey(x => new { x.WorkerId, x.SpecialtyId });

            e.HasIndex(x => x.SpecialtyId);

            e.HasOne(x => x.Worker)
                .WithMany(x => x.WorkerSpecialties)
                .HasForeignKey(x => x.WorkerId);

            e.HasOne(x => x.Specialty)
                .WithMany(x => x.WorkerSpecialties)
                .HasForeignKey(x => x.SpecialtyId);
        });

        builder.Entity<PhoneNumber>(e =>
        {
            e.Property(x => x.Number).HasMaxLength(255).IsRequired();
            e.HasIndex(x => x.Number).IsUnique();

            e.HasIndex(x => x.WorkerId);

            e.HasOne(x => x.Worker)
                .WithMany(x => x.PhoneNumbers)
                .HasForeignKey(x => x.WorkerId);
        });

        builder.Entity<WorkImage>(e =>
        {
            e.Property(x => x.ImageUrl).HasMaxLength(255).IsRequired();

            e.HasIndex(x => x.WorkerId);

            e.HasOne(x => x.Worker)
                .WithMany(x => x.WorkImages)
                .HasForeignKey(x => x.WorkerId);
        });

        builder.Entity<WorkSubImage>(e =>
        {
            e.Property(x => x.ImageUrl).HasMaxLength(255).IsRequired();

            e.HasIndex(x => x.MainImageId);

            e.HasOne(x => x.MainImage)
                .WithMany(x => x.SubImages)
                .HasForeignKey(x => x.MainImageId);
        });

        builder.Entity<WorkingHours>(e =>
        {
            e.Property(x => x.DayOfWeek)
                .HasConversion<string>()
                .HasMaxLength(255)
                .IsRequired();

            e.HasOne(x => x.Worker)
                .WithMany(x => x.WorkingHours)
                .HasForeignKey(x => x.WorkerId);

            e.HasCheckConstraint(
                "CK_WorkingHours_DayOfWeek",
                "[DayOfWeek] IN ('Saturday','Sunday','Monday','Tuesday','Wednesday','Thursday','Friday')"
            );
        });

        builder.Entity<Booking>(e =>
        {
            e.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(255)
                .HasDefaultValue(BookingStatus.Pending);

            e.HasIndex(x => x.WorkerId);
            e.HasIndex(x => x.UserId);
            e.HasIndex(x => x.Status);

            e.HasCheckConstraint(
                "CK_Bookings_Status",
                "[Status] IN ('Pending','Accepted','Rejected','Completed','Cancelled')"
            );

            e.HasOne(x => x.User)
                .WithMany(x => x.UserBookings)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Worker)
                .WithMany(x => x.WorkerBookings)
                .HasForeignKey(x => x.WorkerId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.WorkingHour)
                .WithMany(x => x.Bookings)
                .HasForeignKey(x => x.WorkingHourId);
        });

        builder.Entity<Review>(e =>
        {
            e.HasIndex(x => x.BookingId).IsUnique();

            e.Property(x => x.Rate).IsRequired();

            e.HasCheckConstraint("CK_Review_Rate", "[Rate] BETWEEN 1 AND 5");

            e.HasOne(x => x.Booking)
                .WithOne(x => x.Review)
                .HasForeignKey<Review>(x => x.BookingId);
        });

        builder.Entity<AIDetection>(e =>
        {
            e.Property(x => x.ImagePath).HasMaxLength(255).IsRequired();

            e.Property(x => x.ConfidenceScore)
                .HasPrecision(5, 2);

            e.HasOne(x => x.User)
                .WithMany(x => x.AIDetections)
                .HasForeignKey(x => x.UserId);

            e.HasOne(x => x.PredictedSpecialty)
                .WithMany(x => x.AIDetections)
                .HasForeignKey(x => x.PredictedSpecialtyId);
        });
    }
}