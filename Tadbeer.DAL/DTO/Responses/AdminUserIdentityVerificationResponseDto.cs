namespace Tadbeer.DAL.DTO.Responses;

public class AdminUserIdentityVerificationResponseDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? IdentityImageUrl { get; set; }
    public bool IsIdentityVerified { get; set; }
    public string? IdentityImageRejectionReason { get; set; }
}

