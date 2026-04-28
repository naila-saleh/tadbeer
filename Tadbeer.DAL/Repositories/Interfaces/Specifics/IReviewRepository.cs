using Tadbeer.DAL.Models;

namespace Tadbeer.DAL.Repositories.Interfaces.Specifics;

public interface IReviewRepository : IGenericRepository<Review>
{
	Task<Review?> GetByIdWithDetailsAsync(Guid reviewId);
	Task<Review?> GetByBookingIdAsync(Guid bookingId);
	Task<Review?> GetByIdForUserAsync(Guid reviewId, Guid userId);
	Task<IEnumerable<Review>> GetByWorkerIdPagedAsync(Guid workerId, int skip, int take);
	Task<IEnumerable<Review>> GetAllByWorkerIdAsync(Guid workerId);
	Task<int> CountByWorkerIdAsync(Guid workerId);
}
