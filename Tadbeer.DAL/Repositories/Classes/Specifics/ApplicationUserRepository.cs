using Tadbeer.DAL.Data;
using Tadbeer.DAL.Models;
using Tadbeer.DAL.Repositories.Interfaces.Specifics;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Tadbeer.DAL.Repositories.Classes.Specifics;

public class ApplicationUserRepository : GenericRepository<ApplicationUser>, IApplicationUserRepository
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ApplicationUserRepository(ApplicationDbContext context, UserManager<ApplicationUser> userManager) : base(context)
    {
        _userManager = userManager;
    }

    public async Task<IEnumerable<ApplicationUser>> GetAllWithPhoneNumbersAsync()
    {
        return await _context.Users
            .Include(u => u.PhoneNumbers)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<ApplicationUser?> GetByIdWithWorkImagesAsync(Guid userId)
    {
        return await _context.Users
            .Include(u => u.PhoneNumbers)
            .Include(u => u.WorkImages)
            .ThenInclude(wi => wi.SubImages)
            .Include(u => u.WorkerSpecialties)
            .ThenInclude(ws => ws.Specialty)
            .Include(u => u.WorkingHours)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<bool> BlockUserAsync(Guid id, int minutes)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return false;
        
        var endDate = DateTimeOffset.UtcNow.AddMinutes(minutes);
        
        // Ensure lockout is actually enabled for this user before setting the date
        if (!await _userManager.GetLockoutEnabledAsync(user))
        {
            await _userManager.SetLockoutEnabledAsync(user, true);
        }

        var result = await _userManager.SetLockoutEndDateAsync(user, endDate);
        return result.Succeeded;
    }

    public async Task<bool> UnBlockUserAsync(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return false;
        
        var result = await _userManager.SetLockoutEndDateAsync(user, null);
        return result.Succeeded;
    }

    public async Task<bool> IsBlockedAsync(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return false;
        
        return await _userManager.IsLockedOutAsync(user);
    }

    public async Task<bool> ChangeUserRoleAsync(Guid userId, UserRole newRole)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return false;

        if (!Enum.IsDefined(typeof(UserRole), newRole))
        {
            return false;
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
        if (!removeResult.Succeeded) return false;

        var addResult = await _userManager.AddToRoleAsync(user, newRole.ToString());
        if (!addResult.Succeeded) return false;

        return true;
    }
}
