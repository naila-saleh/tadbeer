namespace Tadbeer.DAL.DTO.Responses;

public class PhoneNumberResponseDto
{
	public Guid Id { get; set; }
	public string Number { get; set; } = string.Empty;
	public Guid UserId { get; set; }
}

public class PhoneNumberPagedResponseDto
{
	public IReadOnlyCollection<PhoneNumberResponseDto> Items { get; set; } = Array.Empty<PhoneNumberResponseDto>();
	public int TotalCount { get; set; }
	public int PageNumber { get; set; }
	public int PageSize { get; set; }
}
