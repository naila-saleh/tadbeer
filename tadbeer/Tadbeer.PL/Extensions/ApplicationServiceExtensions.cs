using Microsoft.Extensions.DependencyInjection;
using Tadbeer.BLL.Services.Classes.Specifics;
using Tadbeer.BLL.Services.Interfaces.Specifics;
using Tadbeer.DAL.Repositories.Classes;
using Tadbeer.DAL.Repositories.Interfaces;

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

        return services;
    }
}
