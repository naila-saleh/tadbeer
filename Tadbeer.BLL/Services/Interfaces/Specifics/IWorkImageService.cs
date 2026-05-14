using Tadbeer.DAL.DTO.Requests.WorkImages;
using Tadbeer.DAL.DTO.Responses;
using Tadbeer.DAL.Models;

namespace Tadbeer.BLL.Services.Interfaces.Specifics;

public interface IWorkImageService : IGenericService<CreateWorkImagesRequestDto, WorkImageResponseDto, WorkImage>
{
}
