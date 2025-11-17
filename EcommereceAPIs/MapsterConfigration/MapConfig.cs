using Mapster;

namespace ECommerceAPI.MapsterConfigration
{
    public static class MapConfig
    {
        public static void RegisterMaps(this IServiceCollection services)
        {
            //TypeAdapterConfig<ApplicationUser, UpdateProfileVM>.NewConfig()
            //    .Map(d => d.FullName, s => $"{s.FirstName} {s.LastName}").TwoWays();
        }
    }
}
