using System.ComponentModel.DataAnnotations;

namespace Tadbeer.DAL.DTO.Requests.Chat;

public class GeminiChatRequestDto
{
    [Required]
    [StringLength(4000, MinimumLength = 1)]
    public string Message { get; set; } = null!;

    [StringLength(2000)]
    public string? SystemPrompt { get; set; }
    
    [MaxLength(20)]
    public ICollection<GeminiChatMessageRequestDto> History { get; set; } = new List<GeminiChatMessageRequestDto>();
}

