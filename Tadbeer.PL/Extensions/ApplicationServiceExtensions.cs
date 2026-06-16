using Microsoft.AspNetCore.Identity.UI.Services;
using Tadbeer.BLL.Services.Classes;
using Tadbeer.BLL.Services.Classes.Specifics;
using Tadbeer.BLL.Services.Interfaces;
using Tadbeer.BLL.Services.Interfaces.Specifics;
using Tadbeer.DAL.Repositories.Classes;
using Tadbeer.DAL.Repositories.Interfaces;
using Tadbeer.DAL.Utilities;
using Tadbeer.PL.Utilities;

namespace Tadbeer.PL.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // 1. Register UnitOfWork and Generic Repository
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // 2. Register Specific Services
        services.AddScoped<IApplicationUserService, ApplicationUserService>();
        services.AddScoped<ISpecialtyService, SpecialtyService>();
        services.AddScoped<IWorkerSpecialtyService, WorkerSpecialtyService>();
        services.AddScoped<IPhoneNumberService, PhoneNumberService>();
        services.AddScoped<IWorkImageService, WorkImageService>();
        services.AddScoped<IWorkSubImageService, WorkSubImageService>();
        services.AddScoped<IWorkingHoursService, WorkingHoursService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IAIDetectionService, AIDetectionService>();
        services.AddScoped<IGeminiChatService, GeminiChatService>();
        services.AddScoped<IEmailSender, EmailSetting>();
        services.AddScoped<IGenerateJWTService, GenerateJWTService>();
        services.AddScoped<ISeedData, SeedData>();
        // Register file storage service
        services.AddScoped<IFileStorageService, FileStorageService>();
        
        services.AddScoped<IAuthService, AuthService>();

        // Named HttpClient for the external AI prediction endpoint
        // Timeout is 5 min to handle Render cold-start (free tier sleeps between requests)
        services.AddHttpClient("AIModel", c =>
        {
            c.BaseAddress = new Uri("https://projectai-drsx.onrender.com/");
            c.Timeout = TimeSpan.FromMinutes(5);
        });

        services.AddHttpClient("Gemini", c =>
        {
            c.BaseAddress = new Uri("https://generativelanguage.googleapis.com/");
            c.Timeout = TimeSpan.FromSeconds(60);
        });

        // Typed HttpClient for Nominatim reverse geocoding (OpenStreetMap)
        services.AddHttpClient<IReverseGeocodingService, NominatimReverseGeocodingService>()
            .ConfigureHttpClient(c =>
            {
                c.BaseAddress = new Uri("https://nominatim.openstreetmap.org");
                c.DefaultRequestHeaders.Add("User-Agent", "Tadbeer-App/1.0");
                c.Timeout = TimeSpan.FromSeconds(10);
            });

        return services;
    }
}
