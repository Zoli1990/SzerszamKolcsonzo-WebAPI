using Microsoft.EntityFrameworkCore;
using SzerszamKolcsonzo.Data;
using SzerszamKolcsonzo.Models;

namespace SzerszamKolcsonzo.Services
{
    /// <summary>
    /// Háttérszolgáltatás, ami automatikusan törli a foglalásokat, 
    /// ha a FoglalasKezdete időponthoz képest 15 percen belül nem lett kiadva az eszköz
    /// MEGJEGYZÉS: Ez a service TOVÁBBRA IS FUT, de az új FoglalasStatusUpdateService-szel együtt.
    /// Ez a service a régi logikát tartalmazza (LetrehozasDatum alapján törlés)
    /// Az új service pedig a FoglalasKezdete alapján vált státuszt.
    /// </summary>
    public class FoglalasCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<FoglalasCleanupService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5); // 5 percenként ellenőriz
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
            _logger.LogInformation("🚀 FoglalasCleanupService elindult (RÉGI cleanup logic)");

            // Várunk 30 másodpercet, mielőtt az első ellenőrzést futtatnánk
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await TorolLejartFoglalasokat();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Hiba történt a foglalások ellenőrzése közben");
                }

                // Várunk a következő ellenőrzésig
                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("🛑 FoglalasCleanupService leállt");
        }

        private async Task TorolLejartFoglalasokat()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var hatarido = DateTime.UtcNow.Subtract(_foglalasTimeout);

            // Foglalások keresése, ahol:
            // - Status = Elofoglalas VAGY Varakozik (még nincs kiadva)
            // - KiadasIdopontja = null (admin nem hagyta jóvá)
            // - LetrehozasDatum + 15 perc < Most
            var lejartFoglalasok = await context.Foglalasok
                .Include(f => f.Eszkoz)
                .Where(f =>
                    (f.Status == FoglalasStatus.Elofoglalas || f.Status == FoglalasStatus.Varakozik) &&
                    f.KiadasIdopontja == null &&
                    f.LetrehozasDatum < hatarido)
                .ToListAsync();

            if (lejartFoglalasok.Any())
            {
                _logger.LogWarning(
                    "⏰ {Count} darab lejárt foglalást találtunk (15+ perc múlva nem lettek kiadva)",
                    lejartFoglalasok.Count);

                foreach (var foglalas in lejartFoglalasok)
                {
                    _logger.LogInformation(
                        "🗑️ Foglalás törlése: ID={FoglalasID}, Eszköz={EszkozNev}, Létrehozva={LetrehozasDatum}",
                        foglalas.FoglalasID,
                        foglalas.Eszkoz.Nev,
                        foglalas.LetrehozasDatum);

                    // Foglalás státusz módosítása
                    foglalas.Status = FoglalasStatus.Torolt; // ← JAVÍTVA! (Torolt már létezik az új enum-ban)

                    // Eszköz státusz visszaállítása ELÉRHETŐ-re
                    // (csak akkor, ha még Foglalva státuszban van)
                    if (foglalas.Eszkoz.Status == EszkozStatus.Foglalva)
                    {
                        foglalas.Eszkoz.Status = EszkozStatus.Elerheto;

                        _logger.LogInformation(
                            "✅ Eszköz szabaddá téve: ID={EszkozID}, Név={EszkozNev}",
                            foglalas.Eszkoz.EszkozID,
                            foglalas.Eszkoz.Nev);
                    }
                }

                await context.SaveChangesAsync();

                _logger.LogInformation(
                    "✅ {Count} darab foglalás sikeresen törölve",
                    lejartFoglalasok.Count);
            }
            else
            {
                _logger.LogDebug("✔️ Nincsenek lejárt foglalások");
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("🛑 FoglalasCleanupService leállítás folyamatban...");
            await base.StopAsync(cancellationToken);
        }
    }
}