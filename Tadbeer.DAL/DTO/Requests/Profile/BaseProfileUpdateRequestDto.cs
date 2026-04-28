using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Tadbeer.DAL.DTO.Requests.Profile;

public class BaseProfileUpdateRequestDto
{
    [StringLength(100)]
    public string? FirstName { get; set; }

    [StringLength(100)]
    public string? LastName { get; set; }

    [Range(-90, 90)]
    public double? Latitude { get; set; }

    [Range(-180, 180)]
    public double? Longitude { get; set; }

    [Phone]
    public string? PhoneNumber { get; set; }

    [DataType(DataType.Date)]
    public DateOnly? DateOfBirth { get; set; }

    public IFormFile? ProfileImage { get; set; }
}
