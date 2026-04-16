using Tadbeer.DAL.Models;

namespace Tadbeer.DAL.Repositories.Interfaces.Specifics;

public interface IApplicationUserRepository : IGenericRepository<ApplicationUser>
{
    Task<IEnumerable<ApplicationUser>> GetAllWithPhoneNumbersAsync();
    Task<ApplicationUser?> GetByIdWithWorkImagesAsync(Guid userId);
    Task<bool> BlockUserAsync(Guid id, int days);
    Task<bool> UnBlockUserAsync(Guid id);
    Task<bool> IsBlockedAsync(Guid id);
    Task<bool> ChangeUserRoleAsync(Guid userId, UserRole newRole);
}
