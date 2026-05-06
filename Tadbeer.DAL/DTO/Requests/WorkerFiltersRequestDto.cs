using System.ComponentModel.DataAnnotations;

namespace Tadbeer.DAL.DTO.Requests;

public class WorkerFiltersRequestDto
{
    public bool AvailableNow { get; set; }
    public bool AvailableWithin24Hours { get; set; }

    [Range(0, 5)]
    public double? MinRating { get; set; }

    public bool SortByNearest { get; set; }

    [Range(-90, 90)]
    public double? Latitude { get; set; }

    [Range(-180, 180)]
    public double? Longitude { get; set; }

    [Range(0, 500)]
    public double? MaxDistanceKm { get; set; }

    [Range(1, 100)]
    public int Page { get; set; } = 1;

    [Range(1, 50)]
    public int PageSize { get; set; } = 10;

    public string? Query { get; set; }
}

