using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Tadbeer.DAL.Data;

/// <summary>
/// Used exclusively by EF Core tooling (migrations, scaffolding) at design time.
/// It is never invoked at runtime.
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        optionsBuilder.UseSqlServer(
            "Server=.;Database=TadbeerDb;Trusted_Connection=True;TrustServerCertificate=True;");

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
