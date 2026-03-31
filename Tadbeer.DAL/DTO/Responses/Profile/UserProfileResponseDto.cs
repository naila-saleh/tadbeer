namespace Tadbeer.DAL.DTO.Responses.Profile;

public class UserProfileResponseDto : BaseProfileResponseDto
{
    public string Status { get; set; } = null!;
    public bool EmailConfirmed { get; set; }
}

