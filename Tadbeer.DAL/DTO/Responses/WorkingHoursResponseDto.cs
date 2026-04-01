using Tadbeer.DAL.Models;

namespace Tadbeer.DAL.DTO.Responses;

public class WorkingHoursResponseDto
{
    public Guid Id { get; set; }
    public WeekDay DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
}
