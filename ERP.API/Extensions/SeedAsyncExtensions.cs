using ERP.API.Services;

namespace ERP.API.Extensions
{
    public static class SeedAsyncExtensions
    {
        public static async Task SeedAsync(this IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var seedDataService = scope.ServiceProvider.GetRequiredService<ISeedDataService>();
            await seedDataService.SeedDataAsync();
        }
    }
}
