using Tadbeer.BLL.Services.Interfaces.Specifics;
using Tadbeer.DAL.DTO.Requests;
using Tadbeer.DAL.DTO.Responses;
using Tadbeer.DAL.Models;
using Tadbeer.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Mapster;
using Tadbeer.BLL.Exceptions;

namespace Tadbeer.BLL.Services.Classes.Specifics;

public class BookingService : GenericService<BookingRequestDto, BookingResponseDto, Booking>, IBookingService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public BookingService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager) : base(unitOfWork, unitOfWork.Bookings)
    {
        _userManager = userManager;
    }

    public async Task<BookingResponseDto> CreateForUserAsync(Guid userId, BookingRequestDto dto)
    {
        var validation = await ValidateCreateOrUpdateAsync(dto.WorkerId, dto.BookingDate, dto.StartTime, dto.EndTime);

        var hasConflict = await _unitOfWork.Bookings.HasActiveBookingConflictAsync(dto.WorkerId, validation.BookingDate, dto.StartTime, dto.EndTime);
        if (hasConflict)
        {
            throw new UserOperationException("Selected booking time range is already reserved.");
        }

        var now = DateTime.UtcNow;
        var booking = dto.Adapt<Booking>();
        booking.Id = Guid.NewGuid();
        booking.UserId = userId;
        booking.WorkingHourId = validation.WorkingHour.Id;
        booking.BookingDate = validation.BookingDate;
        booking.StartTime = dto.StartTime;
        booking.EndTime = dto.EndTime;
        booking.Status = BookingStatus.Pending;
        booking.CreatedAt = now;
        booking.UpdatedAt = now;

        await _unitOfWork.Bookings.AddAsync(booking);
        await _unitOfWork.CompleteAsync();

        var created = await _unitOfWork.Bookings.GetByIdWithDetailsAsync(booking.Id);
        return (created ?? booking).Adapt<BookingResponseDto>();
    }

    public async Task<BookingResponseDto?> UpdateOwnAsync(Guid userId, Guid bookingId, BookingUpdateRequestDto dto)
    {
        var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId);
        if (booking == null || booking.UserId != userId)
        {
            return null;
        }

        if (booking.Status != BookingStatus.Pending)
        {
            throw new UserOperationException("Only pending bookings can be updated.");
        }

        var validation = await ValidateCreateOrUpdateAsync(booking.WorkerId, dto.BookingDate, dto.StartTime, dto.EndTime);

        var hasConflict = await _unitOfWork.Bookings.HasActiveBookingConflictAsync(booking.WorkerId, validation.BookingDate, dto.StartTime, dto.EndTime, bookingId);
        if (hasConflict)
        {
            throw new UserOperationException("Selected booking time range is already reserved.");
        }

        dto.Adapt(booking);
        booking.WorkingHourId = validation.WorkingHour.Id;
        booking.BookingDate = validation.BookingDate;
        booking.StartTime = dto.StartTime;
        booking.EndTime = dto.EndTime;
        booking.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Bookings.Update(booking);
        await _unitOfWork.CompleteAsync();

        var updated = await _unitOfWork.Bookings.GetByIdWithDetailsAsync(bookingId);
        return updated?.Adapt<BookingResponseDto>();
    }

    public async Task<bool> DeleteOwnAsync(Guid userId, Guid bookingId)
    {
        var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId);
        if (booking == null || booking.UserId != userId)
        {
            return false;
        }

        if (booking.Status != BookingStatus.Pending)
        {
            throw new UserOperationException("Only pending bookings can be deleted.");
        }

        _unitOfWork.Bookings.Remove(booking);
        await _unitOfWork.CompleteAsync();
        return true;
    }

    public async Task<BookingResponseDto?> CancelOwnAsync(Guid userId, Guid bookingId)
    {
        var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId);
        if (booking == null || booking.UserId != userId)
        {
            return null;
        }

        if (booking.Status != BookingStatus.Accepted)
        {
            throw new UserOperationException("Only accepted bookings can be cancelled by user.");
        }

        booking.Status = BookingStatus.Cancelled;
        booking.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Bookings.Update(booking);
        await _unitOfWork.CompleteAsync();

        var updated = await _unitOfWork.Bookings.GetByIdWithDetailsAsync(bookingId);
        return updated?.Adapt<BookingResponseDto>();
    }

    public async Task<BookingResponseDto?> GetOwnByIdAsync(Guid userId, Guid bookingId)
    {
        var booking = await _unitOfWork.Bookings.GetByIdForUserAsync(bookingId, userId);
        return booking?.Adapt<BookingResponseDto>();
    }

    public async Task<BookingPagedResponseDto> GetOwnPagedAsync(Guid userId, int pageNumber, int pageSize)
    {
        var (safePageNumber, safePageSize, skip) = NormalizePaging(pageNumber, pageSize);
        var bookings = await _unitOfWork.Bookings.GetByUserIdAsync(userId, skip, safePageSize);
        var total = await _unitOfWork.Bookings.CountByUserIdAsync(userId);

        return new BookingPagedResponseDto
        {
            Items = bookings.Adapt<List<BookingResponseDto>>(),
            TotalCount = total,
            PageNumber = safePageNumber,
            PageSize = safePageSize
        };
    }

    public async Task<BookingResponseDto?> AcceptForWorkerAsync(Guid workerId, Guid bookingId)
    {
        var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId);
        if (booking == null || booking.WorkerId != workerId)
        {
            return null;
        }

        if (booking.Status != BookingStatus.Pending)
        {
            throw new UserOperationException("Only pending bookings can be accepted.");
        }

        var hasAcceptedConflict = await _unitOfWork.Bookings.HasActiveBookingConflictAsync(
            booking.WorkerId,
            booking.BookingDate,
            booking.StartTime,
            booking.EndTime,
            booking.Id);

        if (hasAcceptedConflict)
        {
            throw new UserOperationException("Worker already has an accepted booking in this time range.");
        }

        booking.Status = BookingStatus.Accepted;
        booking.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Bookings.Update(booking);
        await _unitOfWork.CompleteAsync();

        var updated = await _unitOfWork.Bookings.GetByIdWithDetailsAsync(bookingId);
        return updated?.Adapt<BookingResponseDto>();
    }

    public async Task<BookingResponseDto?> CancelForWorkerAsync(Guid workerId, Guid bookingId)
    {
        var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId);
        if (booking == null || booking.WorkerId != workerId)
        {
            return null;
        }

        if (booking.Status is BookingStatus.Rejected or BookingStatus.Completed or BookingStatus.Cancelled)
        {
            throw new UserOperationException("Booking cannot be cancelled in its current status.");
        }

        booking.Status = BookingStatus.Cancelled;
        booking.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Bookings.Update(booking);
        await _unitOfWork.CompleteAsync();

        var updated = await _unitOfWork.Bookings.GetByIdWithDetailsAsync(bookingId);
        return updated?.Adapt<BookingResponseDto>();
    }

    public async Task<BookingResponseDto?> CompleteForWorkerAsync(Guid workerId, Guid bookingId)
    {
        var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId);
        if (booking == null || booking.WorkerId != workerId)
        {
            return null;
        }

        if (booking.Status != BookingStatus.Accepted)
        {
            throw new UserOperationException("Only accepted bookings can be completed.");
        }

        booking.Status = BookingStatus.Completed;
        booking.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Bookings.Update(booking);
        await _unitOfWork.CompleteAsync();

        var updated = await _unitOfWork.Bookings.GetByIdWithDetailsAsync(bookingId);
        return updated?.Adapt<BookingResponseDto>();
    }

    public async Task<BookingResponseDto?> GetForWorkerByIdAsync(Guid workerId, Guid bookingId)
    {
        var booking = await _unitOfWork.Bookings.GetByIdForWorkerAsync(bookingId, workerId);
        return booking?.Adapt<BookingResponseDto>();
    }

    public async Task<BookingPagedResponseDto> GetForWorkerPagedAsync(Guid workerId, int pageNumber, int pageSize)
    {
        var (safePageNumber, safePageSize, skip) = NormalizePaging(pageNumber, pageSize);
        var bookings = await _unitOfWork.Bookings.GetByWorkerIdAsync(workerId, skip, safePageSize);
        var total = await _unitOfWork.Bookings.CountByWorkerIdAsync(workerId);

        return new BookingPagedResponseDto
        {
            Items = bookings.Adapt<List<BookingResponseDto>>(),
            TotalCount = total,
            PageNumber = safePageNumber,
            PageSize = safePageSize
        };
    }

    public async Task<BookingResponseDto?> GetAnyByIdAsync(Guid bookingId)
    {
        var booking = await _unitOfWork.Bookings.GetByIdWithDetailsAsync(bookingId);
        return booking?.Adapt<BookingResponseDto>();
    }

    public async Task<BookingPagedResponseDto> GetAllPagedAsync(int pageNumber, int pageSize)
    {
        var (safePageNumber, safePageSize, skip) = NormalizePaging(pageNumber, pageSize);
        var bookings = await _unitOfWork.Bookings.GetPagedWithDetailsAsync(skip, safePageSize);
        var total = await _unitOfWork.Bookings.CountAsync();

        return new BookingPagedResponseDto
        {
            Items = bookings.Adapt<List<BookingResponseDto>>(),
            TotalCount = total,
            PageNumber = safePageNumber,
            PageSize = safePageSize
        };
    }

    private async Task<(WorkingHours WorkingHour, DateTime BookingDate)> ValidateCreateOrUpdateAsync(Guid workerId, DateTime bookingDate, TimeOnly startTime, TimeOnly endTime)
    {
        if (startTime >= endTime)
        {
            throw new UserOperationException("Booking end time must be after start time.");
        }

        var normalizedDate = bookingDate.Date;
        var bookingStart = normalizedDate + startTime.ToTimeSpan();

        if (bookingStart < DateTime.UtcNow)
        {
            throw new UserOperationException("Booking start time cannot be in the past.");
        }

        var worker = await _userManager.FindByIdAsync(workerId.ToString());
        if (worker == null || !await _userManager.IsInRoleAsync(worker, UserRole.Worker.ToString()))
        {
            throw new UserOperationException("Worker not found.");
        }

        var targetWeekDay = Enum.Parse<WeekDay>(normalizedDate.DayOfWeek.ToString(), true);
        var availableHours = (await _unitOfWork.WorkingHours.FindAsync(wh => wh.WorkerId == workerId && wh.DayOfWeek == targetWeekDay)).ToList();
        if (availableHours.Count == 0)
        {
            throw new UserOperationException("Worker is not available on the selected day.");
        }

        var matchingWorkingHour = availableHours.FirstOrDefault(wh => wh.StartTime <= startTime && wh.EndTime >= endTime);
        if (matchingWorkingHour == null)
        {
            throw new UserOperationException("Selected time range is outside the worker availability.");
        }

        return (matchingWorkingHour, normalizedDate);
    }

    private static (int PageNumber, int PageSize, int Skip) NormalizePaging(int pageNumber, int pageSize)
    {
        var safePageNumber = pageNumber < 1 ? 1 : pageNumber;
        var safePageSize = pageSize is < 1 or > 100 ? 10 : pageSize;
        var skip = (safePageNumber - 1) * safePageSize;
        return (safePageNumber, safePageSize, skip);
    }
}
