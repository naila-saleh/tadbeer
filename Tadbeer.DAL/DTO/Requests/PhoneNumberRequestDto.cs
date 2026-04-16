using System.ComponentModel.DataAnnotations;

namespace Tadbeer.DAL.DTO.Requests;

public class PhoneNumberRequestDto
{
	[Required]
	[StringLength(20, MinimumLength = 5)]
	[RegularExpression(@"^\+?[0-9]{5,20}$", ErrorMessage = "Phone number must contain only digits and an optional leading +.")]
	public string Number { get; set; } = string.Empty;
}
