using Microsoft.AspNetCore.Http;
using Tadbeer.DAL.DTO.Requests;
using Tadbeer.DAL.DTO.Responses;
using Tadbeer.DAL.Models;

namespace Tadbeer.BLL.Services.Interfaces.Specifics;

public interface IAIDetectionService : IGenericService<AIDetectionRequestDto, AIDetectionResponseDto, AIDetection>
{
    Task<AIDetectionResponseDto> PredictAsync(IFormFile image);
}
