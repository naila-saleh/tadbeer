using Tadbeer.DAL.Models;

namespace Tadbeer.DAL.DTO.Responses;

public class BookingResponseDto
{
	public Guid Id { get; set; }
	public Guid UserId { get; set; }
	public string UserName { get; set; } = string.Empty;

	public Guid WorkerId { get; set; }
	public string WorkerName { get; set; } = string.Empty;

	public Guid WorkingHourId { get; set; }
	public WeekDay? WorkingDay { get; set; }
	public TimeOnly? WorkingHourStart { get; set; }
	public TimeOnly? WorkingHourEnd { get; set; }

	public DateTime BookingDate { get; set; }
	public TimeOnly StartTime { get; set; }
	public TimeOnly EndTime { get; set; }
	public int DurationMinutes { get; set; }
	public BookingStatus Status { get; set; }

	public DateTime CreatedAt { get; set; }
	public DateTime UpdatedAt { get; set; }
}

public class BookingPagedResponseDto
{
	public IReadOnlyCollection<BookingResponseDto> Items { get; set; } = Array.Empty<BookingResponseDto>();
	public int TotalCount { get; set; }
	public int PageNumber { get; set; }
	public int PageSize { get; set; }
}
