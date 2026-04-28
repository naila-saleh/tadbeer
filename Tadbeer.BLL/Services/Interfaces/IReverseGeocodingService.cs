using System.Text.Json.Serialization;
namespace Tadbeer.BLL.Services.Interfaces;
public interface IReverseGeocodingService
{
    Task<string?> GetPlaceNameAsync(double latitude, double longitude);
}
