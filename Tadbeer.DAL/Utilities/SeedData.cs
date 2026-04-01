using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using Tadbeer.DAL.Data;
using Tadbeer.DAL.Models;

namespace Tadbeer.DAL.Utilities;

public class SeedData: ISeedData
{
    private readonly ApplicationDbContext _context;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public SeedData(ApplicationDbContext context, RoleManager<IdentityRole<Guid>> roleManager, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task DataSeedingAsync()
    {
        var specialtiesSeed = new[]
        {
            new { Name = "خدمات التنظيف", Description = "خدمات تنظيف المنازل والمكاتب والمنشآت", Icon = "specialty-icons/default.png" },
            new { Name = "الأجهزة المنزلية", Description = "صيانة وإصلاح الأجهزة المنزلية", Icon = "specialty-icons/default.png" },
            new { Name = "أعمال الكهرباء", Description = "تركيب وصيانة الأعطال الكهربائية", Icon = "specialty-icons/default.png" },
            new { Name = "دهانات وتشطيبات وديكور", Description = "أعمال دهان وتشطيب وديكور داخلي", Icon = "specialty-icons/default.png" },
            new { Name = "أعمال السباكة", Description = "إصلاح وتركيب تمديدات السباكة", Icon = "specialty-icons/default.png" },
            new { Name = "صيانة التكييف", Description = "صيانة وتنظيف وتركيب أجهزة التكييف", Icon = "specialty-icons/default.png" },
            new { Name = "أعمال الزراعة", Description = "خدمات الزراعة وتنسيق الحدائق", Icon = "specialty-icons/default.png" },
            new { Name = "فني ستالايت", Description = "تركيب وضبط وصيانة أجهزة الستالايت", Icon = "specialty-icons/default.png" },
            new { Name = "أعمال الألمنيوم", Description = "تصنيع وتركيب وصيانة الألمنيوم", Icon = "specialty-icons/default.png" },
            new { Name = "أعمال النجارة", Description = "تصنيع وصيانة الأعمال الخشبية", Icon = "specialty-icons/default.png" },
            new { Name = "حرفي", Description = "خدمات حرفية متنوعة", Icon = "specialty-icons/default.png" },
            new { Name = "خدمات خزانات المياه", Description = "تنظيف وصيانة خزانات المياه", Icon = "specialty-icons/default.png" },
            new { Name = "كاميرات المراقبة", Description = "تركيب وصيانة أنظمة المراقبة", Icon = "specialty-icons/default.png" },
            new { Name = "أعمال الحدادة", Description = "تصنيع وصيانة أعمال الحدادة", Icon = "specialty-icons/default.png" }
        };

        var existingNames = await _context.Specialties
            .Select(s => s.Name)
            .ToListAsync();

        var existingNormalized = existingNames
            .Select(NormalizeName)
            .ToHashSet(StringComparer.Ordinal);

        var newSpecialties = specialtiesSeed
            .Where(s => !existingNormalized.Contains(NormalizeName(s.Name)))
            .Select(s => new Specialty
            {
                Id = Guid.NewGuid(),
                Name = s.Name.Trim(),
                Description = s.Description.Trim(),
                Icon = s.Icon.Trim()
            })
            .ToList();

        if (newSpecialties.Count == 0)
        {
            return;
        }

        await _context.Specialties.AddRangeAsync(newSpecialties);
        await _context.SaveChangesAsync();
    }

    private static string NormalizeName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        value = value.Trim();
        value = Regex.Replace(value, @"\s+", " ");
        return value.ToUpperInvariant();
    }

    public async Task IdentityDataSeedingAsync()
    {
        if (!await _roleManager.Roles.AnyAsync())
        {
            await _roleManager.CreateAsync(new IdentityRole<Guid> { Name = "SuperAdmin", NormalizedName = "SUPERADMIN" });
            await _roleManager.CreateAsync(new IdentityRole<Guid> { Name = "Admin", NormalizedName = "ADMIN" });
            await _roleManager.CreateAsync(new IdentityRole<Guid> { Name = "Worker", NormalizedName = "WORKER" });
            await _roleManager.CreateAsync(new IdentityRole<Guid> { Name = "User", NormalizedName = "USER" });
        }
        if(!await _userManager.Users.AnyAsync())
        {
            var now = DateTime.UtcNow;

            var user1 = new ApplicationUser
            {
                UserName = "naila",
                Email = "nailasaleh2004@gmail.com",
                EmailConfirmed = true,
                FirstName = "Naila",
                LastName = "Saleh",
                City = "Qalqilia",
                ProfileImage = string.Empty,
                Status = UserStatus.Existed,
                CreatedAt = now,
                UpdatedAt = now
            };
            var user2 = new ApplicationUser
            {
                UserName = "lina",
                Email = "odehlina91@gmail.com",
                EmailConfirmed = true,
                FirstName = "Lina",
                LastName = "Odeh",
                City = "Tulkarm",
                ProfileImage = string.Empty,
                Status = UserStatus.Existed,
                CreatedAt = now,
                UpdatedAt = now
            };
            var user3 = new ApplicationUser
            {
                UserName = "rula",
                Email = "duraidirula@gmail.com",
                EmailConfirmed = true,
                FirstName = "Rula",
                LastName = "Duraidi",
                City = "Tulkarm",
                ProfileImage = string.Empty,
                Status = UserStatus.Existed,
                CreatedAt = now,
                UpdatedAt = now
            };
            var user4 = new ApplicationUser
            {
                UserName = "majd",
                Email = "majdmnassar05@gmail.com",
                EmailConfirmed = true,
                FirstName = "Majd",
                LastName = "Nassar",
                City = "Nablus",
                ProfileImage = string.Empty,
                Status = UserStatus.Existed,
                CreatedAt = now,
                UpdatedAt = now
            };
            
            var create1 = await _userManager.CreateAsync(user1, "Naila@123");
            var create2 = await _userManager.CreateAsync(user2, "Lina@123");
            var create3 = await _userManager.CreateAsync(user3, "Rula@123");
            var create4 = await _userManager.CreateAsync(user4, "Majd@123");

            if (!create1.Succeeded || !create2.Succeeded || !create3.Succeeded || !create4.Succeeded)
            {
                throw new InvalidOperationException("Failed to create one or more seeded users.");
            }
            
            await _userManager.AddToRoleAsync(user1, "SuperAdmin");
            await _userManager.AddToRoleAsync(user2, "Admin");
            await _userManager.AddToRoleAsync(user3, "Worker");
            await _userManager.AddToRoleAsync(user4, "User");
        }
        await _context.SaveChangesAsync();
    }
}