// ============================================================================
// Controllers/FoglalasokController.cs - EGYSZERŰSÍTETT (4 státusz)
// ============================================================================

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SzerszamKolcsonzo.Data;
using SzerszamKolcsonzo.Features.Push.Services;
using SzerszamKolcsonzo.Models;
using SzerszamKolcsonzo.Services;

namespace SzerszamKolcsonzo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FoglalasokController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<FoglalasokController> _logger;
        private readonly PushNotificationService _pushService;

        public FoglalasokController(
            AppDbContext context,
            ILogger<FoglalasokController> logger,
            PushNotificationService pushService)
        {
            _context = context;
            _logger = logger;
            _pushService = pushService;
        }

        // ====================================================================
        // GET: api/Foglalasok
        // ====================================================================
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<object>>> GetFoglalasok()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            IQueryable<Foglalas> query = _context.Foglalasok
                .Include(f => f.Eszkoz);

            // Admin: Összes foglalás
            // User: Csak saját foglalások
            if (!isAdmin)
            {
                query = query.Where(f => f.Email == User.FindFirstValue(ClaimTypes.Email));
            }

            var foglalasok = await query
                .OrderByDescending(f => f.LetrehozasDatum)
                .Select(f => new
                {
                    foglalasID = f.FoglalasID,
                    eszkozID = f.EszkozID,
                    eszkozNev = f.Eszkoz.Nev,
                    eszkozAr = f.Eszkoz.KiadasiAr,
                    nev = f.Nev,
                    email = f.Email,
                    telefonszam = f.Telefonszam,
                    cim = f.Cim,
                    foglalasKezdete = f.FoglalasKezdete,
                    kiadasIdopontja = f.KiadasIdopontja,
                    visszahozasIdopontja = f.VisszahozasIdopontja,
                    status = f.Status.ToString(),
                    bevetel = f.Bevetel,
                    fizetendoOsszeg = f.FizetendoOsszeg,
                    elszamolhatoIdo = f.ElszamolhatoIdo,
                    letrehozasDatum = f.LetrehozasDatum
                })
                .ToListAsync();

            return Ok(foglalasok);
        }

        // ====================================================================
        // POST: api/Foglalasok - Új foglalás létrehozása
        // ====================================================================
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Foglalas>> CreateFoglalas([FromBody] FoglalasCreateDto dto)
        {
            // ═══════════════════════════════════════════════════════════════
            // TRANZAKCIÓ + FOR UPDATE ZÁROLÁS
            // Megakadályozza, hogy két foglalás egyszerre érkezzen
            // ugyanarra az eszközre (race condition védelem)
            // ═══════════════════════════════════════════════════════════════
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // FOR UPDATE: sorszintű zárolás — amíg ez a tranzakció fut,
                // más kérés VÁRAKOZIK ha ugyanerre az eszközre próbál foglalni
                var eszkoz = await _context.Eszkozok
                    .FromSqlRaw("SELECT * FROM Eszkozok WHERE EszkozID = {0} FOR UPDATE", dto.EszkozID)
                    .FirstOrDefaultAsync();

                if (eszkoz == null)
                {
                    await transaction.RollbackAsync();
                    return NotFound(new { message = "Eszköz nem található!" });
                }

                // Státusz ellenőrzés — a zárolás garantálja, hogy ez MINDIG friss
                if (eszkoz.Status != EszkozStatus.Elerheto)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { message = "Eszköz jelenleg nem elérhető! Valaki épp most foglalta le." });
                }

                // User adatok
                var userEmail = User.FindFirstValue(ClaimTypes.Email);
                var userName = User.FindFirstValue(ClaimTypes.Name);

                // Új foglalás
                var foglalas = new Foglalas
                {
                    EszkozID = dto.EszkozID,
                    Nev = dto.Nev ?? userName ?? "Ismeretlen",
                    Email = dto.Email ?? userEmail ?? "nincs@email.com",
                    Telefonszam = dto.Telefonszam ?? "",
                    Cim = dto.Cim ?? "",
                    FoglalasKezdete = dto.FoglalasKezdete,
                    Status = FoglalasStatus.Foglalva,
                    LetrehozasDatum = DateTime.UtcNow
                };

                _context.Foglalasok.Add(foglalas);

                // Eszköz státusz: FOGLALVA
                eszkoz.Status = EszkozStatus.Foglalva;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync(); // ✅ COMMIT — lock feloldódik

                _logger.LogInformation($"[Foglalas] Új foglalás létrehozva: #{foglalas.FoglalasID}");

                // Push notification adminnak (COMMIT után, tranzakción kívül)
                try
                {
                    await _pushService.NotifyNewBookingAsync(
                        foglalas.FoglalasID,
                        eszkoz.Nev,
                        foglalas.Nev
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"[Foglalas] Push notification hiba: #{foglalas.FoglalasID}");
                }

                return CreatedAtAction(nameof(GetFoglalasok), new { id = foglalas.FoglalasID }, new
                {
                    id = foglalas.FoglalasID,
                    message = "Foglalás sikeresen létrehozva!"
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "[Foglalas] Hiba a foglalás létrehozásakor");
                return StatusCode(500, new { message = "Hiba történt a foglalás során. Próbáld újra!" });
            }
        }

        // ====================================================================
        // PUT: api/Foglalasok/{id}/kiadas - FOGLALVA → KIADVA
        // ====================================================================
        [HttpPut("{id}/kiadas")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Kiadas(int id)
        {
            var foglalas = await _context.Foglalasok
                .Include(f => f.Eszkoz)
                .FirstOrDefaultAsync(f => f.FoglalasID == id);

            if (foglalas == null)
            {
                return NotFound(new { message = "Foglalás nem található!" });
            }

            // Státusz validáció: CSAK FOGLALVA → KIADVA
            if (foglalas.Status != FoglalasStatus.Foglalva)
            {
                return BadRequest(new
                {
                    message = "Csak FOGLALVA státuszú foglalást lehet kiadni!",
                    currentStatus = foglalas.Status.ToString()
                });
            }

            // Státusz váltás
            foglalas.Status = FoglalasStatus.Kiadva;
            foglalas.KiadasIdopontja = DateTime.UtcNow;

            // Eszköz státusz: KIADVA
            if (foglalas.Eszkoz != null)
            {
                foglalas.Eszkoz.Status = EszkozStatus.Kiadva;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation($"[Foglalas] Eszköz kiadva: Foglalás #{id}");

            return Ok(new { message = "Eszköz sikeresen kiadva!" });
        }

        // ====================================================================
        // PUT: api/Foglalasok/{id}/lezaras - KIADVA → LEZÁRT (VISSZAHOZVA)
        // ====================================================================
        [HttpPut("{id}/lezaras")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Lezaras(int id)
        {
            var foglalas = await _context.Foglalasok
                .Include(f => f.Eszkoz)
                .FirstOrDefaultAsync(f => f.FoglalasID == id);

            if (foglalas == null)
            {
                return NotFound(new { message = "Foglalás nem található!" });
            }

            // Státusz validáció: CSAK KIADVA → LEZÁRT
            if (foglalas.Status != FoglalasStatus.Kiadva)
            {
                return BadRequest(new
                {
                    message = "Csak KIADVA státuszú foglalást lehet lezárni!",
                    currentStatus = foglalas.Status.ToString()
                });
            }

            // Visszahozás időpontja
            foglalas.VisszahozasIdopontja = DateTime.UtcNow;

            // Díjszámítás
            if (foglalas.KiadasIdopontja.HasValue && foglalas.Eszkoz != null)
            {
                var elapsed = foglalas.VisszahozasIdopontja.Value - foglalas.KiadasIdopontja.Value;
                var elapsedMinutes = (int)Math.Ceiling(elapsed.TotalMinutes);

                // Elszámolható idő (percekben)
                foglalas.ElszamolhatoIdo = elapsedMinutes;

                // Fizetendő összeg (percenkénti díj: KiadasiAr / 60)
                decimal percenkentiDij = foglalas.Eszkoz.KiadasiAr / 60m;
                foglalas.FizetendoOsszeg = elapsedMinutes * percenkentiDij;
                foglalas.Bevetel = foglalas.FizetendoOsszeg;

                _logger.LogInformation($"[Foglalas] Díjszámítás: {elapsedMinutes} perc, {foglalas.FizetendoOsszeg:F2} Ft");
            }

            // Státusz váltás
            foglalas.Status = FoglalasStatus.Lezart;

            // Eszköz státusz: ELÉRHETŐ (felszabadul)
            if (foglalas.Eszkoz != null)
            {
                foglalas.Eszkoz.Status = EszkozStatus.Elerheto;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation($"[Foglalas] Foglalás lezárva: #{id}, Végső ár: {foglalas.FizetendoOsszeg} Ft");

            return Ok(new
            {
                message = "Foglalás sikeresen lezárva!",
                fizetendoOsszeg = foglalas.FizetendoOsszeg,
                elszamolhatoIdo = foglalas.ElszamolhatoIdo
            });
        }

        // ====================================================================
        // PUT: api/Foglalasok/{id}/torles - FOGLALVA/KIADVA → TÖRÖLT
        // ====================================================================
        [HttpPut("{id}/torles")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Torles(int id)
        {
            var foglalas = await _context.Foglalasok
                .Include(f => f.Eszkoz)
                .FirstOrDefaultAsync(f => f.FoglalasID == id);

            if (foglalas == null)
            {
                return NotFound(new { message = "Foglalás nem található!" });
            }

            // Státusz validáció: CSAK FOGLALVA vagy KIADVA → TÖRÖLT
            if (foglalas.Status != FoglalasStatus.Foglalva && foglalas.Status != FoglalasStatus.Kiadva)
            {
                return BadRequest(new
                {
                    message = "Csak FOGLALVA vagy KIADVA státuszú foglalást lehet törölni!",
                    currentStatus = foglalas.Status.ToString()
                });
            }

            // Státusz váltás
            foglalas.Status = FoglalasStatus.Torolt;

            // Eszköz státusz: ELÉRHETŐ (felszabadul)
            if (foglalas.Eszkoz != null)
            {
                foglalas.Eszkoz.Status = EszkozStatus.Elerheto;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation($"[Foglalas] Foglalás törölve: #{id}");

            return Ok(new { message = "Foglalás törölve!" });
        }

        // ====================================================================
        // DELETE: api/Foglalasok/{id} - Foglalás végleges törlése (ADMIN)
        // ====================================================================
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteFoglalas(int id)
        {
            var foglalas = await _context.Foglalasok
                .Include(f => f.Eszkoz)
                .FirstOrDefaultAsync(f => f.FoglalasID == id);

            if (foglalas == null)
            {
                return NotFound(new { message = "Foglalás nem található!" });
            }

            // Ha az eszköz foglalva/kiadva volt, felszabadítjuk
            if (foglalas.Eszkoz != null &&
                (foglalas.Status == FoglalasStatus.Foglalva || foglalas.Status == FoglalasStatus.Kiadva))
            {
                foglalas.Eszkoz.Status = EszkozStatus.Elerheto;
            }

            _context.Foglalasok.Remove(foglalas);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"[Foglalas] Foglalás véglegesen törölve: #{id}");

            return Ok(new { message = "Foglalás véglegesen törölve!" });
        }
    }

    // ====================================================================
    // DTOs
    // ====================================================================
    public class FoglalasCreateDto
    {
        public int EszkozID { get; set; }
        public string? Nev { get; set; }
        public string? Email { get; set; }
        public string? Telefonszam { get; set; }
        public string? Cim { get; set; }
        public DateTime FoglalasKezdete { get; set; }
    }
}