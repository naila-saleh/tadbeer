using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Tadbeer.DAL.DTO.Requests.WorkImages;

public class CreateWorkImagesRequestDto
{
    [Required(ErrorMessage = "Image file is required.")]
    [FromForm(Name = "ImageFile")]
    public IFormFile? ImageFile { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    public string? Description { get; set; }
}

