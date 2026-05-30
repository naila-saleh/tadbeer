using Tadbeer.DAL.Models;
using Tadbeer.DAL.DTO.Requests;

namespace Tadbeer.DAL.Repositories.Interfaces.Specifics;

public interface IApplicationUserRepository : IGenericRepository<ApplicationUser>
{
    Task<IEnumerable<ApplicationUser>> GetAllWithPhoneNumbersAsync();
    Task<ApplicationUser?> GetByIdWithWorkImagesAsync(Guid userId);
    Task<bool> BlockUserAsync(Guid id, int days);
    Task<bool> UnBlockUserAsync(Guid id);
    Task<bool> IsBlockedAsync(Guid id);
    Task<IEnumerable<ApplicationUser>> SearchWorkersAsync(string? query, int page, int pageSize);
    Task<int> CountWorkersAsync(string? query);
    Task<IEnumerable<ApplicationUser>> GetPendingIdentityVerificationWorkersAsync();
    Task<(IReadOnlyList<ApplicationUser> Workers, int TotalCount)> GetWorkersByFiltersAsync(WorkerFiltersRequestDto request);
    Task<bool> ChangeUserRoleAsync(Guid userId, UserRole newRole);
}
