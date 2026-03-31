using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Tadbeer.DAL.DTO.Requests.Profile;

public class WorkerWorkSubImageRequestDto
{
    [Required]
    public IFormFile ImageFile { get; set; } = null!;
}

