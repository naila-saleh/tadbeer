using Tadbeer.DAL.DTO.Requests;
using Tadbeer.DAL.DTO.Responses;
using Tadbeer.DAL.Models;

namespace Tadbeer.BLL.Services.Interfaces.Specifics;

public interface ISpecialtyService : IGenericService<SpecialtyRequestDto, SpecialtyResponseDto, Specialty>
{
    Task<IEnumerable<SpecialtyResponseDto>> SearchServicesAsync(string? query, int page, int pageSize);
    Task<int> CountServicesAsync(string? query);
}
