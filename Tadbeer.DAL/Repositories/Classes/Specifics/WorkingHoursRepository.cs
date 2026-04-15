using Tadbeer.DAL.Data;
using Tadbeer.DAL.Models;
using Tadbeer.DAL.Repositories.Interfaces.Specifics;
using Microsoft.EntityFrameworkCore;

namespace Tadbeer.DAL.Repositories.Classes.Specifics;

public class WorkingHoursRepository : GenericRepository<WorkingHours>, IWorkingHoursRepository
{
    public WorkingHoursRepository(ApplicationDbContext context) : base(context)
    {
    }

    public Task<WorkingHours?> GetByIdAsync(Guid id)
        => _context.WorkingHours.FirstOrDefaultAsync(wh => wh.Id == id);

    public async Task<IEnumerable<WorkingHours>> GetByWorkerIdAsync(Guid workerId, int skip, int take)
    {
        return await _context.WorkingHours
            .AsNoTracking()
            .Where(wh => wh.WorkerId == workerId)
            .OrderBy(wh => wh.DayOfWeek)
            .ThenBy(wh => wh.StartTime)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public Task<int> CountByWorkerIdAsync(Guid workerId)
        => _context.WorkingHours.CountAsync(wh => wh.WorkerId == workerId);

    public Task<bool> HasOverlapAsync(Guid workerId, WeekDay dayOfWeek, TimeOnly startTime, TimeOnly endTime, Guid? ignoreWorkingHourId = null)
    {
        return _context.WorkingHours.AnyAsync(wh =>
            wh.WorkerId == workerId
            && wh.DayOfWeek == dayOfWeek
            && wh.StartTime < endTime
            && wh.EndTime > startTime
            && (!ignoreWorkingHourId.HasValue || wh.Id != ignoreWorkingHourId.Value));
    }

    public Task<bool> HasBookingsAsync(Guid workingHourId)
        => _context.Bookings.AnyAsync(b => b.WorkingHourId == workingHourId);
}
