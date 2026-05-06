using Microsoft.AspNetCore.Http;

namespace Tadbeer.DAL.DTO.Requests;

public class AIDetectionRequestDto
{
    public IFormFile Image { get; set; } = null!;

    [System.ComponentModel.DataAnnotations.Range(-90, 90)]
    public double? Latitude { get; set; }

    [System.ComponentModel.DataAnnotations.Range(-180, 180)]
    public double? Longitude { get; set; }

    [System.ComponentModel.DataAnnotations.Range(0, 500)]
    public double? MaxDistanceKm { get; set; }
}
