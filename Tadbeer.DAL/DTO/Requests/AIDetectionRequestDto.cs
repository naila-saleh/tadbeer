using Microsoft.AspNetCore.Http;

namespace Tadbeer.DAL.DTO.Requests;

public class AIDetectionRequestDto
{
    public IFormFile Image { get; set; } = null!;
}
