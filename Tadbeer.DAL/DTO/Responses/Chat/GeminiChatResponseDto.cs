using System;

namespace Tadbeer.DAL.DTO.Responses.Chat;

public class GeminiChatResponseDto
{
    public Guid SessionId { get; set; }
    public string Reply { get; set; } = string.Empty;
}

