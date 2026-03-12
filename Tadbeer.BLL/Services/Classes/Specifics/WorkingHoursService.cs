using Tadbeer.BLL.Services.Interfaces.Specifics;
using Tadbeer.DAL.DTO.Requests;
using Tadbeer.DAL.DTO.Responses;
using Tadbeer.DAL.Models;
using Tadbeer.DAL.Repositories.Interfaces;

namespace Tadbeer.BLL.Services.Classes.Specifics;

public class WorkingHoursService : GenericService<WorkingHoursRequestDto, WorkingHoursResponseDto, WorkingHours>, IWorkingHoursService
{
    public WorkingHoursService(IUnitOfWork unitOfWork) : base(unitOfWork, unitOfWork.WorkingHours)
    {
    }
}
