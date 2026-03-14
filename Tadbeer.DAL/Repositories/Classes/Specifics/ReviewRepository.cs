using Tadbeer.DAL.Data;
using Tadbeer.DAL.Models;
using Tadbeer.DAL.Repositories.Interfaces.Specifics;

namespace Tadbeer.DAL.Repositories.Classes.Specifics;

public class ReviewRepository : GenericRepository<Review>, IReviewRepository
{
    public ReviewRepository(ApplicationDbContext context) : base(context)
    {
    }
}
