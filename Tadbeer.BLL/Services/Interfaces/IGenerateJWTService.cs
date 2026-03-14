using Tadbeer.DAL.Models;

namespace Tadbeer.BLL.Services.Interfaces;

public interface IGenerateJWTService
{
    Task<string> GenerateJwtTokenAsync(ApplicationUser user);
}