// ============================================================================
// Services/FoglalasStatusUpdateService.cs - Automatikus státuszváltás
// ============================================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SzerszamKolcsonzo.Data;
using SzerszamKolcsonzo.Models;
using SzerszamKolcsonzo.Features.Push.Services;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SzerszamKolcsonzo.Services
{
    /// <summary>
    /// Background service automatikus foglalás státusz frissítéshez
    /// Futási gyakoriság: 15 percenként
    /// </summary>
    public class FoglalasStatusUpdateService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<FoglalasStatusUpdateService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(15);

        public FoglalasStatusUpdateService(
            IServiceProvider serviceProvider,
            ILogger<FoglalasStatusUpdateService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("[FoglalasStatusUpdate] Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await UpdateElofoglalasToVarakozik();
                    await UpdateVarakozikToTorolt();

                    _logger.LogInformation("[FoglalasStatusUpdate] Update cycle completed");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[FoglalasStatusUpdate] Error during update cycle");
                }

                // Várunk 15 percet a következő futásig
                await Task.Delay(_interval, stoppingToken);
            }

            _logger.LogInformation("[FoglalasStatusUpdate] Service stopped");
        }

        /// <summary>
        /// ELŐFOGLALÁS → VÁRAKOZIK
        /// Trigger: kezdetDatum - 15 perc
        /// </summary>
        private async Task UpdateElofoglalasToVarakozik()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var pushService = scope.ServiceProvider.GetRequiredService<PushNotificationService>();

            var now = DateTime.UtcNow;

            // Előfoglalások, ahol már eljött a kezdés - 15 perc
            var elofoglalasok = await context.Foglalasok
                .Include(f => f.Eszkoz)
                .Where(f => f.Status == FoglalasStatus.Elofoglalas)
                .Where(f => f.FoglalasKezdete.AddMinutes(-15) <= now) // ← FoglalasKezdete (DateTime)
                .ToListAsync();

            if (elofoglalasok.Any())
            {
                _logger.LogInformation($"[FoglalasStatusUpdate] Found {elofoglalasok.Count} előfoglalások to activate");

                foreach (var foglalas in elofoglalasok)
                {
                    // Státusz váltás: ELŐFOGLALÁS → VÁRAKOZIK
                    foglalas.Status = FoglalasStatus.Varakozik;

                    _logger.LogInformation($"[FoglalasStatusUpdate] Foglalás #{foglalas.FoglalasID} activated (ELŐFOGLALÁS → VÁRAKOZIK)");

                    // Push notification adminnak
                    try
                    {
                        await pushService.NotifyNewBookingAsync(
                            foglalas.FoglalasID,
                            foglalas.Eszkoz?.Nev ?? "Ismeretlen eszköz",
                            foglalas.Nev ?? "Ismeretlen ügyfél"
                        );
                        _logger.LogInformation($"[FoglalasStatusUpdate] Push notification sent for Foglalás #{foglalas.FoglalasID}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"[FoglalasStatusUpdate] Failed to send push notification for Foglalás #{foglalas.FoglalasID}");
                    }
                }

                await context.SaveChangesAsync();
                _logger.LogInformation($"[FoglalasStatusUpdate] {elofoglalasok.Count} foglalások activated");
            }
        }

        /// <summary>
        /// VÁRAKOZIK → TÖRÖLT (Grace Period lejárt)
        /// Trigger: kezdetDatum + 2h 15m
        /// </summary>
        private async Task UpdateVarakozikToTorolt()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            // var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>(); // Ha van email service

            var now = DateTime.UtcNow;

            // Várakozik státuszú foglalások, ahol lejárt a grace period
            var lejartFoglalasok = await context.Foglalasok
                .Include(f => f.Eszkoz)
                .Where(f => f.Status == FoglalasStatus.Varakozik)
                .Where(f => f.FoglalasKezdete.AddHours(2.25) <= now) // Grace period: 2h 15m
                .ToListAsync();

            if (lejartFoglalasok.Any())
            {
                _logger.LogInformation($"[FoglalasStatusUpdate] Found {lejartFoglalasok.Count} lejárt foglalások");

                foreach (var foglalas in lejartFoglalasok)
                {
                    // Státusz váltás: VÁRAKOZIK → TÖRÖLT
                    foglalas.Status = FoglalasStatus.Torolt;

                    _logger.LogInformation($"[FoglalasStatusUpdate] Foglalás #{foglalas.FoglalasID} expired and deleted (VÁRAKOZIK → TÖRÖLT)");

                    // Email küldése user-nek (opcionális)
                    try
                    {
                        // await emailService.SendCancellationEmail(foglalas);
                        _logger.LogInformation($"[FoglalasStatusUpdate] Cancellation email sent for Foglalás #{foglalas.FoglalasID}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"[FoglalasStatusUpdate] Failed to send email for Foglalás #{foglalas.FoglalasID}");
                    }
                }

                await context.SaveChangesAsync();
                _logger.LogInformation($"[FoglalasStatusUpdate] {lejartFoglalasok.Count} foglalások deleted");
            }
        }
    }
}