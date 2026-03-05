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

        // Rename ALL Identity tables (no more AspNet* defaults)
        builder.Entity<ApplicationUser>().ToTable("ApplicationUser");
        builder.Entity<IdentityRole<Guid>>().ToTable("Role");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRole");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaim");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogin");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaim");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("UserToken");

        // Optional but recommended: keep composite keys explicit (Identity config already does this,
        // but these lines make it crystal-clear and prevent “mystery key” confusion later)
        builder.Entity<IdentityUserRole<Guid>>().HasKey(x => new { x.UserId, x.RoleId });
        builder.Entity<IdentityUserLogin<Guid>>().HasKey(x => new { x.LoginProvider, x.ProviderKey });
        builder.Entity<IdentityUserToken<Guid>>().HasKey(x => new { x.UserId, x.LoginProvider, x.Name });

        builder.Entity<ApplicationUser>(e =>
        {
            e.ToTable("ApplicationUser");

            e.Property(x => x.UserName).HasColumnName("username").HasMaxLength(255);
            e.Property(x => x.NormalizedUserName).HasColumnName("username_normalized").HasMaxLength(255);

            e.Property(x => x.Email).HasColumnName("email").HasMaxLength(255);
            e.Property(x => x.NormalizedEmail).HasColumnName("email_normalized").HasMaxLength(255);

            e.Property(x => x.PasswordHash).HasColumnName("password_hash").HasMaxLength(255);

            e.Property(x => x.FirstName).HasColumnName("first_name").HasMaxLength(255).IsRequired();
            e.Property(x => x.LastName).HasColumnName("last_name").HasMaxLength(255).IsRequired();
            e.Property(x => x.City).HasColumnName("city").HasMaxLength(255).IsRequired();
            e.Property(x => x.ProfileImage).HasColumnName("profile_image").HasMaxLength(255).IsRequired();

            e.Property(x => x.Role)
                .HasColumnName("role")
                .HasConversion<string>()
                .HasMaxLength(255)
                .HasDefaultValue(UserRole.User);

            e.Property(x => x.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(255)
                .HasDefaultValue(UserStatus.Existed);

            e.Property(x => x.JobDescription).HasColumnName("job_description");
            e.Property(x => x.ExperienceYears).HasColumnName("experience_years");
            e.Property(x => x.AvgRating).HasColumnName("avg_rating");

            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at");

            e.HasIndex(x => new { x.Role, x.Status }).HasDatabaseName("applicationuser_role_status_index");
            e.HasIndex(x => new { x.City, x.Role }).HasDatabaseName("applicationuser_city_role_index");
            e.HasIndex(x => x.AvgRating).HasDatabaseName("applicationuser_avg_rating_index");
            e.HasIndex(x => x.City).HasDatabaseName("applicationuser_city_index");

            e.HasIndex(x => x.UserName).IsUnique().HasDatabaseName("applicationuser_username_unique");
            e.HasIndex(x => x.Email).IsUnique().HasDatabaseName("applicationuser_email_unique");

            e.HasCheckConstraint("CK_ApplicationUser_Role", "[role] IN ('admin','worker','user')");
            e.HasCheckConstraint("CK_ApplicationUser_Status", "[status] IN ('existed','deleted')");
        });

        builder.Entity<Specialty>(e =>
        {
            e.ToTable("Specialty");
            e.HasKey(x => x.Id).HasName("specialty_id_primary");
            e.Property(x => x.Name).HasColumnName("name").HasMaxLength(255).IsRequired();
        });

        builder.Entity<WorkerSpecialty>(e =>
        {
            e.ToTable("WorkerSpecialty");

            e.HasKey(x => new { x.WorkerId, x.SpecialtyId });

            e.HasIndex(x => x.SpecialtyId).HasDatabaseName("workerspecialty_specialty_id_index");

            e.Property(x => x.WorkerId).HasColumnName("worker_id");
            e.Property(x => x.SpecialtyId).HasColumnName("specialty_id");

            e.HasOne(x => x.Worker)
                .WithMany(x => x.WorkerSpecialties)
                .HasForeignKey(x => x.WorkerId);

            e.HasOne(x => x.Specialty)
                .WithMany(x => x.WorkerSpecialties)
                .HasForeignKey(x => x.SpecialtyId);
        });

        builder.Entity<PhoneNumber>(e =>
        {
            e.ToTable("PhoneNumber");
            e.HasKey(x => x.Id).HasName("phonenumber_id_primary");

            e.Property(x => x.Number).HasColumnName("phone_number").HasMaxLength(255).IsRequired();
            e.HasIndex(x => x.Number).IsUnique().HasDatabaseName("phonenumber_phone_number_unique");

            e.Property(x => x.WorkerId).HasColumnName("worker_id");
            e.HasIndex(x => x.WorkerId).HasDatabaseName("phonenumber_worker_id_index");

            e.HasOne(x => x.Worker)
                .WithMany(x => x.PhoneNumbers)
                .HasForeignKey(x => x.WorkerId);
        });

        builder.Entity<WorkImage>(e =>
        {
            e.ToTable("WorkImage");
            e.HasKey(x => x.Id).HasName("workimage_id_primary");

            e.Property(x => x.ImageUrl).HasColumnName("image_url").HasMaxLength(255).IsRequired();
            e.Property(x => x.WorkerId).HasColumnName("worker_id");

            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at");

            e.HasIndex(x => x.WorkerId).HasDatabaseName("workimage_worker_id_index");

            e.HasOne(x => x.Worker)
                .WithMany(x => x.WorkImages)
                .HasForeignKey(x => x.WorkerId);
        });

        builder.Entity<WorkSubImage>(e =>
        {
            e.ToTable("WorkSubImages");
            e.HasKey(x => x.Id).HasName("worksubimages_id_primary");

            e.Property(x => x.MainImageId).HasColumnName("main_image_id");
            e.Property(x => x.ImageUrl).HasColumnName("image_url").HasMaxLength(255).IsRequired();

            e.HasIndex(x => x.MainImageId).HasDatabaseName("worksubimages_main_image_id_index");

            e.HasOne(x => x.MainImage)
                .WithMany(x => x.SubImages)
                .HasForeignKey(x => x.MainImageId);
        });

        builder.Entity<WorkingHours>(e =>
        {
            e.ToTable("WorkingHours");
            e.HasKey(x => x.Id).HasName("workinghours_id_primary");

            e.Property(x => x.WorkerId).HasColumnName("worker_id");

            e.Property(x => x.DayOfWeek)
                .HasColumnName("day_of_week")
                .HasConversion<string>()
                .HasMaxLength(255)
                .IsRequired();

            e.Property(x => x.StartTime).HasColumnName("start_time");
            e.Property(x => x.EndTime).HasColumnName("end_time");

            e.HasCheckConstraint(
                "CK_WorkingHours_DayOfWeek",
                "[day_of_week] IN ('Saturday','Sunday','Monday','Tuesday','Wednesday','Thursday','Friday')"
            );

            e.HasOne(x => x.Worker)
                .WithMany(x => x.WorkingHours)
                .HasForeignKey(x => x.WorkerId);
        });

        builder.Entity<Booking>(e =>
        {
            e.ToTable("Bookings");
            e.HasKey(x => x.Id).HasName("bookings_id_primary");

            e.Property(x => x.UserId).HasColumnName("user_id");
            e.Property(x => x.WorkerId).HasColumnName("worker_id");
            e.Property(x => x.BookingDate).HasColumnName("booking_date");

            e.Property(x => x.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(255)
                .HasDefaultValue(BookingStatus.Pending);

            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at");

            e.Property(x => x.WorkingHourId).HasColumnName("working_hour_id");

            e.HasIndex(x => x.WorkerId).HasDatabaseName("bookings_worker_id_index");
            e.HasIndex(x => x.UserId).HasDatabaseName("bookings_user_id_index");
            e.HasIndex(x => x.Status).HasDatabaseName("bookings_status_index");

            e.HasCheckConstraint(
                "CK_Bookings_Status",
                "[status] IN ('pending','accepted','rejected','completed','cancelled')"
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
            e.ToTable("Review");
            e.HasKey(x => x.Id).HasName("review_id_primary");

            e.Property(x => x.BookingId).HasColumnName("booking_id");
            e.HasIndex(x => x.BookingId).IsUnique().HasDatabaseName("review_booking_id_unique");
            e.HasIndex(x => x.BookingId).HasDatabaseName("review_booking_id_index");

            e.Property(x => x.Rate).HasColumnName("rate").IsRequired();
            e.Property(x => x.Comment).HasColumnName("comment");

            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at");

            e.HasCheckConstraint("CK_Review_Rate", "[rate] BETWEEN 1 AND 5");

            e.HasOne(x => x.Booking)
                .WithOne(x => x.Review)
                .HasForeignKey<Review>(x => x.BookingId);
        });

        builder.Entity<AIDetection>(e =>
        {
            e.ToTable("AIDetection");
            e.HasKey(x => x.Id).HasName("aidetection_id_primary");

            e.Property(x => x.UserId).HasColumnName("user_id");
            e.Property(x => x.ImagePath).HasColumnName("image_path").HasMaxLength(255).IsRequired();

            e.Property(x => x.PredictedSpecialtyId).HasColumnName("predicted_specialty_id");

            e.Property(x => x.ConfidenceScore)
                .HasColumnName("confidence_score")
                .HasPrecision(5, 2);

            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at");

            e.HasOne(x => x.User)
                .WithMany(x => x.AIDetections)
                .HasForeignKey(x => x.UserId);

            e.HasOne(x => x.PredictedSpecialty)
                .WithMany(x => x.AIDetections)
                .HasForeignKey(x => x.PredictedSpecialtyId);
        });
    }
}