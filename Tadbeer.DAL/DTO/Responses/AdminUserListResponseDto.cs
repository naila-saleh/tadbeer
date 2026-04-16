using Tadbeer.DAL.Models;

namespace Tadbeer.DAL.DTO.Responses;

public class AdminUserListResponseDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? PrimaryPhoneNumber { get; set; }
    public int PhoneNumbersCount { get; set; }
    public string City { get; set; } = null!;
    public string ProfileImage { get; set; } = null!;
    public UserRole Role { get; set; }
    public string Status { get; set; } = null!;
    public bool EmailConfirmed { get; set; }
}

