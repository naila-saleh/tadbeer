namespace Tadbeer.DAL.DTO.Responses.Profile;

public class WorkerIdentityVerificationStatusResponseDto
{
    public bool HasIdentityImage { get; set; }
    public bool IsVerified { get; set; }
    public string? IdentityImageUrl { get; set; }
    public string? IdentityImageRejectionReason { get; set; }
}

