using Tadbeer.BLL.Services.Interfaces.Specifics;
using Tadbeer.DAL.DTO.Requests;
using Tadbeer.DAL.DTO.Responses;
using Tadbeer.DAL.Models;
using Tadbeer.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Mapster;
using Tadbeer.BLL.Exceptions;

namespace Tadbeer.BLL.Services.Classes.Specifics;

public class PhoneNumberService : GenericService<PhoneNumberRequestDto, PhoneNumberResponseDto, PhoneNumber>, IPhoneNumberService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public PhoneNumberService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager) : base(unitOfWork, unitOfWork.PhoneNumbers)
    {
        _userManager = userManager;
    }

    public async Task<PhoneNumberResponseDto> CreateOwnAsync(Guid userId, PhoneNumberRequestDto dto)
    {
        await EnsureUserExistsAsync(userId);

        dto.Number = dto.Number.Trim();
        await EnsurePhoneNumberUniqueAsync(dto.Number);

        var phoneNumber = dto.Adapt<PhoneNumber>();
        phoneNumber.Id = Guid.NewGuid();
        phoneNumber.UserId = userId;
        phoneNumber.CreatedAt = DateTime.UtcNow;

        await _unitOfWork.PhoneNumbers.AddAsync(phoneNumber);
        await _unitOfWork.CompleteAsync();

        return phoneNumber.Adapt<PhoneNumberResponseDto>();
    }

    public async Task<PhoneNumberResponseDto?> UpdateOwnAsync(Guid userId, Guid phoneNumberId, PhoneNumberRequestDto dto)
    {
        var phoneNumber = await _unitOfWork.PhoneNumbers.GetByIdForUserAsync(phoneNumberId, userId);
        if (phoneNumber == null)
        {
            return null;
        }

        dto.Number = dto.Number.Trim();
        await EnsurePhoneNumberUniqueAsync(dto.Number, phoneNumberId);

        dto.Adapt(phoneNumber);
        phoneNumber.UserId = userId;

        _unitOfWork.PhoneNumbers.Update(phoneNumber);
        await _unitOfWork.CompleteAsync();

        return phoneNumber.Adapt<PhoneNumberResponseDto>();
    }

    public async Task<bool> DeleteOwnAsync(Guid userId, Guid phoneNumberId)
    {
        var phoneNumber = await _unitOfWork.PhoneNumbers.GetByIdForUserAsync(phoneNumberId, userId);
        if (phoneNumber == null)
        {
            return false;
        }

        _unitOfWork.PhoneNumbers.Remove(phoneNumber);
        await _unitOfWork.CompleteAsync();
        return true;
    }

    public async Task<PhoneNumberPagedResponseDto> GetByWorkerPagedAsync(Guid workerId, int pageNumber, int pageSize)
    {
        var worker = await _userManager.FindByIdAsync(workerId.ToString());
        if (worker == null)
        {
            throw new UserOperationException("User not found.");
        }

        if (!await _userManager.IsInRoleAsync(worker, UserRole.Worker.ToString()))
        {
            throw new UserOperationException("Requested user is not a Worker.");
        }

        var (safePageNumber, safePageSize, skip) = NormalizePaging(pageNumber, pageSize);
        var phoneNumbers = await _unitOfWork.PhoneNumbers.GetByUserIdAsync(workerId, skip, safePageSize);
        var total = await _unitOfWork.PhoneNumbers.CountByUserIdAsync(workerId);

        return new PhoneNumberPagedResponseDto
        {
            Items = phoneNumbers.Adapt<List<PhoneNumberResponseDto>>(),
            TotalCount = total,
            PageNumber = safePageNumber,
            PageSize = safePageSize
        };
    }

    public async Task<IEnumerable<PhoneNumberResponseDto>> GetOwnAsync(Guid userId)
    {
        await EnsureUserExistsAsync(userId);
        var phoneNumbers = await _unitOfWork.PhoneNumbers.GetByUserIdAsync(userId);
        return phoneNumbers.Adapt<List<PhoneNumberResponseDto>>();
    }

    private async Task EnsureUserExistsAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            throw new UserOperationException("User not found.");
        }
    }

    private async Task EnsurePhoneNumberUniqueAsync(string number, Guid? ignorePhoneNumberId = null)
    {
        var existing = await _unitOfWork.PhoneNumbers.FindAsync(p => p.Number == number);
        var duplicate = ignorePhoneNumberId.HasValue
            ? existing.Any(p => p.Id != ignorePhoneNumberId.Value)
            : existing.Any();

        if (duplicate)
        {
            throw new UserOperationException("Phone number already exists.");
        }
    }

    private static (int PageNumber, int PageSize, int Skip) NormalizePaging(int pageNumber, int pageSize)
    {
        var safePageNumber = pageNumber < 1 ? 1 : pageNumber;
        var safePageSize = pageSize is < 1 or > 100 ? 10 : pageSize;
        var skip = (safePageNumber - 1) * safePageSize;
        return (safePageNumber, safePageSize, skip);
    }
}
