using Microsoft.EntityFrameworkCore;
using SzerszamKolcsonzo.Data;
using SzerszamKolcsonzo.Models;

namespace SzerszamKolcsonzo.Services
{
    /// <summary>
    /// Háttérszolgáltatás: automatikusan törli a foglalásokat,
    /// ha 15 percen belül nem lett kiadva az eszköz (admin nem hagyta jóvá).
    /// Foglalva → Törölt + Eszköz felszabadul
    /// </summary>
    public class FoglalasCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<FoglalasCleanupService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1); // 1 percenként ellenőriz
        private readonly TimeSpan _foglalasTimeout = TimeSpan.FromMinutes(15); // 15 perc várakozás

        public FoglalasCleanupService(
            IServiceProvider serviceProvider,
            ILogger<FoglalasCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("[FoglalasCleanup] Service elindult");

            // Várunk 30 másodpercet az első ellenőrzés előtt
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await TorolLejartFoglalasokat();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[FoglalasCleanup] Hiba történt az ellenőrzés közben");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("[FoglalasCleanup] Service leállt");
        }

        private async Task TorolLejartFoglalasokat()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var now = DateTime.UtcNow;

            // ────────────────────────────────────────────────────────────
            // 1. Lejárt foglalások törlése
            // Feltétel: FoglalasKezdete + 15 perc < Most (nem jelent meg)
            // ────────────────────────────────────────────────────────────
            var lejartFoglalasok = await context.Foglalasok
                .Include(f => f.Eszkoz)
                .Where(f =>
                    f.Status == FoglalasStatus.Foglalva &&
                    f.KiadasIdopontja == null &&
                    f.FoglalasKezdete.AddMinutes(15) < now)
                .ToListAsync();

            if (lejartFoglalasok.Any())
            {
                _logger.LogWarning(
                    "[FoglalasCleanup] {Count} lejárt foglalás (15+ perc a kezdet után, nem kiadva)",
                    lejartFoglalasok.Count);

                foreach (var foglalas in lejartFoglalasok)
                {
                    _logger.LogInformation(
                        "[FoglalasCleanup] Törlés: #{FoglalasID} - {EszkozNev} (kezdet: {Datum})",
                        foglalas.FoglalasID,
                        foglalas.Eszkoz?.Nev ?? "?",
                        foglalas.FoglalasKezdete);

                    foglalas.Status = FoglalasStatus.Torolt;
                }

                await context.SaveChangesAsync();

                _logger.LogInformation(
                    "[FoglalasCleanup] {Count} foglalás törölve",
                    lejartFoglalasok.Count);
            }

            // ────────────────────────────────────────────────────────────
            // 2. Közelgő foglalások értesítése
            // Ha az eszköz Elerheto ÉS a foglalás 15 percen belül esedékes
            // ÉS még nem küldtünk értesítést → értesítés küldése
            // ────────────────────────────────────────────────────────────
            var kozelgoFoglalasok = await context.Foglalasok
                .Include(f => f.Eszkoz)
                .Where(f =>
                    f.Status == FoglalasStatus.Foglalva &&
                    f.KiadasIdopontja == null &&
                    !f.ErtesitesKuldve &&
                    f.FoglalasKezdete <= now.AddMinutes(15) &&
                    f.FoglalasKezdete > now &&
                    f.Eszkoz.Status == EszkozStatus.Elerheto)
                .ToListAsync();

            foreach (var foglalas in kozelgoFoglalasok)
            {
                foglalas.ErtesitesKuldve = true;

                // TODO: email/push küldés a foglalónak
                _logger.LogInformation(
                    "[FoglalasCleanup] Értesítés: {EszkozNev} elérhető → {Email} ({Nev}), kezdet: {Kezdet:g}",
                    foglalas.Eszkoz?.Nev, foglalas.Email, foglalas.Nev, foglalas.FoglalasKezdete);
            }

            if (kozelgoFoglalasok.Any())
                await context.SaveChangesAsync();
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("[FoglalasCleanup] Leállítás...");
            await base.StopAsync(cancellationToken);
        }
    }
}