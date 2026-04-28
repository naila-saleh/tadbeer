using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Tadbeer.BLL.Services.Interfaces;

namespace Tadbeer.BLL.Services.Classes;
public class NominatimReverseGeocodingService : IReverseGeocodingService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private const int CacheDurationMinutes = 1440;
    public NominatimReverseGeocodingService(HttpClient httpClient, IMemoryCache cache)
    {
        _httpClient = httpClient;
        _cache = cache;
    }
    public async Task<string?> GetPlaceNameAsync(double latitude, double longitude)
    {
        // bump cache version to invalidate older entries if extraction logic changes
        var cacheKey = $"geo:v3:{latitude:F6}:{longitude:F6}";
        if (_cache.TryGetValue(cacheKey, out string? cachedPlace))
        {
            return cachedPlace;
        }

        // Try multiple zoom levels (more granular first) to increase chance of getting a populated place
        var zoomLevels = new[] { 18, 16, 14, 10 };
        try
        {
            foreach (var z in zoomLevels)
            {
                var url = $"/reverse?format=jsonv2&lat={latitude:F6}&lon={longitude:F6}&accept-language=ar,en&zoom={z}&addressdetails=1";
                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    continue;
                }

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                var placeName = ExtractPlaceName(root);
                if (!string.IsNullOrWhiteSpace(placeName))
                {
                    _cache.Set(cacheKey, placeName, TimeSpan.FromMinutes(CacheDurationMinutes));
                    return placeName;
                }

                // Fallback: try to use the first segment of display_name if it's meaningful
                if (root.TryGetProperty("display_name", out var display) && display.ValueKind == JsonValueKind.String)
                {
                    var displayStr = display.GetString();
                    if (!string.IsNullOrWhiteSpace(displayStr))
                    {
                        var candidate = displayStr.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).FirstOrDefault();
                        if (!string.IsNullOrWhiteSpace(candidate))
                        {
                            _cache.Set(cacheKey, candidate, TimeSpan.FromMinutes(CacheDurationMinutes));
                            return candidate;
                        }
                    }
                }
            }
        }
        catch
        {
            // ignore network/parse errors and return null
        }

        return null;
    }
    private static string? ExtractPlaceName(JsonElement root)
    {
        if (root.TryGetProperty("address", out var address))
        {
            // Priority 1: Populated places (real cities/towns, not admin regions)
            var populatedPlaces = new[]
            {
                "city",
                "town",
                "village",
                "municipality",
                "locality",
                "hamlet",
                "suburb",
                "district",
                "city_district",
                "borough",
                "quarter",
                "neighbourhood",
                "neighborhood"
            };
            foreach (var prop in populatedPlaces)
            {
                if (address.TryGetProperty(prop, out var element) && element.ValueKind == JsonValueKind.String)
                {
                    var value = element.GetString();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        return value;
                    }
                }
            }
            
            // Priority 2: Smaller administrative divisions (more likely to be actual cities/towns)
            // Prefer county/state_district over region/state to avoid very large area names
            var smallerAdminDivisions = new[] { "county", "state_district", "croft" };
            foreach (var prop in smallerAdminDivisions)
            {
                if (address.TryGetProperty(prop, out var element) && element.ValueKind == JsonValueKind.String)
                {
                    var value = element.GetString();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        return value;
                    }
                }
            }
            
            // Priority 3: Larger administrative regions (only if smaller divisions not found)
            var largerAdminDivisions = new[] { "region", "state", "province" };
            foreach (var prop in largerAdminDivisions)
            {
                if (address.TryGetProperty(prop, out var element) && element.ValueKind == JsonValueKind.String)
                {
                    var value = element.GetString();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        return value;
                    }
                }
            }
            
            // Priority 4: Last resort - try any non-empty string property in address
            // This handles cases where Nominatim returns unexpected property names
            var enumerator = address.EnumerateObject();
            foreach (var property in enumerator)
            {
                if (property.Value.ValueKind == JsonValueKind.String)
                {
                    var value = property.Value.GetString();
                    // Skip known non-place properties
                    if (!string.IsNullOrWhiteSpace(value) && 
                        !property.Name.Equals("country", StringComparison.OrdinalIgnoreCase) &&
                        !property.Name.Equals("country_code", StringComparison.OrdinalIgnoreCase) &&
                        !property.Name.StartsWith("ISO", StringComparison.OrdinalIgnoreCase))
                    {
                        return value;
                    }
                }
            }
        }
        return null;
    }
}
