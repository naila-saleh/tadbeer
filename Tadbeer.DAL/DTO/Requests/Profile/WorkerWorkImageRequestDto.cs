using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Tadbeer.DAL.DTO.Requests.Profile;

public class WorkerWorkImageRequestDto
{
    [Required]
    public IFormFile ImageFile { get; set; } = null!;

    public ICollection<WorkerWorkSubImageRequestDto> SubImages { get; set; } = new List<WorkerWorkSubImageRequestDto>();
}

