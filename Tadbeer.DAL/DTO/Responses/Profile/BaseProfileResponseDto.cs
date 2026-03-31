namespace Tadbeer.DAL.DTO.Responses.Profile;

public class BaseProfileResponseDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public string City { get; set; } = null!;
    public string ProfileImage { get; set; } = null!;
    public string Role { get; set; } = null!;
}

