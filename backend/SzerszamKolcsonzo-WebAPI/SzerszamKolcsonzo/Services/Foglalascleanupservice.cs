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

            var hatarido = DateTime.UtcNow.Subtract(_foglalasTimeout);

            // Foglalások keresése, ahol:
            // - Status = Foglalva (admin még nem hagyta jóvá)
            // - KiadasIdopontja = null (nem lett kiadva)
            // - LetrehozasDatum + 15 perc < Most
            var lejartFoglalasok = await context.Foglalasok
                .Include(f => f.Eszkoz)
                .Where(f =>
                    f.Status == FoglalasStatus.Foglalva &&
                    f.KiadasIdopontja == null &&
                    f.LetrehozasDatum < hatarido)
                .ToListAsync();

            if (lejartFoglalasok.Any())
            {
                _logger.LogWarning(
                    "[FoglalasCleanup] {Count} lejárt foglalás (15+ perc, nem kiadva)",
                    lejartFoglalasok.Count);

                foreach (var foglalas in lejartFoglalasok)
                {
                    _logger.LogInformation(
                        "[FoglalasCleanup] Törlés: #{FoglalasID} - {EszkozNev} (létrehozva: {Datum})",
                        foglalas.FoglalasID,
                        foglalas.Eszkoz?.Nev ?? "?",
                        foglalas.LetrehozasDatum);

                    // Foglalás → Törölt
                    foglalas.Status = FoglalasStatus.Torolt;

                    // Eszköz felszabadítása (ha még Foglalva státuszban van)
                    if (foglalas.Eszkoz != null && foglalas.Eszkoz.Status == EszkozStatus.Foglalva)
                    {
                        foglalas.Eszkoz.Status = EszkozStatus.Elerheto;

                        _logger.LogInformation(
                            "[FoglalasCleanup] Eszköz felszabadítva: #{EszkozID} - {EszkozNev}",
                            foglalas.Eszkoz.EszkozID,
                            foglalas.Eszkoz.Nev);
                    }
                }

                await context.SaveChangesAsync();

                _logger.LogInformation(
                    "[FoglalasCleanup] {Count} foglalás törölve",
                    lejartFoglalasok.Count);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("[FoglalasCleanup] Leállítás...");
            await base.StopAsync(cancellationToken);
        }
    }
}