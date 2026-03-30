using System.ComponentModel.DataAnnotations;

namespace Tadbeer.DAL.DTO.Requests;

public class SpecialtyRequestDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = null!;
}

