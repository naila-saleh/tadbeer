using System.ComponentModel.DataAnnotations;
using Tadbeer.DAL.Models;

namespace Tadbeer.DAL.DTO.Requests;

public class WorkingHoursRequestDto
{
    [Required]
    public WeekDay DayOfWeek { get; set; }

    [Required]
    public TimeOnly StartTime { get; set; }

    [Required]
    public TimeOnly EndTime { get; set; }
}
