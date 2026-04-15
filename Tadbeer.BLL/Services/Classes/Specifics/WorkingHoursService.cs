using Tadbeer.BLL.Services.Interfaces.Specifics;
using Tadbeer.DAL.DTO.Requests;
using Tadbeer.DAL.DTO.Responses;
using Tadbeer.DAL.Models;
using Tadbeer.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Mapster;
using Tadbeer.BLL.Exceptions;

namespace Tadbeer.BLL.Services.Classes.Specifics;

public class WorkingHoursService : GenericService<WorkingHoursRequestDto, WorkingHoursResponseDto, WorkingHours>, IWorkingHoursService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public WorkingHoursService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager) : base(unitOfWork, unitOfWork.WorkingHours)
    {
        _userManager = userManager;
    }

    public async Task<WorkingHoursResponseDto> CreateForWorkerAsync(Guid workerId, WorkingHoursRequestDto dto)
    {
        await ValidateWorkerAsync(workerId);
        ValidateTimeRange(dto);

        var hasOverlap = await _unitOfWork.WorkingHours.HasOverlapAsync(workerId, dto.DayOfWeek, dto.StartTime, dto.EndTime);
        if (hasOverlap)
        {
            throw new UserOperationException("Working hours overlap with an existing schedule.");
        }

        var workingHours = dto.Adapt<WorkingHours>();
        workingHours.Id = Guid.NewGuid();
        workingHours.WorkerId = workerId;

        await _unitOfWork.WorkingHours.AddAsync(workingHours);
        await _unitOfWork.CompleteAsync();

        return workingHours.Adapt<WorkingHoursResponseDto>();
    }

    public async Task<WorkingHoursResponseDto?> UpdateOwnAsync(Guid workerId, Guid workingHoursId, WorkingHoursRequestDto dto)
    {
        var workingHours = await _unitOfWork.WorkingHours.GetByIdAsync(workingHoursId);
        if (workingHours == null || workingHours.WorkerId != workerId)
        {
            return null;
        }

        await ValidateWorkerAsync(workerId);
        ValidateTimeRange(dto);

        var hasOverlap = await _unitOfWork.WorkingHours.HasOverlapAsync(workerId, dto.DayOfWeek, dto.StartTime, dto.EndTime, workingHoursId);
        if (hasOverlap)
        {
            throw new UserOperationException("Working hours overlap with an existing schedule.");
        }

        dto.Adapt(workingHours);
        workingHours.WorkerId = workerId;

        _unitOfWork.WorkingHours.Update(workingHours);
        await _unitOfWork.CompleteAsync();

        return workingHours.Adapt<WorkingHoursResponseDto>();
    }

    public async Task<bool> DeleteOwnAsync(Guid workerId, Guid workingHoursId)
    {
        var workingHours = await _unitOfWork.WorkingHours.GetByIdAsync(workingHoursId);
        if (workingHours == null || workingHours.WorkerId != workerId)
        {
            return false;
        }

        if (await _unitOfWork.WorkingHours.HasBookingsAsync(workingHoursId))
        {
            throw new UserOperationException("Cannot delete working hours that already have bookings.");
        }

        _unitOfWork.WorkingHours.Remove(workingHours);
        await _unitOfWork.CompleteAsync();
        return true;
    }

    public async Task<WorkingHoursPagedResponseDto> GetPagedByWorkerAsync(Guid? workerId, int pageNumber, int pageSize)
    {
        var (safePageNumber, safePageSize, skip) = NormalizePaging(pageNumber, pageSize);

        IEnumerable<WorkingHours> items;
        int totalCount;

        if (workerId.HasValue)
        {
            items = await _unitOfWork.WorkingHours.GetByWorkerIdAsync(workerId.Value, skip, safePageSize);
            totalCount = await _unitOfWork.WorkingHours.CountByWorkerIdAsync(workerId.Value);
        }
        else
        {
            var all = (await _unitOfWork.WorkingHours.GetAllAsync()).ToList();
            totalCount = all.Count;
            items = all
                .OrderBy(wh => wh.WorkerId)
                .ThenBy(wh => wh.DayOfWeek)
                .ThenBy(wh => wh.StartTime)
                .Skip(skip)
                .Take(safePageSize)
                .ToList();
        }

        return new WorkingHoursPagedResponseDto
        {
            Items = items.Adapt<List<WorkingHoursResponseDto>>(),
            TotalCount = totalCount,
            PageNumber = safePageNumber,
            PageSize = safePageSize
        };
    }

    private async Task ValidateWorkerAsync(Guid workerId)
    {
        var worker = await _userManager.FindByIdAsync(workerId.ToString());
        if (worker == null || !await _userManager.IsInRoleAsync(worker, UserRole.Worker.ToString()))
        {
            throw new UserOperationException("Worker not found.");
        }
    }

    private static void ValidateTimeRange(WorkingHoursRequestDto dto)
    {
        if (dto.StartTime >= dto.EndTime)
        {
            throw new UserOperationException("Working hours end time must be after start time.");
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
