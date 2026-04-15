using Tadbeer.DAL.DTO.Requests;
using Tadbeer.DAL.DTO.Responses;
using Tadbeer.DAL.Models;

namespace Tadbeer.BLL.Services.Interfaces.Specifics;

public interface IBookingService : IGenericService<BookingRequestDto, BookingResponseDto, Booking>
{
	Task<BookingResponseDto> CreateForUserAsync(Guid userId, BookingRequestDto dto);
	Task<BookingResponseDto?> UpdateOwnAsync(Guid userId, Guid bookingId, BookingUpdateRequestDto dto);
	Task<bool> DeleteOwnAsync(Guid userId, Guid bookingId);
	Task<BookingResponseDto?> CancelOwnAsync(Guid userId, Guid bookingId);
	Task<BookingResponseDto?> GetOwnByIdAsync(Guid userId, Guid bookingId);
	Task<BookingPagedResponseDto> GetOwnPagedAsync(Guid userId, int pageNumber, int pageSize);

	Task<BookingResponseDto?> AcceptForWorkerAsync(Guid workerId, Guid bookingId);
	Task<BookingResponseDto?> CancelForWorkerAsync(Guid workerId, Guid bookingId);
	Task<BookingResponseDto?> GetForWorkerByIdAsync(Guid workerId, Guid bookingId);
	Task<BookingPagedResponseDto> GetForWorkerPagedAsync(Guid workerId, int pageNumber, int pageSize);

	Task<BookingResponseDto?> GetAnyByIdAsync(Guid bookingId);
	Task<BookingPagedResponseDto> GetAllPagedAsync(int pageNumber, int pageSize);
}
