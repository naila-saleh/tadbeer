using Mapster;
using Microsoft.EntityFrameworkCore;
using Tadbeer.BLL.Exceptions;
using Tadbeer.BLL.Services.Interfaces;
using Tadbeer.BLL.Services.Interfaces.Specifics;
using Tadbeer.BLL.Utilities;
using Tadbeer.DAL.DTO.Requests;
using Tadbeer.DAL.DTO.Responses;
using Tadbeer.DAL.Models;
using Tadbeer.DAL.Repositories.Interfaces;

namespace Tadbeer.BLL.Services.Classes.Specifics;

public class SpecialtyService : GenericService<SpecialtyRequestDto, SpecialtyResponseDto, Specialty>, ISpecialtyService
{
    private const string DuplicateSpecialtyMessage = "Specialty already exists.";
    private const string DefaultIconUrl = "specialty-icons/default.png";
    private static readonly string[] AllowedIconExtensions = [".jpg", ".jpeg", ".png", ".svg", ".webp"];
    private const long MaxIconSizeBytes = 10 * 1024 * 1024; // 10 MB

    private readonly IFileStorageService _fileStorageService;

    public SpecialtyService(IUnitOfWork unitOfWork, IFileStorageService fileStorageService)
        : base(unitOfWork, unitOfWork.Specialties)
    {
        _fileStorageService = fileStorageService;
    }

    public new async Task<IEnumerable<SpecialtyResponseDto>> GetAllAsync()
    {
        var specialties = await _repository.GetAllAsync();
        return specialties.Select(s => s.Adapt<SpecialtyResponseDto>()).ToList();
    }

    public new async Task<SpecialtyResponseDto?> GetByIdAsync(params object[] ids)
    {
        var specialty = await _repository.GetByIdAsync(ids);
        return specialty?.Adapt<SpecialtyResponseDto>();
    }

    public new async Task<IEnumerable<SpecialtyResponseDto>> FindAsync(System.Linq.Expressions.Expression<Func<Specialty, bool>> predicate)
    {
        var specialties = await _repository.FindAsync(predicate);
        return specialties.Select(s => s.Adapt<SpecialtyResponseDto>()).ToList();
    }

    public override async Task<SpecialtyResponseDto> AddAsync(SpecialtyRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new UserOperationException("Specialty name is required.");
        }

        if (string.IsNullOrWhiteSpace(dto.Description))
        {
            throw new UserOperationException("Specialty description is required.");
        }

        if (dto.Icon == null)
        {
            throw new UserOperationException("Specialty icon is required.");
        }

        var normalizedName = StringNormalization.NormalizeName(dto.Name);

        // Normalize in memory because custom normalization logic cannot be translated to SQL.
        var allSpecialties = await _repository.GetAllAsync();
        var exists = allSpecialties.Any(s =>
            StringNormalization.NormalizeName(s.Name) == normalizedName);

        if (exists)
        {
            throw new DuplicateSpecialtyException(DuplicateSpecialtyMessage);
        }

        dto.Name = dto.Name.Trim();

        // Build the entity manually so we can handle icon upload before saving.
        var entity = new Specialty
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description.Trim()
        };

        if (!_fileStorageService.ValidateFile(dto.Icon, AllowedIconExtensions, MaxIconSizeBytes))
        {
            throw new UserOperationException(
                "Invalid icon file. Allowed: jpg, jpeg, png, svg, webp. Max size: 10 MB.");
        }

        entity.Icon = await _fileStorageService.SaveFileAsync(dto.Icon, "specialty-icons", Guid.Empty);

        try
        {
            await _repository.AddAsync(entity);
            await _unitOfWork.CompleteAsync();
        }
        catch (DbUpdateException)
        {
            throw new DuplicateSpecialtyException(DuplicateSpecialtyMessage);
        }

        return entity.Adapt<SpecialtyResponseDto>();
    }

    public override async Task UpdateAsync(SpecialtyRequestDto dto, params object[] ids)
    {
        var target = await _repository.GetByIdAsync(ids);
        if (target is null)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(dto.Name))
        {
            var normalizedName = StringNormalization.NormalizeName(dto.Name);

            // Keep DB filtering by id in SQL, then run custom normalization safely in memory.
            var otherSpecialties = await _repository.FindAsync(s => s.Id != target.Id);
            var exists = otherSpecialties.Any(s =>
                StringNormalization.NormalizeName(s.Name) == normalizedName);

            if (exists)
            {
                throw new DuplicateSpecialtyException(DuplicateSpecialtyMessage);
            }

            target.Name = dto.Name.Trim();
        }

        if (!string.IsNullOrWhiteSpace(dto.Description))
        {
            target.Description = dto.Description.Trim();
        }

        if (dto.Icon != null)
        {
            if (!_fileStorageService.ValidateFile(dto.Icon, AllowedIconExtensions, MaxIconSizeBytes))
            {
                throw new UserOperationException(
                    "Invalid icon file. Allowed: jpg, jpeg, png, svg, webp. Max size: 10 MB.");
            }

            // Delete old icon if present.
            if (CanDeleteIcon(target.Icon))
            {
                await _fileStorageService.DeleteFileAsync(target.Icon);
            }

            target.Icon = await _fileStorageService.SaveFileAsync(dto.Icon, "specialty-icons", Guid.Empty);
        }

        _repository.Update(target);

        try
        {
            await _unitOfWork.CompleteAsync();
        }
        catch (DbUpdateException)
        {
            throw new DuplicateSpecialtyException(DuplicateSpecialtyMessage);
        }
    }

    public override async Task RemoveAsync(params object[] ids)
    {
        var target = await _repository.GetByIdAsync(ids);
        if (target is null)
        {
            return;
        }

        if (CanDeleteIcon(target.Icon))
        {
            await _fileStorageService.DeleteFileAsync(target.Icon);
        }

        _repository.Remove(target);
        await _unitOfWork.CompleteAsync();
    }

    private static bool CanDeleteIcon(string? iconPath)
        => !string.IsNullOrWhiteSpace(iconPath) &&
           !string.Equals(iconPath.Trim(), DefaultIconUrl, StringComparison.OrdinalIgnoreCase);
}
