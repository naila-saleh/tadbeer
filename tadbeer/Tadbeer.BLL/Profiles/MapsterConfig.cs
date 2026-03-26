using Mapster;
using Tadbeer.DAL.DTO.Requests;
using Tadbeer.DAL.DTO.Responses;
using Tadbeer.DAL.Models;

namespace Tadbeer.BLL.Profiles;

public static class MapsterConfig
{
    public static void RegisterMappings()
    {
        // Example configuration (Mapster handles basic properties by matching names automatically):
        // TypeAdapterConfig<ApplicationUserRequestDto, ApplicationUser>
        //     .NewConfig()
        //     .IgnoreNullValues(true);

        // Add custom mapping rules here as your application grows
    }
}
