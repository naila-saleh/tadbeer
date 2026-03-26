using Tadbeer.DAL.Data;
using Tadbeer.DAL.Models;
using Tadbeer.DAL.Repositories.Interfaces.Specifics;

namespace Tadbeer.DAL.Repositories.Classes.Specifics;

public class WorkImageRepository : GenericRepository<WorkImage>, IWorkImageRepository
{
    public WorkImageRepository(ApplicationDbContext context) : base(context)
    {
    }
}
