using Tadbeer.DAL.Models;

namespace Tadbeer.DAL.Repositories.Interfaces.Specifics;

public interface ISpecialtyRepository : IGenericRepository<Specialty>
{
    Task<IEnumerable<Specialty>> SearchServicesAsync(string? query, int page, int pageSize);
    Task<int> CountServicesAsync(string? query);
}
