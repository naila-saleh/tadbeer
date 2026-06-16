using Tadbeer.DAL.DTO.Requests.Chat;
using Tadbeer.DAL.DTO.Responses.Chat;

namespace Tadbeer.BLL.Services.Interfaces.Specifics;

public interface IGeminiChatService
{
    Task<GeminiChatResponseDto> ChatAsync(GeminiChatRequestDto request, CancellationToken cancellationToken = default);
}
