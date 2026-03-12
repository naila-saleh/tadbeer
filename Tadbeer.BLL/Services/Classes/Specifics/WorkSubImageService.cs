using Tadbeer.BLL.Services.Interfaces.Specifics;
using Tadbeer.DAL.DTO.Requests;
using Tadbeer.DAL.DTO.Responses;
using Tadbeer.DAL.Models;
using Tadbeer.DAL.Repositories.Interfaces;

namespace Tadbeer.BLL.Services.Classes.Specifics;

public class WorkSubImageService : GenericService<WorkSubImageRequestDto, WorkSubImageResponseDto, WorkSubImage>, IWorkSubImageService
{
    public WorkSubImageService(IUnitOfWork unitOfWork) : base(unitOfWork, unitOfWork.WorkSubImages)
    {
    }
}
