using Tadbeer.BLL.Services.Interfaces.Specifics;
using Tadbeer.DAL.DTO.Requests.WorkImages;
using Tadbeer.DAL.DTO.Responses;
using Tadbeer.DAL.Models;
using Tadbeer.DAL.Repositories.Interfaces;

namespace Tadbeer.BLL.Services.Classes.Specifics;

public class WorkImageService : GenericService<CreateWorkImagesRequestDto, WorkImageResponseDto, WorkImage>, IWorkImageService
{
    public WorkImageService(IUnitOfWork unitOfWork) : base(unitOfWork, unitOfWork.WorkImages)
    {
    }
}
