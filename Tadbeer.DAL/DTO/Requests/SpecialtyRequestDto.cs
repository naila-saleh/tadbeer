using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Tadbeer.DAL.DTO.Requests;

public class SpecialtyRequestDto
{
    [StringLength(100, MinimumLength = 2)]
    public string? Name { get; set; }

    [StringLength(1000, MinimumLength = 2)]
    public string? Description { get; set; }

    /// <summary>Icon image file (jpg, jpeg, png, svg). Optional on update.</summary>
    public IFormFile? Icon { get; set; }
}
