using Tadbeer.BLL.Services.Interfaces.Specifics;
using Tadbeer.DAL.DTO.Requests;
using Tadbeer.DAL.DTO.Requests.Profile;
using Tadbeer.DAL.DTO.Requests.WorkImages;
using Tadbeer.DAL.DTO.Responses;
using Tadbeer.DAL.DTO.Responses.Profile;
using Tadbeer.DAL.DTO.Responses.WorkImages;
using Tadbeer.DAL.Models;
using Microsoft.AspNetCore.Identity;
using Mapster;
using Tadbeer.DAL.Repositories.Interfaces;
using Tadbeer.BLL.Exceptions;
using Tadbeer.BLL.Services.Interfaces;

namespace Tadbeer.BLL.Services.Classes.Specifics;

public class ApplicationUserService : GenericService<ApplicationUserRequestDto, ApplicationUserResponseDto, ApplicationUser>, IApplicationUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IFileStorageService _fileStorageService;

    public ApplicationUserService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, IFileStorageService fileStorageService) : base(unitOfWork, unitOfWork.ApplicationUsers)
    {
        _userManager = userManager;
        _fileStorageService = fileStorageService;
    }

    public new async Task<IEnumerable<ApplicationUserResponseDto>> GetAllAsync()
    {
        var users = (await _repository.GetAllAsync()).ToList();
        var dtos = users.Adapt<List<ApplicationUserResponseDto>>();
        foreach (var dto in dtos)
        {
            var user = users.First(u => u.Id == dto.Id);
            var role = await GetPrimaryRoleAsync(user);
            dto.Role = role;

            if (role == UserRole.Worker)
            {
                var workerWithRelations = await _unitOfWork.ApplicationUsers.GetByIdWithWorkImagesAsync(user.Id);
                var specialties = workerWithRelations?.WorkerSpecialties.ToList() ?? new List<WorkerSpecialty>();
                dto.SpecialtyIds = specialties.Select(ws => ws.SpecialtyId).Distinct().ToList();
                dto.SpecialtyNames = specialties
                    .Select(ws => ws.Specialty.Name)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
                dto.WorkingHours = workerWithRelations?.WorkingHours.Adapt<List<WorkingHoursResponseDto>>()
                    ?? new List<WorkingHoursResponseDto>();
            }
        }
        return dtos;
    }

    public new async Task<ApplicationUserResponseDto?> GetByIdAsync(params object[] ids)
    {
        var user = await _repository.GetByIdAsync(ids);
        if (user == null) return null;
        
        var dto = user.Adapt<ApplicationUserResponseDto>();
        var role = await GetPrimaryRoleAsync(user);
        dto.Role = role;

        if (role == UserRole.Worker)
        {
            var workerWithRelations = await _unitOfWork.ApplicationUsers.GetByIdWithWorkImagesAsync(user.Id);
            var specialties = workerWithRelations?.WorkerSpecialties.ToList() ?? new List<WorkerSpecialty>();
            dto.SpecialtyIds = specialties.Select(ws => ws.SpecialtyId).Distinct().ToList();
            dto.SpecialtyNames = specialties
                .Select(ws => ws.Specialty.Name)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            dto.WorkingHours = workerWithRelations?.WorkingHours.Adapt<List<WorkingHoursResponseDto>>()
                ?? new List<WorkingHoursResponseDto>();
        }

        return dto;
    }

    public override async Task RemoveAsync(params object[] ids)
    {
        var user = await _userManager.FindByIdAsync(ids[0].ToString()!);
        if (user != null && await IsInRoleAsync(user, UserRole.SuperAdmin))
        {
            throw new UserOperationException("Cannot delete a SuperAdmin user.");
        }
        await base.RemoveAsync(ids);
    }

    public async Task<bool> BlockUserAsync(Guid id, int minutes)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user != null && await IsInRoleAsync(user, UserRole.SuperAdmin))
        {
            throw new UserOperationException("Cannot block a SuperAdmin user.");
        }
        return await _unitOfWork.ApplicationUsers.BlockUserAsync(id, minutes);
    }

    public async Task<bool> UnBlockUserAsync(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user != null && await IsInRoleAsync(user, UserRole.SuperAdmin))
        {
            throw new UserOperationException("Cannot unblock a SuperAdmin user.");
        }
        return await _unitOfWork.ApplicationUsers.UnBlockUserAsync(id);
    }

    public async Task<bool> IsBlockedAsync(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user != null && await IsInRoleAsync(user, UserRole.SuperAdmin))
        {
            throw new UserOperationException("Cannot check block status for a SuperAdmin user.");
        }
        return await _unitOfWork.ApplicationUsers.IsBlockedAsync(id);
    }

    public async Task<bool> ChangeUserRoleAsync(Guid userId, ChangeRoleRequest request)
    {
        if (request.Role == UserRole.SuperAdmin)
        {
            throw new UserOperationException("Cannot change a user's role to SuperAdmin.");
        }

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user != null && await IsInRoleAsync(user, UserRole.SuperAdmin))
        {
            throw new UserOperationException("Cannot change the role of a SuperAdmin user.");
        }

        return await _unitOfWork.ApplicationUsers.ChangeUserRoleAsync(userId, request.Role);
    }

    public async Task<UserProfileResponseDto?> GetUserProfileAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null || !await IsInRoleAsync(user, UserRole.User))
        {
            return null;
        }

        return user.Adapt<UserProfileResponseDto>();
    }

    public async Task<UserProfileResponseDto?> UpdateUserProfileAsync(Guid userId, UserProfileUpdateRequestDto request)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null || !await IsInRoleAsync(user, UserRole.User))
        {
            return null;
        }

        await ApplyBaseProfileUpdatesAsync(user, request);
        _unitOfWork.ApplicationUsers.Update(user);
        await _unitOfWork.CompleteAsync();

        return user.Adapt<UserProfileResponseDto>();
    }

    public async Task<WorkerProfileResponseDto?> GetWorkerProfileAsync(Guid userId)
    {
        var user = await _unitOfWork.ApplicationUsers.GetByIdWithWorkImagesAsync(userId);
        if (user == null || !await IsInRoleAsync(user, UserRole.Worker))
        {
            return null;
        }

        return user.Adapt<WorkerProfileResponseDto>();
    }

    public async Task<WorkerPublicProfileResponseDto?> GetWorkerPublicProfileAsync(Guid workerId)
    {
        var user = await _unitOfWork.ApplicationUsers.GetByIdWithWorkImagesAsync(workerId);
        if (user == null || !await IsInRoleAsync(user, UserRole.Worker))
        {
            return null;
        }

        return user.Adapt<WorkerPublicProfileResponseDto>();
    }

    public async Task<WorkerProfileResponseDto?> UpdateWorkerProfileAsync(Guid userId, WorkerProfileUpdateRequestDto request)
    {
        var user = await _unitOfWork.ApplicationUsers.GetByIdWithWorkImagesAsync(userId);
        if (user == null || !await IsInRoleAsync(user, UserRole.Worker))
        {
            return null;
        }

        await ApplyBaseProfileUpdatesAsync(user, request);

        if (!string.IsNullOrWhiteSpace(request.JobDescription))
        {
            user.JobDescription = request.JobDescription;
        }

        if (request.ExperienceYears.HasValue)
        {
            user.ExperienceYears = request.ExperienceYears.Value;
        }

        user.UpdatedAt = DateTime.UtcNow;
        
        var hasSpecialtyIds = request.SpecialtyIds != null && request.SpecialtyIds.Count > 0;

        if (hasSpecialtyIds)
        {
            var distinctIds = request.SpecialtyIds!.Distinct().ToList();
            var specialties = await _unitOfWork.Specialties.FindAsync(s => distinctIds.Contains(s.Id));
            var specialtyList = specialties.ToList();

            var foundIds = specialtyList.Select(s => s.Id).ToHashSet();
            var missingIds = distinctIds.Where(id => !foundIds.Contains(id)).ToList();
            if (missingIds.Count > 0)
            {
                throw new UserOperationException($"Specialty IDs not found: {string.Join(", ", missingIds)}");
            }


            var existingSpecialties = user.WorkerSpecialties.ToList();
            if (existingSpecialties.Count > 0)
            {
                _unitOfWork.WorkerSpecialties.RemoveRange(existingSpecialties);
            }

            foreach (var specialty in specialtyList)
            {
                var workerSpecialty = new WorkerSpecialty
                {
                    WorkerId = user.Id,
                    SpecialtyId = specialty.Id
                };
                await _unitOfWork.WorkerSpecialties.AddAsync(workerSpecialty);
            }
        }

        if (request.WorkingHours != null)
        {
            var existingHours = user.WorkingHours.ToList();
            if (existingHours.Count > 0)
            {
                _unitOfWork.WorkingHours.RemoveRange(existingHours);
            }

            foreach (var whDto in request.WorkingHours)
            {
                var workingHours = new WorkingHours
                {
                    Id = Guid.NewGuid(),
                    WorkerId = user.Id,
                    DayOfWeek = whDto.DayOfWeek,
                    StartTime = whDto.StartTime,
                    EndTime = whDto.EndTime
                };
                await _unitOfWork.WorkingHours.AddAsync(workingHours);
            }
        }

        _unitOfWork.ApplicationUsers.Update(user);
        await _unitOfWork.CompleteAsync();

        var refreshedUser = await _unitOfWork.ApplicationUsers.GetByIdWithWorkImagesAsync(userId);
        return refreshedUser?.Adapt<WorkerProfileResponseDto>();
    }

    public async Task<WorkerProfileResponseDto?> DeleteWorkerMainImageAsync(Guid userId, Guid mainImageId)
    {
        var user = await _unitOfWork.ApplicationUsers.GetByIdWithWorkImagesAsync(userId);
        if (user == null || !await IsInRoleAsync(user, UserRole.Worker))
        {
            return null;
        }

        var mainImage = user.WorkImages.FirstOrDefault(i => i.Id == mainImageId);
        if (mainImage == null)
        {
            return null;
        }

        if (mainImage.SubImages.Count > 0)
        {
            foreach (var subImage in mainImage.SubImages)
            {
                await _fileStorageService.DeleteFileAsync(subImage.ImageUrl);
            }
            _unitOfWork.WorkSubImages.RemoveRange(mainImage.SubImages);
        }

        await _fileStorageService.DeleteFileAsync(mainImage.ImageUrl);
        _unitOfWork.WorkImages.Remove(mainImage);
        user.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.ApplicationUsers.Update(user);
        await _unitOfWork.CompleteAsync();

        var refreshedUser = await _unitOfWork.ApplicationUsers.GetByIdWithWorkImagesAsync(userId);
        return refreshedUser?.Adapt<WorkerProfileResponseDto>();
    }

    public async Task<WorkerProfileResponseDto?> DeleteWorkerSubImageAsync(Guid userId, Guid subImageId)
    {
        var user = await _unitOfWork.ApplicationUsers.GetByIdWithWorkImagesAsync(userId);
        if (user == null || !await IsInRoleAsync(user, UserRole.Worker))
        {
            return null;
        }

        var parentMain = user.WorkImages.FirstOrDefault(main => main.SubImages.Any(sub => sub.Id == subImageId));
        if (parentMain == null)
        {
            return null;
        }

        var targetSub = parentMain.SubImages.First(sub => sub.Id == subImageId);
        await _fileStorageService.DeleteFileAsync(targetSub.ImageUrl);
        _unitOfWork.WorkSubImages.Remove(targetSub);
        parentMain.UpdatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.ApplicationUsers.Update(user);
        await _unitOfWork.CompleteAsync();

        var refreshedUser = await _unitOfWork.ApplicationUsers.GetByIdWithWorkImagesAsync(userId);
        return refreshedUser?.Adapt<WorkerProfileResponseDto>();
    }

    public async Task<IEnumerable<WorkerWorkImageResponseDto>?> GetWorkerMainImagesAsync(Guid userId)
    {
        var user = await _unitOfWork.ApplicationUsers.GetByIdWithWorkImagesAsync(userId);
        if (user == null || !await IsInRoleAsync(user, UserRole.Worker))
        {
            return null;
        }

        return user.WorkImages.Adapt<List<WorkerWorkImageResponseDto>>();
    }

    public async Task<IEnumerable<WorkerWorkSubImageResponseDto>?> GetWorkerSubImagesAsync(Guid userId, Guid mainImageId)
    {
        var user = await _unitOfWork.ApplicationUsers.GetByIdWithWorkImagesAsync(userId);
        if (user == null || !await IsInRoleAsync(user, UserRole.Worker))
        {
            return null;
        }

        var mainImage = user.WorkImages.FirstOrDefault(img => img.Id == mainImageId);
        if (mainImage == null)
        {
            return null;
        }

        return mainImage.SubImages.Adapt<List<WorkerWorkSubImageResponseDto>>();
    }

    public async Task<WorkerProfileResponseDto?> AddWorkerSubImagesToMainImageAsync(Guid userId, Guid mainImageId, WorkerMainImageSubImagesRequestDto request)
    {
        var user = await _unitOfWork.ApplicationUsers.GetByIdWithWorkImagesAsync(userId);
        if (user == null || !await IsInRoleAsync(user, UserRole.Worker))
        {
            return null;
        }

        var mainImage = user.WorkImages.FirstOrDefault(img => img.Id == mainImageId);
        if (mainImage == null)
        {
            return null;
        }

        if (request.SubImageFiles == null || request.SubImageFiles.Count == 0)
        {
            throw new UserOperationException("At least one sub-image file is required.");
        }

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        foreach (var file in request.SubImageFiles)
        {
            if (!_fileStorageService.ValidateFile(file, allowedExtensions, 5 * 1024 * 1024))
            {
                throw new UserOperationException("Invalid sub-image file. Allowed: jpg, jpeg, png, gif. Max size: 5MB per file.");
            }

            var imageUrl = await _fileStorageService.SaveFileAsync(file, "work-sub-images", userId);
            var subImage = new WorkSubImage
            {
                Id = Guid.NewGuid(),
                MainImageId = mainImageId,
                ImageUrl = imageUrl
            };
            await _unitOfWork.WorkSubImages.AddAsync(subImage);
        }

        mainImage.UpdatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.ApplicationUsers.Update(user);
        await _unitOfWork.CompleteAsync();

        var refreshedUser = await _unitOfWork.ApplicationUsers.GetByIdWithWorkImagesAsync(userId);
        return refreshedUser?.Adapt<WorkerProfileResponseDto>();
    }

    public async Task<IEnumerable<WorkImageCreatedResponseDto>?> CreateWorkerWorkImagesAsync(Guid userId, CreateWorkImagesRequestDto request)
    {
        var user = await _unitOfWork.ApplicationUsers.GetByIdWithWorkImagesAsync(userId);
        if (user == null || !await IsInRoleAsync(user, UserRole.Worker))
        {
            return null;
        }

        if (request.MainImageFiles == null || request.MainImageFiles.Count == 0)
        {
            throw new UserOperationException("At least one image file is required.");
        }

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        var createdImages = new List<WorkImageCreatedResponseDto>();
        var now = DateTime.UtcNow;

        foreach (var file in request.MainImageFiles)
        {
            if (!_fileStorageService.ValidateFile(file, allowedExtensions, 10 * 1024 * 1024))
            {
                throw new UserOperationException("Invalid image file. Allowed: jpg, jpeg, png, gif. Max size: 10MB.");
            }

            var mainImageId = Guid.NewGuid();
            var mainImageUrl = await _fileStorageService.SaveFileAsync(file, "work-images", user.Id);

            var newMainImage = new WorkImage
            {
                Id = mainImageId,
                WorkerId = user.Id,
                ImageUrl = mainImageUrl,
                CreatedAt = now,
                UpdatedAt = now,
                SubImages = new List<WorkSubImage>()
            };

            await _unitOfWork.WorkImages.AddAsync(newMainImage);

            createdImages.Add(newMainImage.Adapt<WorkImageCreatedResponseDto>());
        }

        user.UpdatedAt = now;
        _unitOfWork.ApplicationUsers.Update(user);
        await _unitOfWork.CompleteAsync();

        return createdImages;
    }

    public async Task<string?> ToggleUserOrWorkerStatusAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return null;
        }

        var isUser = await IsInRoleAsync(user, UserRole.User);
        var isWorker = await IsInRoleAsync(user, UserRole.Worker);
        if (!isUser && !isWorker)
        {
            throw new UserOperationException("Only User and Worker profiles can be temporarily deactivated.");
        }

        user.Status = user.Status == UserStatus.Existed ? UserStatus.Deleted : UserStatus.Existed;
        user.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.ApplicationUsers.Update(user);
        await _unitOfWork.CompleteAsync();

        return user.Status.ToString();
    }

    public async Task<AdminProfileResponseDto?> GetAdminProfileAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null || !await IsInRoleAsync(user, UserRole.Admin))
        {
            return null;
        }

        return user.Adapt<AdminProfileResponseDto>();
    }

    public async Task<AdminProfileResponseDto?> UpdateAdminProfileAsync(Guid userId, AdminProfileUpdateRequestDto request)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null || !await IsInRoleAsync(user, UserRole.Admin))
        {
            return null;
        }

        await ApplyBaseProfileUpdatesAsync(user, request);
        _unitOfWork.ApplicationUsers.Update(user);
        await _unitOfWork.CompleteAsync();

        return user.Adapt<AdminProfileResponseDto>();
    }

    public async Task<SuperAdminProfileResponseDto?> GetSuperAdminProfileAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null || !await IsInRoleAsync(user, UserRole.SuperAdmin))
        {
            return null;
        }

        return user.Adapt<SuperAdminProfileResponseDto>();
    }

    public async Task<SuperAdminProfileResponseDto?> UpdateSuperAdminProfileAsync(Guid userId, SuperAdminProfileUpdateRequestDto request)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null || !await IsInRoleAsync(user, UserRole.SuperAdmin))
        {
            return null;
        }

        await ApplyBaseProfileUpdatesAsync(user, request);
        _unitOfWork.ApplicationUsers.Update(user);
        await _unitOfWork.CompleteAsync();

        return user.Adapt<SuperAdminProfileResponseDto>();
    }

    private async Task ApplyBaseProfileUpdatesAsync(ApplicationUser user, BaseProfileUpdateRequestDto request)
    {
        if (!string.IsNullOrWhiteSpace(request.FirstName))
        {
            user.FirstName = request.FirstName;
        }

        if (!string.IsNullOrWhiteSpace(request.LastName))
        {
            user.LastName = request.LastName;
        }

        if (!string.IsNullOrWhiteSpace(request.City))
        {
            user.City = request.City;
        }

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            user.PhoneNumber = request.PhoneNumber;
        }

        if (request.ProfileImage != null)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            if (!_fileStorageService.ValidateFile(request.ProfileImage, allowedExtensions, 5 * 1024 * 1024))
            {
                throw new UserOperationException("Invalid profile image file. Allowed: jpg, jpeg, png, gif. Max size: 5MB.");
            }

            if (!string.IsNullOrEmpty(user.ProfileImage))
            {
                await _fileStorageService.DeleteFileAsync(user.ProfileImage);
            }

            user.ProfileImage = await _fileStorageService.SaveFileAsync(request.ProfileImage, "profiles", user.Id);
        }

        user.UpdatedAt = DateTime.UtcNow;
    }

    private async Task<UserRole> GetPrimaryRoleAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var roleName = roles.FirstOrDefault();
        return Enum.TryParse<UserRole>(roleName, ignoreCase: true, out var role)
            ? role
            : UserRole.User;
    }

    private Task<bool> IsInRoleAsync(ApplicationUser user, UserRole role)
        => _userManager.IsInRoleAsync(user, role.ToString());

}