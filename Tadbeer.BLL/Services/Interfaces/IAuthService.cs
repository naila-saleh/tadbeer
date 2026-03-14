using Microsoft.AspNetCore.Http;
using Tadbeer.DAL.DTO.Requests;
using Tadbeer.DAL.DTO.Responses;

namespace Tadbeer.BLL.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto model, HttpRequest request);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto model);
    Task<AuthResponseDto> ConfirmEmailAsync(string userId, string token);
}
