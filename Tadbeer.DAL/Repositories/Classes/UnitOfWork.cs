using System;
using System.Threading.Tasks;
using Tadbeer.DAL.Data;
using Tadbeer.DAL.Repositories.Classes.Specifics;
using Tadbeer.DAL.Repositories.Interfaces;
using Tadbeer.DAL.Repositories.Interfaces.Specifics;

namespace Tadbeer.DAL.Repositories.Classes;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public IApplicationUserRepository ApplicationUsers { get; private set; }
    public ISpecialtyRepository Specialties { get; private set; }
    public IWorkerSpecialtyRepository WorkerSpecialties { get; private set; }
    public IPhoneNumberRepository PhoneNumbers { get; private set; }
    public IWorkImageRepository WorkImages { get; private set; }
    public IWorkSubImageRepository WorkSubImages { get; private set; }
    public IWorkingHoursRepository WorkingHours { get; private set; }
    public IBookingRepository Bookings { get; private set; }
    public IReviewRepository Reviews { get; private set; }
    public IAIDetectionRepository AIDetections { get; private set; }

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;

        ApplicationUsers = new ApplicationUserRepository(_context);
        Specialties = new SpecialtyRepository(_context);
        WorkerSpecialties = new WorkerSpecialtyRepository(_context);
        PhoneNumbers = new PhoneNumberRepository(_context);
        WorkImages = new WorkImageRepository(_context);
        WorkSubImages = new WorkSubImageRepository(_context);
        WorkingHours = new WorkingHoursRepository(_context);
        Bookings = new BookingRepository(_context);
        Reviews = new ReviewRepository(_context);
        AIDetections = new AIDetectionRepository(_context);
    }

    public async Task<int> CompleteAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
