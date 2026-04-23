using Microsoft.EntityFrameworkCore;
using Tadbeer.DAL.Data;
using Tadbeer.DAL.Models;
using Tadbeer.DAL.Repositories.Interfaces.Specifics;

namespace Tadbeer.DAL.Repositories.Classes.Specifics;

public class SpecialtyRepository : GenericRepository<Specialty>, ISpecialtyRepository
{
    public SpecialtyRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Specialty>> SearchServicesAsync(string? query, int page, int pageSize)
    {
        var specialtiesQuery = BuildServicesSearchQuery(query);

        return await specialtiesQuery
            .OrderBy(s => s.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountServicesAsync(string? query)
    {
        return await BuildServicesSearchQuery(query).CountAsync();
    }

    private IQueryable<Specialty> BuildServicesSearchQuery(string? query)
    {
        var specialtiesQuery = _context.Specialties.AsNoTracking();

        if (string.IsNullOrWhiteSpace(query))
        {
            return specialtiesQuery;
        }

        var pattern = $"%{query.Trim()}%";
        return specialtiesQuery.Where(s =>
            EF.Functions.Like(s.Name, pattern) ||
            EF.Functions.Like(s.Description, pattern));
    }
}
