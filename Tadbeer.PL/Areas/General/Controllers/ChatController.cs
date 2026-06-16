using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Security.Claims;
using System.Text;
using Tadbeer.BLL.Services.Interfaces.Specifics;
using Tadbeer.DAL.DTO.Requests.Chat;
using Tadbeer.DAL.DTO.Responses.Chat;
using Tadbeer.DAL.Models;
using Tadbeer.DAL.Repositories.Interfaces;

namespace Tadbeer.PL.Areas.General.Controllers;

[Route("api/[area]/[controller]")]
[ApiController]
[Area("General")]
[Authorize]
public class ChatController : ControllerBase
{
    private const int MaxHistoryMessages = 20;
    private static readonly ConcurrentDictionary<string, List<GeminiChatMessageRequestDto>> SessionHistory = new();

    private readonly IGeminiChatService _geminiChatService;
    private readonly IConfiguration _configuration;
    private readonly IUnitOfWork _unitOfWork;

    public ChatController(
        IGeminiChatService geminiChatService,
        IConfiguration configuration,
        IUnitOfWork unitOfWork)
    {
        _geminiChatService = geminiChatService;
        _configuration = configuration;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Sends a chat message to Gemini and returns the assistant reply.
    /// Real active workers and specialties from the database are provided to ensure accurate responses.
    /// </summary>
    // POST api/General/chat/gemini
    [HttpPost("gemini")]
    [Consumes("application/json")]
    public async Task<ActionResult<GeminiChatResponseDto>> SendToGemini([FromBody] ClientChatRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown-user";
            var sessionId = request.SessionId ?? Guid.NewGuid();
            var sessionKey = $"{userId}:{sessionId}";

            // Fetch real active specialties from the database
            var specialties = (await _unitOfWork.Specialties.GetAllAsync()).ToList();

            // Fetch active workers using SearchWorkersAsync (which already filters by role and status)
            var activeWorkers = (await _unitOfWork.ApplicationUsers.SearchWorkersAsync(null, 1, 500)).ToList();

            // Load the system prompt from config or use a refined default
            var systemPrompt = _configuration["Gemini:SystemPrompt"];
            if (string.IsNullOrWhiteSpace(systemPrompt))
            {
                systemPrompt = BuildDefaultSystemPrompt();
            }

            var history = SessionHistory.GetOrAdd(sessionKey, _ => new List<GeminiChatMessageRequestDto>());

            List<GeminiChatMessageRequestDto> historySnapshot;
            lock (history)
            {
                historySnapshot = history.TakeLast(MaxHistoryMessages).ToList();
            }

            // Build a context message with real data so the AI doesn't hallucinate.
            // Use the current message plus previous USER turns to keep specialty intent in follow-ups.
            var contextMessage = BuildContextMessage(request.Message, historySnapshot, specialties, activeWorkers);

            // Merge the prompt and real DB context into the system prompt instead of adding a custom history role
            systemPrompt = $"{systemPrompt}\n\n{contextMessage}";

            // Build the Gemini request with server-side context and sanitized history
            var geminiRequest = new GeminiChatRequestDto
            {
                Message = request.Message,
                SystemPrompt = systemPrompt,
                History = historySnapshot
            };

            var result = await _geminiChatService.ChatAsync(geminiRequest, cancellationToken);

            // Persist this turn in server-side session history.
            lock (history)
            {
                history.Add(new GeminiChatMessageRequestDto { Role = "user", Content = request.Message.Trim() });
                history.Add(new GeminiChatMessageRequestDto { Role = "assistant", Content = result.Reply.Trim() });

                if (history.Count > MaxHistoryMessages)
                {
                    history.RemoveRange(0, history.Count - MaxHistoryMessages);
                }
            }

            result.SessionId = sessionId;
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred while processing your request.", details = ex.Message });
        }
    }

