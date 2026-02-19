// ============================================================================
// Features/Push/Extensions/PushServiceExtensions.cs
// ============================================================================

using SzerszamKolcsonzo.Features.Push.Services;

namespace SzerszamKolcsonzo.Features.Push.Extensions
{
    public static class PushServiceExtensions
    {
        public static IServiceCollection AddPushModule(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // VAPID beállítások
            services.Configure<VapidSettings>(
                configuration.GetSection(VapidSettings.SectionName));

            // Push szolgáltatás
            services.AddScoped<PushNotificationService>();

            return services;
        }
    }
}
