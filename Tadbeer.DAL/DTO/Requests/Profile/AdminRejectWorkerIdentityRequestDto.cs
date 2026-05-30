using System.ComponentModel.DataAnnotations;

namespace Tadbeer.DAL.DTO.Requests.Profile;

public class AdminRejectWorkerIdentityRequestDto
{
    [Required]
    [StringLength(1000)]
    public string Reason { get; set; } = null!;
}

