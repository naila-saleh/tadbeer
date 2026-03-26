using Tadbeer.DAL.Data;
using Tadbeer.DAL.Models;
using Tadbeer.DAL.Repositories.Interfaces.Specifics;

namespace Tadbeer.DAL.Repositories.Classes.Specifics;

public class ApplicationUserRepository : GenericRepository<ApplicationUser>, IApplicationUserRepository
{
    public ApplicationUserRepository(ApplicationDbContext context) : base(context)
    {
    }
}
