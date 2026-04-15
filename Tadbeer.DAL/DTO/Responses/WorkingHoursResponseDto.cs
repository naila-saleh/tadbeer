using Tadbeer.DAL.Models;

namespace Tadbeer.DAL.DTO.Responses;

public class WorkingHoursResponseDto
{
    public Guid Id { get; set; }
    public Guid WorkerId { get; set; }
    public WeekDay DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
}

public class WorkingHoursPagedResponseDto
{
    public IReadOnlyCollection<WorkingHoursResponseDto> Items { get; set; } = Array.Empty<WorkingHoursResponseDto>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

