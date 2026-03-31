using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Tadbeer.DAL.DTO.Requests.Profile;

public class WorkerMainImageSubImagesRequestDto
{
    [Required]
    [FromForm(Name = "SubImageFiles")]
    public List<IFormFile> SubImageFiles { get; set; } = new();
}

