using Tadbeer.DAL.DTO.Requests;
using Tadbeer.DAL.DTO.Responses;

namespace Tadbeer.BLL.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto model);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto model);
}
