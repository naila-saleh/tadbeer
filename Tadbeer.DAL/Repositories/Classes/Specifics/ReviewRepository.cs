using Tadbeer.DAL.Data;
using Tadbeer.DAL.Models;
using Tadbeer.DAL.Repositories.Interfaces.Specifics;
using Microsoft.EntityFrameworkCore;

namespace Tadbeer.DAL.Repositories.Classes.Specifics;

public class ReviewRepository : GenericRepository<Review>, IReviewRepository
{
    public ReviewRepository(ApplicationDbContext context) : base(context)
    {
    }

    public Task<Review?> GetByIdWithDetailsAsync(Guid reviewId)
        => BuildDetailsQuery().FirstOrDefaultAsync(r => r.Id == reviewId);

    public Task<Review?> GetByBookingIdAsync(Guid bookingId)
        => BuildDetailsQuery().FirstOrDefaultAsync(r => r.BookingId == bookingId);

    public Task<Review?> GetByIdForUserAsync(Guid reviewId, Guid userId)
        => BuildDetailsQuery().FirstOrDefaultAsync(r => r.Id == reviewId && r.Booking.UserId == userId);

    public async Task<IEnumerable<Review>> GetByWorkerIdPagedAsync(Guid workerId, int skip, int take)
    {
        return await BuildDetailsQuery()
            .Where(r => r.Booking.WorkerId == workerId)
            .OrderByDescending(r => r.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public Task<int> CountByWorkerIdAsync(Guid workerId)
        => _context.Reviews.CountAsync(r => r.Booking.WorkerId == workerId);

    private IQueryable<Review> BuildDetailsQuery()
    {
        return _context.Reviews
            .Include(r => r.Booking)
            .ThenInclude(b => b.User)
            .Include(r => r.Booking)
            .ThenInclude(b => b.Worker)
            .AsNoTracking();
    }
}
