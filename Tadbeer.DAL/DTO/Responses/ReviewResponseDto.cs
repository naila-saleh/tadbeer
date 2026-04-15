namespace Tadbeer.DAL.DTO.Responses;

public class ReviewResponseDto
{
	public Guid Id { get; set; }
	public Guid BookingId { get; set; }

	public Guid UserId { get; set; }
	public string UserName { get; set; } = string.Empty;

	public Guid WorkerId { get; set; }
	public string WorkerName { get; set; } = string.Empty;

	public byte Rate { get; set; }
	public string? Comment { get; set; }

	public DateTime CreatedAt { get; set; }
	public DateTime UpdatedAt { get; set; }
}

public class ReviewPagedResponseDto
{
	public IReadOnlyCollection<ReviewResponseDto> Items { get; set; } = Array.Empty<ReviewResponseDto>();
	public int TotalCount { get; set; }
	public int PageNumber { get; set; }
	public int PageSize { get; set; }
}
