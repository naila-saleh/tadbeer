using Tadbeer.DAL.Data;
using Tadbeer.DAL.DTO.Requests;
using Tadbeer.DAL.Models;
using Tadbeer.DAL.Repositories.Interfaces.Specifics;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Tadbeer.DAL.Repositories.Classes.Specifics;

public class ApplicationUserRepository : GenericRepository<ApplicationUser>, IApplicationUserRepository
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ApplicationUserRepository(ApplicationDbContext context, UserManager<ApplicationUser> userManager) : base(context)
    {
        _userManager = userManager;
    }

    public async Task<IEnumerable<ApplicationUser>> GetAllWithPhoneNumbersAsync()
    {
        return await _context.Users
            .Include(u => u.PhoneNumbers)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<ApplicationUser?> GetByIdWithWorkImagesAsync(Guid userId)
    {
        return await _context.Users
            .Include(u => u.PhoneNumbers)
            .Include(u => u.WorkImages)
            .ThenInclude(wi => wi.SubImages)
            .Include(u => u.WorkerSpecialties)
            .ThenInclude(ws => ws.Specialty)
            .Include(u => u.WorkingHours)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<bool> BlockUserAsync(Guid id, int minutes)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return false;
        
        var endDate = DateTimeOffset.UtcNow.AddMinutes(minutes);
        
        // Ensure lockout is actually enabled for this user before setting the date
        if (!await _userManager.GetLockoutEnabledAsync(user))
        {
            await _userManager.SetLockoutEnabledAsync(user, true);
        }

        var result = await _userManager.SetLockoutEndDateAsync(user, endDate);
        return result.Succeeded;
    }

    public async Task<bool> UnBlockUserAsync(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return false;
        
        var result = await _userManager.SetLockoutEndDateAsync(user, null);
        return result.Succeeded;
    }

    public async Task<bool> IsBlockedAsync(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return false;
        
        return await _userManager.IsLockedOutAsync(user);
    }

    public async Task<bool> ChangeUserRoleAsync(Guid userId, UserRole newRole)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return false;

        if (!Enum.IsDefined(typeof(UserRole), newRole))
        {
            return false;
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
        if (!removeResult.Succeeded) return false;

        var addResult = await _userManager.AddToRoleAsync(user, newRole.ToString());
        if (!addResult.Succeeded) return false;

        return true;
    }

    public async Task<IEnumerable<ApplicationUser>> SearchWorkersAsync(string? query, int page, int pageSize)
    {
        var workersQuery = BuildWorkersSearchQuery(query);

        return await workersQuery
            .OrderByDescending(u => u.AvgRating ?? 0m)
            .ThenBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountWorkersAsync(string? query)
    {
        return await BuildWorkersSearchQuery(query).CountAsync();
    }

    public async Task<(IReadOnlyList<ApplicationUser> Workers, int TotalCount)> GetWorkersByFiltersAsync(WorkerFiltersRequestDto request)
    {
        var workersQuery = BuildWorkersSearchQuery(request.Query)
            .Include(u => u.WorkerBookings.Where(b => b.Status == BookingStatus.Accepted));

        // Materialize immediately to avoid IQueryable type chain issues
        var workers = await workersQuery.ToListAsync();
        var now = DateTime.Now;

        // Apply rating filter: calculate average rating from reviews on-the-fly
        if (request.MinRating.HasValue)
        {
            var min = (decimal)request.MinRating.Value;
            workers = workers.Where(u =>
            {
                // Get all reviews for this worker from their completed bookings
                var reviewsForWorker = _context.Reviews
                    .Where(r => r.Booking.WorkerId == u.Id)
                    .ToList();

                if (!reviewsForWorker.Any())
                {
                    return false; // No reviews, exclude
                }

                var avgRating = reviewsForWorker.Average(r => (decimal)r.Rate);
                return avgRating >= min;
            }).ToList();
        }

        if (request.AvailableNow)
        {
            workers = workers.Where(worker => IsWorkerAvailableNow(worker, now)).ToList();
        }

        if (request.AvailableWithin24Hours)
        {
            workers = workers.Where(worker => IsWorkerAvailableWithin24Hours(worker, now)).ToList();
        }

        if (request.Latitude.HasValue && request.Longitude.HasValue)
        {
            var originLat = request.Latitude.Value;
            var originLng = request.Longitude.Value;

            workers = workers
                .Where(w => w.Latitude.HasValue && w.Longitude.HasValue)
                .ToList();

            if (request.MaxDistanceKm.HasValue)
            {
                workers = workers
                    .Where(w => CalculateDistanceKm(originLat, originLng, w.Latitude!.Value, w.Longitude!.Value) <= request.MaxDistanceKm.Value)
                    .ToList();
            }

            if (request.SortByNearest)
            {
                workers = workers
                    .OrderBy(w => CalculateDistanceKm(originLat, originLng, w.Latitude!.Value, w.Longitude!.Value))
                    .ThenBy(u =>
                    {
                        var reviews = _context.Reviews.Where(r => r.Booking.WorkerId == u.Id).ToList();
                        return reviews.Any() ? reviews.Average(r => (decimal)r.Rate) : 0m;
                    })
                    .ThenByDescending(u => u.FirstName)
                    .ThenBy(u => u.LastName)
                    .ToList();
            }
        }

        if (!(request.SortByNearest && request.Latitude.HasValue && request.Longitude.HasValue))
        {
            workers = workers
                .OrderBy(u =>
                {
                    var reviews = _context.Reviews.Where(r => r.Booking.WorkerId == u.Id).ToList();
                    return reviews.Any() ? -(reviews.Average(r => (decimal)r.Rate)) : 0m;
                })
                .ThenBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .ToList();
        }

        var totalCount = workers.Count;
        var pagedWorkers = workers
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return (pagedWorkers, totalCount);
    }

    private IQueryable<ApplicationUser> BuildWorkersSearchQuery(string? query)
    {
        var workerIdsQuery = _context.UserRoles
            .Join(_context.Roles,
                userRole => userRole.RoleId,
                role => role.Id,
                (userRole, role) => new { userRole.UserId, role.Name })
            .Where(x => x.Name == UserRole.Worker.ToString())
            .Select(x => x.UserId);

        var workersQuery = _context.Users
            .Include(u => u.WorkerSpecialties)
            .ThenInclude(ws => ws.Specialty)
            .Include(u => u.WorkingHours)
            .Include(u => u.WorkImages)
            .ThenInclude(wi => wi.SubImages)
            .Where(u => workerIdsQuery.Contains(u.Id))
            .Where(u => u.Status == UserStatus.Existed)
            .AsNoTracking();

        if (string.IsNullOrWhiteSpace(query))
        {
            return workersQuery;
        }

        var pattern = $"%{query.Trim()}%";
        return workersQuery.Where(u =>
            EF.Functions.Like(u.FirstName, pattern) ||
            EF.Functions.Like(u.LastName, pattern) ||
            EF.Functions.Like(u.FirstName + " " + u.LastName, pattern) ||
            u.WorkerSpecialties.Any(ws => EF.Functions.Like(ws.Specialty.Name, pattern)));
    }

    private static bool IsWorkerAvailableNow(ApplicationUser worker, DateTime now)
    {
        var weekDay = ToWeekDay(now.DayOfWeek);
        var nowTime = TimeOnly.FromDateTime(now);

        var hasWorkingSlot = worker.WorkingHours.Any(wh =>
            wh.DayOfWeek == weekDay
            && wh.StartTime <= nowTime
            && wh.EndTime > nowTime);

        if (!hasWorkingSlot)
        {
            return false;
        }

        var occupied = worker.WorkerBookings.Any(b =>
            b.Status == BookingStatus.Accepted
            && b.BookingDate.Date == now.Date
            && b.StartTime <= nowTime
            && b.EndTime > nowTime);

        return !occupied;
    }

    private static bool IsWorkerAvailableWithin24Hours(ApplicationUser worker, DateTime now)
    {
        var windowStart = now;
        var windowEnd = now.AddHours(24);

        for (var date = windowStart.Date; date <= windowEnd.Date; date = date.AddDays(1))
        {
            var day = ToWeekDay(date.DayOfWeek);
            var daySlots = worker.WorkingHours
                .Where(wh => wh.DayOfWeek == day)
                .ToList();

            foreach (var slot in daySlots)
            {
                var slotStart = date + slot.StartTime.ToTimeSpan();
                var slotEnd = date + slot.EndTime.ToTimeSpan();

                var intersectionStart = slotStart > windowStart ? slotStart : windowStart;
                var intersectionEnd = slotEnd < windowEnd ? slotEnd : windowEnd;

                if (intersectionStart >= intersectionEnd)
                {
                    continue;
                }

                var acceptedBookings = worker.WorkerBookings
                    .Where(b => b.Status == BookingStatus.Accepted && b.BookingDate.Date == date)
                    .Select(b => new
                    {
                        Start = date + b.StartTime.ToTimeSpan(),
                        End = date + b.EndTime.ToTimeSpan()
                    })
                    .Where(b => b.Start < intersectionEnd && b.End > intersectionStart)
                    .OrderBy(b => b.Start)
                    .ToList();

                var cursor = intersectionStart;
                foreach (var booking in acceptedBookings)
                {
                    if (booking.Start > cursor)
                    {
                        return true;
                    }

                    if (booking.End > cursor)
                    {
                        cursor = booking.End;
                    }

                    if (cursor >= intersectionEnd)
                    {
                        break;
                    }
                }

                if (cursor < intersectionEnd)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static WeekDay ToWeekDay(DayOfWeek dayOfWeek)
        => Enum.Parse<WeekDay>(dayOfWeek.ToString(), true);

    private static double CalculateDistanceKm(double fromLat, double fromLng, double toLat, double toLng)
    {
        const double earthRadiusKm = 6371.0;
        var dLat = DegreesToRadians(toLat - fromLat);
        var dLng = DegreesToRadians(toLng - fromLng);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                + Math.Cos(DegreesToRadians(fromLat)) * Math.Cos(DegreesToRadians(toLat))
                * Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return earthRadiusKm * c;
    }

    private static double DegreesToRadians(double value)
        => value * (Math.PI / 180);
}
