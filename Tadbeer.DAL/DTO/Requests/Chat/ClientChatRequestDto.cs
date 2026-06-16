using System;
using System.ComponentModel.DataAnnotations;

namespace Tadbeer.DAL.DTO.Requests.Chat;

public class ClientChatRequestDto
{
    public Guid? SessionId { get; init; }

    [Required]
    [StringLength(4000, MinimumLength = 1)]
    public string Message { get; init; } = string.Empty;
}

