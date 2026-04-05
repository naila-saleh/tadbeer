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

    public async Task SpecialtiesDataSeedingAsync()
    {
        // Ensure specialty-icons directory exists in wwwroot
        // Use a more reliable path - get the executing assembly location
        var currentDir = AppContext.BaseDirectory;
        var wwwrootPath = Path.Combine(currentDir, "wwwroot");
        var iconsDirPath = Path.Combine(wwwrootPath, "specialty-icons");
        
        // If not found in standard location, try alternative path
        if (!Directory.Exists(wwwrootPath))
        {
            wwwrootPath = Path.Combine(currentDir, "..", "..", "wwwroot");
            iconsDirPath = Path.Combine(wwwrootPath, "specialty-icons");
        }

        // Normalize the path
        wwwrootPath = Path.GetFullPath(wwwrootPath);
        iconsDirPath = Path.GetFullPath(iconsDirPath);
        
        Directory.CreateDirectory(iconsDirPath);

        // Create a default placeholder PNG icon if it doesn't exist
        var defaultIconPath = Path.Combine(iconsDirPath, "default.png");
        if (!File.Exists(defaultIconPath))
        {
            CreateDefaultIconFile(defaultIconPath);
        }

        const string defaultIconUrl = "specialty-icons/default.png";
        
        var specialtiesSeed = new[]
        {
            new { Name = "خدمات التنظيف", Description = "خدمات تنظيف المنازل والمكاتب والمنشآت", Icon = defaultIconUrl },
            new { Name = "الأجهزة المنزلية", Description = "صيانة وإصلاح الأجهزة المنزلية", Icon = defaultIconUrl },
            new { Name = "أعمال الكهرباء", Description = "تركيب وصيانة الأعطال الكهربائية", Icon = defaultIconUrl },
            new { Name = "دهانات وتشطيبات وديكور", Description = "أعمال دهان وتشطيب وديكور داخلي", Icon = defaultIconUrl },
            new { Name = "أعمال السباكة", Description = "إصلاح وتركيب تمديدات السباكة", Icon = defaultIconUrl },
            new { Name = "صيانة التكييف", Description = "صيانة وتنظيف وتركيب أجهزة التكييف", Icon = defaultIconUrl },
            new { Name = "أعمال الزراعة", Description = "خدمات الزراعة وتنسيق الحدائق", Icon = defaultIconUrl },
            new { Name = "فني ستالايت", Description = "تركيب وضبط وصيانة أجهزة الستالايت", Icon = defaultIconUrl },
            new { Name = "أعمال الألمنيوم", Description = "تصنيع وتركيب وصيانة الألمنيوم", Icon = defaultIconUrl },
            new { Name = "أعمال النجارة", Description = "تصنيع وصيانة الأعمال الخشبية", Icon = defaultIconUrl },
            new { Name = "حرفي", Description = "خدمات حرفية متنوعة", Icon = defaultIconUrl },
            new { Name = "خدمات خزانات المياه", Description = "تنظيف وصيانة خزانات المياه", Icon = defaultIconUrl },
            new { Name = "كاميرات المراقبة", Description = "تركيب وصيانة أنظمة المراقبة", Icon = defaultIconUrl },
            new { Name = "أعمال الحدادة", Description = "تصنيع وصيانة أعمال الحدادة", Icon = defaultIconUrl }
        };

        var existingSpecialties = await _context.Specialties.ToListAsync();

        // Update existing specialties that have null or empty icons
        foreach (var specialty in existingSpecialties)
        {
            if (string.IsNullOrWhiteSpace(specialty.Icon))
            {
                specialty.Icon = defaultIconUrl;
            }
        }

        var existingNames = existingSpecialties
            .Select(s => s.Name)
            .ToList();

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

        if (newSpecialties.Count > 0)
        {
            await _context.Specialties.AddRangeAsync(newSpecialties);
        }

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Creates a minimal valid PNG file as a placeholder default icon.
    /// This is a 1x1 pixel transparent PNG.
    /// </summary>
    private static void CreateDefaultIconFile(string filePath)
    {
        // Minimal 1x1 transparent PNG (PNG header + IHDR chunk + IDAT chunk + IEND chunk)
        byte[] pngData = new byte[]
        {
            // PNG signature
            0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A,
            // IHDR chunk (13 bytes data + 12 bytes header)
            0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52,
            0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01,
            0x08, 0x06, 0x00, 0x00, 0x00, 0x1F, 0x15, 0xC4,
            0x89,
            // IDAT chunk (minimal data + 12 bytes header)
            0x00, 0x00, 0x00, 0x0A, 0x49, 0x44, 0x41, 0x54,
            0x78, 0x9C, 0x63, 0x00, 0x01, 0x00, 0x00, 0x05,
            0x00, 0x01, 0x0D, 0x0A, 0x2D, 0xB4,
            // IEND chunk
            0x00, 0x00, 0x00, 0x00, 0x49, 0x45, 0x4E, 0x44,
            0xAE, 0x42, 0x60, 0x82
        };

        File.WriteAllBytes(filePath, pngData);
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
            await _roleManager.CreateAsync(new IdentityRole<Guid> { Name = nameof(UserRole.SuperAdmin), NormalizedName = nameof(UserRole.SuperAdmin).ToUpperInvariant() });
            await _roleManager.CreateAsync(new IdentityRole<Guid> { Name = nameof(UserRole.Admin), NormalizedName = nameof(UserRole.Admin).ToUpperInvariant() });
            await _roleManager.CreateAsync(new IdentityRole<Guid> { Name = nameof(UserRole.Worker), NormalizedName = nameof(UserRole.Worker).ToUpperInvariant() });
            await _roleManager.CreateAsync(new IdentityRole<Guid> { Name = nameof(UserRole.User), NormalizedName = nameof(UserRole.User).ToUpperInvariant() });
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
            
            await _userManager.AddToRoleAsync(user1, nameof(UserRole.SuperAdmin));
            await _userManager.AddToRoleAsync(user2, nameof(UserRole.Admin));
            await _userManager.AddToRoleAsync(user3, nameof(UserRole.Worker));
            await _userManager.AddToRoleAsync(user4, nameof(UserRole.User));
        }
        await _context.SaveChangesAsync();
    }
}