using Tadbeer.DAL.Data;
using Tadbeer.DAL.Models;
using Tadbeer.DAL.Repositories.Interfaces.Specifics;

namespace Tadbeer.DAL.Repositories.Classes.Specifics;

public class BookingRepository : GenericRepository<Booking>, IBookingRepository
{
    public BookingRepository(ApplicationDbContext context) : base(context)
    {
    }
}
