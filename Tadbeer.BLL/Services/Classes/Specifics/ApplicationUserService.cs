using Tadbeer.BLL.Services.Interfaces.Specifics;
using Tadbeer.DAL.DTO.Requests;
using Tadbeer.DAL.DTO.Responses;
using Tadbeer.DAL.Models;
using Microsoft.AspNetCore.Identity;
using Mapster;
using Tadbeer.DAL.Repositories.Interfaces;
using Tadbeer.BLL.Exceptions;

namespace Tadbeer.BLL.Services.Classes.Specifics;

public class ApplicationUserService : GenericService<ApplicationUserRequestDto, ApplicationUserResponseDto, ApplicationUser>, IApplicationUserService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ApplicationUserService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager) : base(unitOfWork, unitOfWork.ApplicationUsers)
    {
        _userManager = userManager;
    }

    public new async Task<IEnumerable<ApplicationUserResponseDto>> GetAllAsync()
    {
        var users = await _repository.GetAllAsync();
        var dtos = users.Adapt<List<ApplicationUserResponseDto>>();
        foreach (var dto in dtos)
        {
            var user = users.First(u => u.Id == dto.Id);
            var roles = await _userManager.GetRolesAsync(user);
            dto.Role = roles.FirstOrDefault() ?? "User";
        }
        return dtos;
    }

    public new async Task<ApplicationUserResponseDto?> GetByIdAsync(params object[] ids)
    {
        var user = await _repository.GetByIdAsync(ids);
        if (user == null) return null;
        
        var dto = user.Adapt<ApplicationUserResponseDto>();
        var roles = await _userManager.GetRolesAsync(user);
        dto.Role = roles.FirstOrDefault() ?? "User";
        return dto;
    }

    public override async Task RemoveAsync(params object[] ids)
    {
        var user = await _userManager.FindByIdAsync(ids[0].ToString()!);
        if (user != null && await _userManager.IsInRoleAsync(user, "SuperAdmin"))
        {
            throw new UserOperationException("Cannot delete a SuperAdmin user.");
        }
        await base.RemoveAsync(ids);
    }

    public async Task<bool> BlockUserAsync(Guid id, int minutes)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user != null && await _userManager.IsInRoleAsync(user, "SuperAdmin"))
        {
            throw new UserOperationException("Cannot block a SuperAdmin user.");
        }
        return await _unitOfWork.ApplicationUsers.BlockUserAsync(id, minutes);
    }

    public async Task<bool> UnBlockUserAsync(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user != null && await _userManager.IsInRoleAsync(user, "SuperAdmin"))
        {
            throw new UserOperationException("Cannot unblock a SuperAdmin user.");
        }
        return await _unitOfWork.ApplicationUsers.UnBlockUserAsync(id);
    }

    public async Task<bool> IsBlockedAsync(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user != null && await _userManager.IsInRoleAsync(user, "SuperAdmin"))
        {
            throw new UserOperationException("Cannot check block status for a SuperAdmin user.");
        }
        return await _unitOfWork.ApplicationUsers.IsBlockedAsync(id);
    }

    public async Task<bool> ChangeUserRoleAsync(Guid userId, ChangeRoleRequest request)
    {
        if (string.Equals(request.Role, "SuperAdmin", StringComparison.OrdinalIgnoreCase))
        {
            throw new UserOperationException("Cannot change a user's role to SuperAdmin.");
        }

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user != null && await _userManager.IsInRoleAsync(user, "SuperAdmin"))
        {
            throw new UserOperationException("Cannot change the role of a SuperAdmin user.");
        }

        return await _unitOfWork.ApplicationUsers.ChangeUserRoleAsync(userId, request.Role);
    }
}
