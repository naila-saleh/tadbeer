namespace Tadbeer.DAL.DTO.Responses.Profile;

public class AdminProfileResponseDto : BaseProfileResponseDto
{
    public string Status { get; set; } = null!;
    public bool EmailConfirmed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

