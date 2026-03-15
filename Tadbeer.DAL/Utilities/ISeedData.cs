namespace Tadbeer.DAL.Utilities;

public interface ISeedData
{
    Task DataSeedingAsync();
    Task IdentityDataSeedingAsync();
}