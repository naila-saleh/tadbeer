using Mapster;
using Microsoft.EntityFrameworkCore;
using Tadbeer.BLL.Exceptions;
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

    public SpecialtyService(IUnitOfWork unitOfWork) : base(unitOfWork, unitOfWork.Specialties)
    {
    }

    public override async Task<SpecialtyResponseDto> AddAsync(SpecialtyRequestDto dto)
    {
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

        try
        {
            return await base.AddAsync(dto);
        }
        catch (DbUpdateException)
        {
            throw new DuplicateSpecialtyException(DuplicateSpecialtyMessage);
        }
    }

    public override async Task UpdateAsync(SpecialtyRequestDto dto, params object[] ids)
    {
        var target = await _repository.GetByIdAsync(ids);
        if (target is null)
        {
            return;
        }

        var normalizedName = StringNormalization.NormalizeName(dto.Name);

        // Keep DB filtering by id in SQL, then run custom normalization safely in memory.
        var otherSpecialties = await _repository.FindAsync(s => s.Id != target.Id);
        var exists = otherSpecialties.Any(s =>
            StringNormalization.NormalizeName(s.Name) == normalizedName);

        if (exists)
        {
            throw new DuplicateSpecialtyException(DuplicateSpecialtyMessage);
        }

        dto.Name = dto.Name.Trim();
        dto.Adapt(target);
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
}
