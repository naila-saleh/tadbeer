using Tadbeer.DAL.Models;

namespace Tadbeer.DAL.Repositories.Interfaces.Specifics;

public interface IPhoneNumberRepository : IGenericRepository<PhoneNumber>
{
	Task<PhoneNumber?> GetByIdAsync(Guid id);
	Task<PhoneNumber?> GetByIdForUserAsync(Guid id, Guid userId);
	Task<IEnumerable<PhoneNumber>> GetByUserIdAsync(Guid userId, int skip, int take);
	Task<int> CountByUserIdAsync(Guid userId);
	Task<IEnumerable<PhoneNumber>> GetByUserIdAsync(Guid userId);
}
