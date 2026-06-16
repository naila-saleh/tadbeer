using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Tadbeer.BLL.Services.Interfaces.Specifics;
using Tadbeer.DAL.DTO.Requests.Chat;
using Tadbeer.DAL.DTO.Responses.Chat;

namespace Tadbeer.BLL.Services.Classes.Specifics;

public class GeminiChatService : IGeminiChatService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public GeminiChatService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClient = httpClientFactory.CreateClient("Gemini");
        _configuration = configuration;
    }

    public async Task<GeminiChatResponseDto> ChatAsync(GeminiChatRequestDto request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var apiKey = _configuration["Gemini:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("Gemini API key is not configured. Set Gemini:ApiKey in user-secrets or environment variables.");
        }

        var endpoint = _configuration["Gemini:Endpoint"];
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            endpoint = "https://generativelanguage.googleapis.com/v1beta";
        }

        var model = _configuration["Gemini:Model"];
        if (string.IsNullOrWhiteSpace(model))
        {
            model = "gemini-flash-latest";
        }

        var contents = BuildContents(request);
        if (contents.Count == 0)
        {
            throw new InvalidOperationException("At least one non-empty chat message is required.");
        }

        var payload = new Dictionary<string, object?>
        {
            ["contents"] = contents,
            ["generationConfig"] = new Dictionary<string, object?>
            {
                ["temperature"] = 0.7,
                ["topK"] = 40,
                ["topP"] = 0.95,
                ["maxOutputTokens"] = 1024
            }
        };

        if (!string.IsNullOrWhiteSpace(request.SystemPrompt))
        {
            payload["systemInstruction"] = new
            {
                parts = new[]
                {
                    new { text = request.SystemPrompt.Trim() }
                }
            };
        }

        using var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var requestUri = $"{endpoint.TrimEnd('/')}/models/{Uri.EscapeDataString(model)}:generateContent?key={Uri.EscapeDataString(apiKey)}";
        var response = await _httpClient.PostAsync(requestUri, content, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Gemini API returned {(int)response.StatusCode}: {responseBody}");
        }

        using var document = JsonDocument.Parse(responseBody);
        var root = document.RootElement;

        if (root.TryGetProperty("promptFeedback", out var promptFeedback) &&
            promptFeedback.TryGetProperty("blockReason", out var blockReasonElement) &&
            blockReasonElement.ValueKind is not JsonValueKind.Null and not JsonValueKind.Undefined)
        {
            var blockReason = blockReasonElement.GetString();
            if (!string.IsNullOrWhiteSpace(blockReason))
            {
                throw new InvalidOperationException($"Gemini blocked the prompt: {blockReason}");
            }
        }

        if (!root.TryGetProperty("candidates", out var candidates) || candidates.ValueKind != JsonValueKind.Array || candidates.GetArrayLength() == 0)
        {
            throw new InvalidOperationException("Gemini returned no candidates.");
        }

        var candidate = candidates[0];
        var reply = ExtractText(candidate);
        if (string.IsNullOrWhiteSpace(reply))
        {
            throw new InvalidOperationException("Gemini returned an empty reply.");
        }

        // Return only the minimal response (Reply) to match GeminiChatResponseDto
        return new GeminiChatResponseDto
        {
            Reply = reply.Trim()
        };
    }

    private static List<object> BuildContents(GeminiChatRequestDto request)
    {
        var contents = new List<object>();

        foreach (var message in request.History)
        {
            // GeminiChatMessageRequestDto.Content is required, so Trim() is safe
            var content = message.Content.Trim();
            if (string.IsNullOrWhiteSpace(content))
            {
                continue;
            }

            contents.Add(new
            {
                role = NormalizeRole(message.Role),
                parts = new[]
                {
                    new { text = content }
                }
            });
        }

        // Message is required on the request DTO, so Trim() is safe
        var currentMessage = request.Message.Trim();
        if (!string.IsNullOrWhiteSpace(currentMessage))
        {
            contents.Add(new
            {
                role = "user",
                parts = new[]
                {
                    new { text = currentMessage }
                }
            });
        }

        return contents;
    }

    private static string NormalizeRole(string? role)
    {
        return role?.Trim().ToLowerInvariant() switch
        {
            "user" => "user",
            "assistant" => "model",
            "model" => "model",
            _ => throw new InvalidOperationException("Gemini chat history roles must be user, assistant, or model.")
        };
    }

    private static string ExtractText(JsonElement candidate)
    {
        if (!candidate.TryGetProperty("content", out var content) ||
            !content.TryGetProperty("parts", out var parts) ||
            parts.ValueKind != JsonValueKind.Array)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();
        foreach (var part in parts.EnumerateArray())
        {
            if (part.TryGetProperty("text", out var textElement))
            {
                var text = textElement.GetString();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    builder.Append(text);
                }
            }
        }

        return builder.ToString();
    }
}

