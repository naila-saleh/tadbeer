using Tadbeer.DAL.Data;
using Tadbeer.DAL.Models;
using Tadbeer.DAL.Repositories.Interfaces.Specifics;

namespace Tadbeer.DAL.Repositories.Classes.Specifics;

public class WorkSubImageRepository : GenericRepository<WorkSubImage>, IWorkSubImageRepository
{
    public WorkSubImageRepository(ApplicationDbContext context) : base(context)
    {
    }
}
