using Mapster;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using Tadbeer.BLL.Services.Interfaces.Specifics;
using Tadbeer.DAL.DTO.Requests;
using Tadbeer.DAL.DTO.Responses;
using Tadbeer.DAL.Models;
using Tadbeer.DAL.Repositories.Interfaces;

namespace Tadbeer.BLL.Services.Classes.Specifics;

public class AIDetectionService : GenericService<AIDetectionRequestDto, AIDetectionResponseDto, AIDetection>, IAIDetectionService
{
    private readonly HttpClient _httpClient;

    // Maps the AI model label (case-insensitive) → one or more Arabic specialty names
    private static readonly Dictionary<string, string[]> LabelToSpecialties =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["Surveillance cameras"] = ["كاميرات المراقبة"],
            ["wood"]                 = ["أعمال النجارة"],
            ["Tile"]                 = ["أرضيات وحوائط"],
            ["water leak"]           = ["أعمال السباكة"],
            ["moisture"]             = ["عزل مائي وحراري وتركيب قرميد للأسطح", "أعمال السباكة", "دهانات وتشطيبات وديكور"],
            ["عوازل وقرميد"]         = ["عزل مائي وحراري وتركيب قرميد للأسطح"],
            ["Pipes"]                = ["أعمال السباكة"],
            ["كهرباء"]               = ["أعمال الكهرباء"],
            ["plants"]               = ["أعمال الزراعة وتنسيق الحدائق"],
            ["aluminum"]             = ["أعمال الألمنيوم"],
            ["Water Tank"]           = ["خدمات خزانات الماء"],
            ["satellite dish"]       = ["فني ستالايت"],
            ["Conditioner"]          = ["صيانة التكييف"],
            ["solar energy"]         = ["الطاقة الشمسية"],
            ["crack"]                = ["خدمة حرفي", "دهانات وتشطيبات وديكور"],
            ["حديد مصدي"]            = ["أعمال الحدادة"],
            ["حديد"]                 = ["أعمال الحدادة"],
        };

    public AIDetectionService(IUnitOfWork unitOfWork, IHttpClientFactory httpClientFactory)
        : base(unitOfWork, unitOfWork.AIDetections)
    {
        _httpClient = httpClientFactory.CreateClient("AIModel");
    }

    public async Task<AIDetectionResponseDto> PredictAsync(IFormFile image)
    {
        // 1. Send image to external AI endpoint
        using var content = new MultipartFormDataContent();
        await using var stream = image.OpenReadStream();

        var streamContent = new StreamContent(stream);
        streamContent.Headers.ContentType =
            new System.Net.Http.Headers.MediaTypeHeaderValue(image.ContentType ?? "image/jpeg");

        content.Add(streamContent, "image", image.FileName);

        var response = await _httpClient.PostAsync("predict", content);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(
                $"AI endpoint returned {(int)response.StatusCode}: {errorBody}");
        }

        // 2. Parse the label from the JSON response
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        // Try the most common key names used by Python ML APIs
        string[] candidateKeys = ["prediction", "class", "label", "result", "predicted_class", "predicted_label"];
        var label = candidateKeys
            .Where(k => root.TryGetProperty(k, out _))
            .Select(k => root.GetProperty(k).GetString())
            .FirstOrDefault() ?? json; // fallback: return raw JSON so you can see what the API actually sent

        // 3. Look up which specialty names to search for
        var specialtyNames = LabelToSpecialties.TryGetValue(label, out var names)
            ? names
            : [];

        // 4. Fetch matching specialties from DB (case-insensitive)
        var matched = specialtyNames.Length > 0
            ? await _unitOfWork.Specialties.FindAsync(s =>
                specialtyNames.Contains(s.Name))
            : [];

        return new AIDetectionResponseDto
        {
            PredictedLabel     = label,
            MatchedSpecialties = matched.Select(s => s.Adapt<SpecialtyResponseDto>()).ToList()
        };
    }
}
