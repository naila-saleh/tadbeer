using Tadbeer.DAL.Models;

namespace Tadbeer.DAL.Repositories.Interfaces.Specifics;

public interface IWorkingHoursRepository : IGenericRepository<WorkingHours>
{
	Task<WorkingHours?> GetByIdAsync(Guid id);
	Task<IEnumerable<WorkingHours>> GetByWorkerIdAsync(Guid workerId, int skip, int take);
	Task<int> CountByWorkerIdAsync(Guid workerId);
	Task<bool> HasOverlapAsync(Guid workerId, WeekDay dayOfWeek, TimeOnly startTime, TimeOnly endTime, Guid? ignoreWorkingHourId = null);
	Task<bool> HasBookingsAsync(Guid workingHourId);
}
