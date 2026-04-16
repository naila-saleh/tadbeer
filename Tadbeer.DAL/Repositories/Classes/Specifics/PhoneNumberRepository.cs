using Tadbeer.DAL.Data;
using Tadbeer.DAL.Models;
using Tadbeer.DAL.Repositories.Interfaces.Specifics;
using Microsoft.EntityFrameworkCore;

namespace Tadbeer.DAL.Repositories.Classes.Specifics;

public class PhoneNumberRepository : GenericRepository<PhoneNumber>, IPhoneNumberRepository
{
    public PhoneNumberRepository(ApplicationDbContext context) : base(context)
    {
    }

    public Task<PhoneNumber?> GetByIdAsync(Guid id)
        => _context.PhoneNumbers.FirstOrDefaultAsync(p => p.Id == id);

    public Task<PhoneNumber?> GetByIdForUserAsync(Guid id, Guid userId)
        => _context.PhoneNumbers.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

    public async Task<IEnumerable<PhoneNumber>> GetByUserIdAsync(Guid userId, int skip, int take)
    {
        return await _context.PhoneNumbers
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .OrderBy(p => p.CreatedAt)
            .ThenBy(p => p.Number)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public Task<int> CountByUserIdAsync(Guid userId)
        => _context.PhoneNumbers.CountAsync(p => p.UserId == userId);

    public async Task<IEnumerable<PhoneNumber>> GetByUserIdAsync(Guid userId)
    {
        return await _context.PhoneNumbers
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .OrderBy(p => p.CreatedAt)
            .ThenBy(p => p.Number)
            .ToListAsync();
    }
}
