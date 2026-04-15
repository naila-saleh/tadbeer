using Tadbeer.DAL.DTO.Requests;
using Tadbeer.DAL.DTO.Responses;
using Tadbeer.DAL.Models;

namespace Tadbeer.BLL.Services.Interfaces.Specifics;

public interface IWorkingHoursService : IGenericService<WorkingHoursRequestDto, WorkingHoursResponseDto, WorkingHours>
{
	Task<WorkingHoursResponseDto> CreateForWorkerAsync(Guid workerId, WorkingHoursRequestDto dto);
	Task<WorkingHoursResponseDto?> UpdateOwnAsync(Guid workerId, Guid workingHoursId, WorkingHoursRequestDto dto);
	Task<bool> DeleteOwnAsync(Guid workerId, Guid workingHoursId);
	Task<WorkingHoursPagedResponseDto> GetPagedByWorkerAsync(Guid? workerId, int pageNumber, int pageSize);
}
