using System.ComponentModel.DataAnnotations;

namespace Tadbeer.DAL.DTO.Requests;

public class ReviewRequestDto
{
	[Required]
	public Guid BookingId { get; set; }

	[Range(1, 5)]
	public byte Rate { get; set; }

	[StringLength(1000)]
	public string? Comment { get; set; }
}

public class ReviewUpdateRequestDto
{
	[Range(1, 5)]
	public byte Rate { get; set; }

	[StringLength(1000)]
	public string? Comment { get; set; }
}