    /// <summary>
    /// Builds a context message containing real active specialties and workers from the database.
    /// This ensures the AI only mentions real data and doesn't make up worker names or IDs.
    /// </summary>
    private string BuildContextMessage(string userMessage, List<GeminiChatMessageRequestDto> historySnapshot, List<Specialty> specialties, List<ApplicationUser> activeWorkers)
    {
        var previousUserTurns = historySnapshot
            .Where(m => string.Equals(m.Role, "user", StringComparison.OrdinalIgnoreCase))
            .Select(m => m.Content)
            .ToList();

        var intentText = string.Join(" ", previousUserTurns.Append(userMessage));
        var requestedSpecialties = FindRequestedSpecialties(intentText, specialties);
        var requestedSpecialtyIds = requestedSpecialties.Select(s => s.Id).ToHashSet();

        var allowedWorkers = requestedSpecialtyIds.Count == 0
            ? activeWorkers
            : activeWorkers.Where(w => w.WorkerSpecialties.Any(ws => requestedSpecialtyIds.Contains(ws.SpecialtyId))).ToList();

        var allSpecialtiesText = specialties.Any()
            ? string.Join(", ", specialties.Select(s => s.Name))
            : "None";

        var requestedSpecialtiesText = requestedSpecialties.Any()
            ? string.Join(", ", requestedSpecialties.Select(s => s.Name))
            : "None detected";

        var allowedWorkersText = allowedWorkers.Any()
            ? string.Join("\n", allowedWorkers.Select(w => $"- {w.FirstName} {w.LastName}"))
            : "- No matching active workers found";

        var sb = new StringBuilder();
        sb.AppendLine("REAL DATABASE CONTEXT (STRICT):");
        sb.AppendLine($"User Message: {userMessage}");
        sb.AppendLine($"Conversation Intent Text: {intentText}");
        sb.AppendLine($"Active Specialties: {allSpecialtiesText}");
        sb.AppendLine($"Requested Specialty Match: {requestedSpecialtiesText}");
        sb.AppendLine("ALLOWED_WORKERS_EXACT:");
        sb.AppendLine(allowedWorkersText);
        sb.AppendLine();
        sb.AppendLine("HARD RULES (MUST FOLLOW):");
        sb.AppendLine("1) If the user requested a specialty, list ONLY workers from ALLOWED_WORKERS_EXACT.");
        sb.AppendLine("2) NEVER invent names. If no matching worker exists, say no matching active worker is available.");
        sb.AppendLine("3) Do NOT mention API endpoints, payloads, IDs, or technical backend details.");
        sb.AppendLine("4) Keep the answer concise and clear for normal users.");
        sb.AppendLine("5) Respond in Arabic when user message is Arabic; otherwise respond in user's language.");

        return sb.ToString();
    }

    private static List<Specialty> FindRequestedSpecialties(string message, List<Specialty> specialties)
    {
        if (string.IsNullOrWhiteSpace(message) || specialties.Count == 0)
        {
            return new List<Specialty>();
        }

        var normalizedMessage = message.Trim().ToLowerInvariant();
        var requested = specialties
            .Where(s => normalizedMessage.Contains(s.Name.Trim().ToLowerInvariant()))
            .ToList();

        // Common plumbing hints so a phrase like "kitchen leaking" maps to plumbing workers.
        var plumbingHints = new[] { "plumb", "leak", "pipe", "sink", "drain", "سباك", "سباكة", "تسريب", "مواسير", "مطبخ" };
        var hasPlumbingHint = plumbingHints.Any(h => normalizedMessage.Contains(h));
        if (hasPlumbingHint)
        {
            requested.AddRange(specialties.Where(s =>
            {
                var n = s.Name.ToLowerInvariant();
                return n.Contains("plumb") || n.Contains("سبا") || n.Contains("تسريب") || n.Contains("pipe");
            }));
        }

        return requested
            .GroupBy(s => s.Id)
            .Select(g => g.First())
            .ToList();
    }

    /// <summary>
    /// Builds the default system prompt for the Tadbeer assistant when no configured prompt is found.
    /// </summary>
    private static string BuildDefaultSystemPrompt()
    {
        return @"You are the Tadbeer Assistant, helping users find qualified workers and services on the Tadbeer platform.

Your role:
- Answer questions about available workers and specialties.
- Be helpful, concise, and clear.
- Only use information about real available workers and specialties provided to you.

Important rules:
- NEVER make up worker names, IDs, or specialties.
- NEVER show API endpoints, technical details, or JSON payloads to end users.
- NEVER expose secrets, connection strings, or backend infrastructure details.
- If a specialty or worker is not in the real data provided, say so clearly and suggest alternatives.
- Respond in the user's language (Arabic or English).
- Be user-friendly and focus on helping them book services.";
    }

}