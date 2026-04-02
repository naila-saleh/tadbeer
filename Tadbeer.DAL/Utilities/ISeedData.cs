namespace Tadbeer.DAL.Utilities;

public interface ISeedData
{
    Task SpecialtiesDataSeedingAsync();
    Task IdentityDataSeedingAsync();
}