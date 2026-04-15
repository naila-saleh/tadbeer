using Tadbeer.DAL.Models;

namespace Tadbeer.DAL.Repositories.Interfaces.Specifics;

public interface IBookingRepository : IGenericRepository<Booking>
{
	Task<Booking?> GetByIdWithDetailsAsync(Guid bookingId);
	Task<Booking?> GetByIdForUserAsync(Guid bookingId, Guid userId);
	Task<Booking?> GetByIdForWorkerAsync(Guid bookingId, Guid workerId);
	Task<IEnumerable<Booking>> GetByUserIdAsync(Guid userId, int skip, int take);
	Task<int> CountByUserIdAsync(Guid userId);
	Task<IEnumerable<Booking>> GetByWorkerIdAsync(Guid workerId, int skip, int take);
	Task<int> CountByWorkerIdAsync(Guid workerId);
	Task<IEnumerable<Booking>> GetPagedWithDetailsAsync(int skip, int take);
	Task<int> CountAsync();
	Task<bool> WorkingHourBelongsToWorkerAsync(Guid workingHourId, Guid workerId);
	Task<bool> HasActiveBookingConflictAsync(Guid workerId, DateTime bookingDate, TimeOnly startTime, TimeOnly endTime, Guid? ignoreBookingId = null);
}
