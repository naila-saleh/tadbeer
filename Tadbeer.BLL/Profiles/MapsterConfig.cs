using Mapster;
using Tadbeer.DAL.DTO.Requests;
using Tadbeer.DAL.DTO.Responses;
using Tadbeer.DAL.DTO.Responses.Profile;
using Tadbeer.DAL.DTO.Responses.WorkImages;
using Tadbeer.DAL.Models;

namespace Tadbeer.BLL.Profiles;

public static class MapsterConfig
{
    private const string DefaultSpecialtyIconUrl = "specialty-icons/default.png";

    public static void RegisterMappings()
    {
        // Example configuration (Mapster handles basic properties by matching names automatically):
        // TypeAdapterConfig<ApplicationUserRequestDto, ApplicationUser>
        //     .NewConfig()
        //     .IgnoreNullValues(true);

        // Add custom mapping rules here as your application grows

        TypeAdapterConfig<ApplicationUser, ApplicationUserResponseDto>
            .NewConfig()
            .Map(dest => dest.Email, src => src.Email ?? string.Empty)
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.Role, _ => UserRole.User);

        TypeAdapterConfig<ApplicationUser, UserProfileResponseDto>
            .NewConfig()
            .Map(dest => dest.Email, src => src.Email ?? string.Empty)
            .Map(dest => dest.Role, _ => UserRole.User)
            .Map(dest => dest.Status, src => src.Status.ToString());

        TypeAdapterConfig<ApplicationUser, WorkerProfileResponseDto>
            .NewConfig()
            .Map(dest => dest.Email, src => src.Email ?? string.Empty)
            .Map(dest => dest.Role, _ => UserRole.Worker)
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.SpecialtyIds,
                src => src.WorkerSpecialties.Select(ws => ws.SpecialtyId).Distinct().ToList())
            .Map(dest => dest.SpecialtyNames,
                src => src.WorkerSpecialties
                    .Select(ws => ws.Specialty.Name)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList());

        TypeAdapterConfig<ApplicationUser, WorkerPublicProfileResponseDto>
            .NewConfig()
            .Map(dest => dest.SpecialtyIds,
                src => src.WorkerSpecialties.Select(ws => ws.SpecialtyId).Distinct().ToList())
            .Map(dest => dest.SpecialtyNames,
                src => src.WorkerSpecialties
                    .Select(ws => ws.Specialty.Name)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList());

        TypeAdapterConfig<ApplicationUser, AdminProfileResponseDto>
            .NewConfig()
            .Map(dest => dest.Email, src => src.Email ?? string.Empty)
            .Map(dest => dest.Role, _ => UserRole.Admin)
            .Map(dest => dest.Status, src => src.Status.ToString());

        TypeAdapterConfig<ApplicationUser, SuperAdminProfileResponseDto>
            .NewConfig()
            .Map(dest => dest.Email, src => src.Email ?? string.Empty)
            .Map(dest => dest.Role, _ => UserRole.SuperAdmin)
            .Map(dest => dest.Status, src => src.Status.ToString());

        TypeAdapterConfig<WorkImage, WorkImageCreatedResponseDto>
            .NewConfig();

        TypeAdapterConfig<Specialty, SpecialtyResponseDto>
            .NewConfig()
            .Map(dest => dest.IconUrl,
                src => string.IsNullOrWhiteSpace(src.Icon)
                    ? DefaultSpecialtyIconUrl
                    : src.Icon);

        TypeAdapterConfig<BookingRequestDto, Booking>
            .NewConfig();

        TypeAdapterConfig<BookingUpdateRequestDto, Booking>
            .NewConfig()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.UserId)
            .Ignore(dest => dest.WorkerId)
            .Ignore(dest => dest.Status)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.User)
            .Ignore(dest => dest.Worker)
            .Ignore(dest => dest.WorkingHour)
            .Ignore(dest => dest.Review);

        TypeAdapterConfig<Booking, BookingResponseDto>
            .NewConfig()
            .Map(dest => dest.DurationMinutes,
                src => (int)Math.Max(0, (src.EndTime.ToTimeSpan() - src.StartTime.ToTimeSpan()).TotalMinutes))
            .Map(dest => dest.UserName,
                src => $"{src.User.FirstName} {src.User.LastName}".Trim())
            .Map(dest => dest.WorkerName,
                src => $"{src.Worker.FirstName} {src.Worker.LastName}".Trim())
            .Map(dest => dest.WorkingDay, src => src.WorkingHour.DayOfWeek)
            .Map(dest => dest.WorkingHourStart, src => src.WorkingHour.StartTime)
            .Map(dest => dest.WorkingHourEnd, src => src.WorkingHour.EndTime);

        TypeAdapterConfig<ReviewRequestDto, Review>
            .NewConfig();

        TypeAdapterConfig<ReviewUpdateRequestDto, Review>
            .NewConfig()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.BookingId)
            .Ignore(dest => dest.Booking)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt);

        TypeAdapterConfig<Review, ReviewResponseDto>
            .NewConfig()
            .Map(dest => dest.UserId, src => src.Booking.UserId)
            .Map(dest => dest.WorkerId, src => src.Booking.WorkerId)
            .Map(dest => dest.UserName, src => $"{src.Booking.User.FirstName} {src.Booking.User.LastName}".Trim())
            .Map(dest => dest.WorkerName, src => $"{src.Booking.Worker.FirstName} {src.Booking.Worker.LastName}".Trim());
    }
}
