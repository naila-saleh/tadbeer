using Tadbeer.DAL.Data;
using Tadbeer.DAL.Models;
using Tadbeer.DAL.Repositories.Interfaces.Specifics;
using Microsoft.EntityFrameworkCore;

namespace Tadbeer.DAL.Repositories.Classes.Specifics;

public class BookingRepository : GenericRepository<Booking>, IBookingRepository
{
    public BookingRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Booking?> GetByIdWithDetailsAsync(Guid bookingId)
    {
        return await BuildDetailsQuery()
            .FirstOrDefaultAsync(b => b.Id == bookingId);
    }

    public async Task<Booking?> GetByIdForUserAsync(Guid bookingId, Guid userId)
    {
        return await BuildDetailsQuery()
            .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == userId);
    }

    public async Task<Booking?> GetByIdForWorkerAsync(Guid bookingId, Guid workerId)
    {
        return await BuildDetailsQuery()
            .FirstOrDefaultAsync(b => b.Id == bookingId && b.WorkerId == workerId);
    }

    public async Task<IEnumerable<Booking>> GetByUserIdAsync(Guid userId, int skip, int take)
    {
        return await BuildDetailsQuery()
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public Task<int> CountByUserIdAsync(Guid userId)
        => BuildDetailsQuery().CountAsync(b => b.UserId == userId);

    public async Task<IEnumerable<Booking>> GetByWorkerIdAsync(Guid workerId, int skip, int take)
    {
        return await BuildDetailsQuery()
            .Where(b => b.WorkerId == workerId)
            .OrderByDescending(b => b.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public Task<int> CountByWorkerIdAsync(Guid workerId)
        => BuildDetailsQuery().CountAsync(b => b.WorkerId == workerId);

    public async Task<IEnumerable<Booking>> GetPagedWithDetailsAsync(int skip, int take)
    {
        return await BuildDetailsQuery()
            .OrderByDescending(b => b.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public Task<int> CountAsync()
        => _context.Bookings.CountAsync();

    public Task<bool> WorkingHourBelongsToWorkerAsync(Guid workingHourId, Guid workerId)
        => _context.WorkingHours.AnyAsync(wh => wh.Id == workingHourId && wh.WorkerId == workerId);

    public Task<bool> HasActiveBookingConflictAsync(Guid workerId, DateTime bookingDate, TimeOnly startTime, TimeOnly endTime, Guid? ignoreBookingId = null)
    {
        var dayStart = bookingDate.Date;
        var dayEnd = dayStart.AddDays(1);

        return _context.Bookings.AnyAsync(b =>
            b.WorkerId == workerId
            && b.BookingDate >= dayStart
            && b.BookingDate < dayEnd
            && b.StartTime < endTime
            && b.EndTime > startTime
            && b.Status == BookingStatus.Accepted
            && (!ignoreBookingId.HasValue || b.Id != ignoreBookingId.Value));
    }

    private IQueryable<Booking> BuildDetailsQuery()
    {
        return _context.Bookings
            .Include(b => b.User)
            .Include(b => b.Worker)
            .Include(b => b.WorkingHour)
            .AsNoTracking();
    }
}
