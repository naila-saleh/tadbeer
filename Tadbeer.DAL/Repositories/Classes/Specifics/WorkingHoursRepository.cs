using Tadbeer.DAL.Data;
using Tadbeer.DAL.Models;
using Tadbeer.DAL.Repositories.Interfaces.Specifics;

namespace Tadbeer.DAL.Repositories.Classes.Specifics;

public class WorkingHoursRepository : GenericRepository<WorkingHours>, IWorkingHoursRepository
{
    public WorkingHoursRepository(ApplicationDbContext context) : base(context)
    {
    }
}
