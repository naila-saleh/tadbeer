using System.ComponentModel.DataAnnotations;

namespace Tadbeer.DAL.DTO.Requests;

public class BookingRequestDto
{
	[Required]
	public Guid WorkerId { get; set; }

	[Required]
	public Guid SpecialtyId { get; set; }

	[Required]
	public DateTime BookingDate { get; set; }

	[Required]
	public TimeOnly StartTime { get; set; }

	[Required]
	public TimeOnly EndTime { get; set; }
}

public class BookingUpdateRequestDto
{
	[Required]
	public DateTime BookingDate { get; set; }

	[Required]
	public TimeOnly StartTime { get; set; }

	[Required]
	public TimeOnly EndTime { get; set; }
}
