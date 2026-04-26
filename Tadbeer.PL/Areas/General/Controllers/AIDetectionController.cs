using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tadbeer.BLL.Services.Interfaces.Specifics;
using Tadbeer.DAL.DTO.Requests;

namespace Tadbeer.PL.Areas.General.Controllers;

[Route("api/[area]/[controller]")]
[ApiController]
[Area("General")]
[Authorize]
public class AIDetectionController : ControllerBase
{
    private readonly IAIDetectionService _aiDetectionService;

    public AIDetectionController(IAIDetectionService aiDetectionService)
    {
        _aiDetectionService = aiDetectionService;
    }

    /// <summary>
    /// Sends an image to the AI model, returns the predicted label and
    /// the matching specialties from the database.
    /// </summary>
    // POST api/General/aidetection/predict
    [HttpPost("predict")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Predict([FromForm] AIDetectionRequestDto request)
    {
        if (request.Image is null || request.Image.Length == 0)
            return BadRequest("يرجى إرسال صورة صالحة.");

        var result = await _aiDetectionService.PredictAsync(request.Image);
        return Ok(result);
    }
}
