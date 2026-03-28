using Tadbeer.DAL.DTO.Requests;
using Tadbeer.DAL.DTO.Responses;
using Tadbeer.DAL.Models;

namespace Tadbeer.BLL.Services.Interfaces.Specifics;

public interface IApplicationUserService : IGenericService<ApplicationUserRequestDto, ApplicationUserResponseDto, ApplicationUser>
{
    new Task<IEnumerable<ApplicationUserResponseDto>> GetAllAsync();
    new Task<ApplicationUserResponseDto?> GetByIdAsync(params object[] ids);
    Task<bool> BlockUserAsync(Guid id, int days);
    Task<bool> UnBlockUserAsync(Guid id);
    Task<bool> IsBlockedAsync(Guid id);
    Task<bool> ChangeUserRoleAsync(Guid userId, ChangeRoleRequest request);
}
