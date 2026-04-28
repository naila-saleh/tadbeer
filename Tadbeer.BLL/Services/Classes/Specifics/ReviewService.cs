using Tadbeer.BLL.Services.Interfaces.Specifics;
using Tadbeer.DAL.DTO.Requests;
using Tadbeer.DAL.DTO.Responses;
using Tadbeer.DAL.Models;
using Tadbeer.DAL.Repositories.Interfaces;
using Tadbeer.BLL.Exceptions;
using Mapster;

namespace Tadbeer.BLL.Services.Classes.Specifics;

public class ReviewService : GenericService<ReviewRequestDto, ReviewResponseDto, Review>, IReviewService
{
    public ReviewService(IUnitOfWork unitOfWork) : base(unitOfWork, unitOfWork.Reviews)
    {
    }

    public async Task<ReviewResponseDto> CreateForUserAsync(Guid userId, ReviewRequestDto dto)
    {
        var booking = await _unitOfWork.Bookings.GetByIdAsync(dto.BookingId);
        if (booking == null)
        {
            throw new UserOperationException("Booking not found.");
        }

        if (booking.UserId != userId)
        {
            throw new UserOperationException("You can only review your own bookings.");
        }

        if (booking.Status != BookingStatus.Completed)
        {
            throw new UserOperationException("Review can only be added for completed bookings.");
        }

        var existingReview = await _unitOfWork.Reviews.GetByBookingIdAsync(dto.BookingId);
        if (existingReview != null)
        {
            throw new UserOperationException("This booking already has a review.");
        }

        var now = DateTime.UtcNow;
        var review = dto.Adapt<Review>();
        review.Id = Guid.NewGuid();
        review.CreatedAt = now;
        review.UpdatedAt = now;

        await _unitOfWork.Reviews.AddAsync(review);
        await _unitOfWork.CompleteAsync();


        var created = await _unitOfWork.Reviews.GetByIdWithDetailsAsync(review.Id);
        return (created ?? review).Adapt<ReviewResponseDto>();
    }

    public async Task<ReviewResponseDto?> UpdateOwnAsync(Guid userId, Guid reviewId, ReviewUpdateRequestDto dto)
    {
        var review = await _unitOfWork.Reviews.GetByIdAsync(reviewId);
        if (review == null)
        {
            return null;
        }

        var booking = await _unitOfWork.Bookings.GetByIdAsync(review.BookingId);
        if (booking == null || booking.UserId != userId)
        {
            return null;
        }

        dto.Adapt(review);
        review.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Reviews.Update(review);
        await _unitOfWork.CompleteAsync();


        var updated = await _unitOfWork.Reviews.GetByIdWithDetailsAsync(reviewId);
        return updated?.Adapt<ReviewResponseDto>();
    }

    public async Task<bool> DeleteOwnAsync(Guid userId, Guid reviewId)
    {
        var review = await _unitOfWork.Reviews.GetByIdAsync(reviewId);
        if (review == null)
        {
            return false;
        }

        var booking = await _unitOfWork.Bookings.GetByIdAsync(review.BookingId);
        if (booking == null || booking.UserId != userId)
        {
            return false;
        }

        _unitOfWork.Reviews.Remove(review);
        await _unitOfWork.CompleteAsync();


        return true;
    }

    public async Task<bool> DeleteAnyAsync(Guid reviewId)
    {
        var review = await _unitOfWork.Reviews.GetByIdWithDetailsAsync(reviewId);
        if (review == null)
        {
            return false;
        }

        _unitOfWork.Reviews.Remove(review);
        await _unitOfWork.CompleteAsync();


        return true;
    }

    public async Task<ReviewPagedResponseDto> GetByWorkerPagedAsync(Guid workerId, int pageNumber, int pageSize)
    {
        var (safePageNumber, safePageSize, skip) = NormalizePaging(pageNumber, pageSize);
        var reviews = await _unitOfWork.Reviews.GetByWorkerIdPagedAsync(workerId, skip, safePageSize);
        var total = await _unitOfWork.Reviews.CountByWorkerIdAsync(workerId);

        return new ReviewPagedResponseDto
        {
            Items = reviews.Adapt<List<ReviewResponseDto>>(),
            TotalCount = total,
            PageNumber = safePageNumber,
            PageSize = safePageSize
        };
    }
    
    private static (int PageNumber, int PageSize, int Skip) NormalizePaging(int pageNumber, int pageSize)
    {
        var safePageNumber = pageNumber < 1 ? 1 : pageNumber;
        var safePageSize = pageSize is < 1 or > 100 ? 10 : pageSize;
        var skip = (safePageNumber - 1) * safePageSize;
        return (safePageNumber, safePageSize, skip);
    }
}
