using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Tadbeer.DAL.DTO.Requests.Profile;

public class WorkerIdentityImageUploadRequestDto
{
    [Required(ErrorMessage = "Identity image file is required.")]
    [FromForm(Name = "IdentityImage")]
    public IFormFile? IdentityImage { get; set; }
}

