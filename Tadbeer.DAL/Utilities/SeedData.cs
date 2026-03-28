using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

    public Task DataSeedingAsync()
    {
        // Not yet implemented – add specialty/booking seed data here when needed.
        return Task.CompletedTask;
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