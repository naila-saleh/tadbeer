using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Tadbeer.DAL.DTO.Requests.Profile;

public class BaseProfileUpdateRequestDto
{
    [StringLength(100)]
    public string? FirstName { get; set; }

    [StringLength(100)]
    public string? LastName { get; set; }

    [StringLength(100)]
    public string? City { get; set; }

    [Phone]
    public string? PhoneNumber { get; set; }

    public IFormFile? ProfileImage { get; set; }
}

