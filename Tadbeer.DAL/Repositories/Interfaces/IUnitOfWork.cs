using System;
using System.Threading.Tasks;
using Tadbeer.DAL.Repositories.Interfaces.Specifics;

namespace Tadbeer.DAL.Repositories.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IApplicationUserRepository ApplicationUsers { get; }
    ISpecialtyRepository Specialties { get; }
    IWorkerSpecialtyRepository WorkerSpecialties { get; }
    IPhoneNumberRepository PhoneNumbers { get; }
    IWorkImageRepository WorkImages { get; }
    IWorkSubImageRepository WorkSubImages { get; }
    IWorkingHoursRepository WorkingHours { get; }
    IBookingRepository Bookings { get; }
    IReviewRepository Reviews { get; }
    IAIDetectionRepository AIDetections { get; }

    Task<int> CompleteAsync();
}
