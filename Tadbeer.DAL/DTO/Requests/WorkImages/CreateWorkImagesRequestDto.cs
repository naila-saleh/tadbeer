using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Tadbeer.DAL.DTO.Requests.WorkImages;

public class CreateWorkImagesRequestDto
{
    [Required(ErrorMessage = "At least one image file is required.")]
    [FromForm(Name = "ImageFiles")]
    public List<IFormFile> MainImageFiles { get; set; } = new();
}

