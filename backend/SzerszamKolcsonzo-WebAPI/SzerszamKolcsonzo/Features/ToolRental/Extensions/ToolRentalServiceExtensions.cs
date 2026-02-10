using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SzerszamKolcsonzo.Data;

namespace SzerszamKolcsonzo.Features.ToolRental.Extensions
{
    public static class ToolRentalServiceExtensions
    {
        public static IServiceCollection AddToolRentalModule(this IServiceCollection services, IConfiguration configuration)
        {
            // AppDbContext (Szerszámkölcsönző)
            services.AddDbContext<AppDbContext>(options =>
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            });

            return services;
        }

        public static async Task MigrateToolRentalDatabaseAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            try
            {
                await context.Database.MigrateAsync();
                Console.WriteLine("✅ Szerszámkölcsönző adatbázis migrációja sikeres!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Szerszámkölcsönző migrációs hiba: {ex.Message}");
            }
        }
    }
}

