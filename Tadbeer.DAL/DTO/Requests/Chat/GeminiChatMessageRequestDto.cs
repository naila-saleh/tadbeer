using System.ComponentModel.DataAnnotations;

namespace Tadbeer.DAL.DTO.Requests.Chat;

public class GeminiChatMessageRequestDto
{
    [Required]
    [RegularExpression("^(user|assistant|model)$", ErrorMessage = "Role must be user, assistant, or model.")]
    public string Role { get; set; } = "user";

    [Required]
    [StringLength(4000, MinimumLength = 1)]
    public string Content { get; set; } = null!;
}

