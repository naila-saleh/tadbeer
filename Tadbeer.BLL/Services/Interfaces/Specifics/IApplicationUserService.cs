using Tadbeer.DAL.DTO.Requests;
using Tadbeer.DAL.DTO.Requests.Profile;
using Tadbeer.DAL.DTO.Requests.WorkImages;
using Tadbeer.DAL.DTO.Responses;
using Tadbeer.DAL.DTO.Responses.Profile;
using Tadbeer.DAL.DTO.Responses.WorkImages;
using Tadbeer.DAL.Models;

namespace Tadbeer.BLL.Services.Interfaces.Specifics;

public interface IApplicationUserService : IGenericService<ApplicationUserRequestDto, ApplicationUserResponseDto, ApplicationUser>
{
    new Task<IEnumerable<ApplicationUserResponseDto>> GetAllAsync();
    new Task<ApplicationUserResponseDto?> GetByIdAsync(params object[] ids);
    Task<IEnumerable<AdminUserListResponseDto>> GetAdminUsersListAsync();
    Task<AdminUserDetailResponseDto?> GetAdminUserDetailAsync(Guid id);
    Task<bool> BlockUserAsync(Guid id, int days);
    Task<bool> UnBlockUserAsync(Guid id);
    Task<bool> IsBlockedAsync(Guid id);
    Task<bool> ChangeUserRoleAsync(Guid userId, ChangeRoleRequest request);

    Task<UserProfileResponseDto?> GetUserProfileAsync(Guid userId);
    Task<UserProfileResponseDto?> UpdateUserProfileAsync(Guid userId, UserProfileUpdateRequestDto request);

    Task<WorkerProfileResponseDto?> GetWorkerProfileAsync(Guid userId);
    Task<WorkerPublicProfileResponseDto?> GetWorkerPublicProfileAsync(Guid workerId);
    Task<WorkersFilteredResponseDto> GetWorkersByFiltersAsync(WorkerFiltersRequestDto request);
    Task<IEnumerable<WorkerPublicProfileResponseDto>> SearchWorkersAsync(string? query, int page, int pageSize);
    Task<int> CountWorkersAsync(string? query);
    Task<WorkerProfileResponseDto?> UpdateWorkerProfileAsync(Guid userId, WorkerProfileUpdateRequestDto request);
    Task<WorkerProfileResponseDto?> DeleteWorkerMainImageAsync(Guid userId, Guid mainImageId);
    Task<WorkerProfileResponseDto?> DeleteWorkerSubImageAsync(Guid userId, Guid subImageId);
    Task<IEnumerable<WorkerWorkImageResponseDto>?> GetWorkerMainImagesAsync(Guid userId);
    Task<IEnumerable<WorkerWorkSubImageResponseDto>?> GetWorkerSubImagesAsync(Guid userId, Guid mainImageId);
    Task<WorkerProfileResponseDto?> AddWorkerSubImagesToMainImageAsync(Guid userId, Guid mainImageId, WorkerMainImageSubImagesRequestDto request);
    Task<IEnumerable<WorkImageCreatedResponseDto>?> CreateWorkerWorkImagesAsync(Guid userId, CreateWorkImagesRequestDto request);

    Task<string?> ToggleUserOrWorkerStatusAsync(Guid userId);

    Task<AdminProfileResponseDto?> GetAdminProfileAsync(Guid userId);
    Task<AdminProfileResponseDto?> UpdateAdminProfileAsync(Guid userId, AdminProfileUpdateRequestDto request);

    Task<SuperAdminProfileResponseDto?> GetSuperAdminProfileAsync(Guid userId);
    Task<SuperAdminProfileResponseDto?> UpdateSuperAdminProfileAsync(Guid userId, SuperAdminProfileUpdateRequestDto request);
}
