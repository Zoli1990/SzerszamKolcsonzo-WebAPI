// ============================================================================
// Controllers/FoglalasokController.cs - FRISSÍTETT (státusz validációkkal)
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
        // GET: api/Foglalas
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
                    id = f.FoglalasID,
                    eszkoz = new
                    {
                        id = f.Eszkoz.EszkozID,
                        nev = f.Eszkoz.Nev,
                        ar = f.Eszkoz.KiadasiAr
                    },
                    felhasznalo = new
                    {
                        nev = f.Nev,
                        email = f.Email,
                        telefonszam = f.Telefonszam,
                        cim = f.Cim
                    },
                    kezdetDatum = f.FoglalasKezdete,
                    kiadasDatum = f.KiadasIdopontja,
                    visszahozasDatum = f.VisszahozasIdopontja,
                    status = (int)f.Status,
                    bevetel = f.Bevetel,
                    fizetendoOsszeg = f.FizetendoOsszeg,
                    elszamolhatoIdo = f.ElszamolhatoIdo,
                    createdAt = f.LetrehozasDatum
                })
                .ToListAsync();

            return Ok(foglalasok);
        }

        // ====================================================================
        // POST: api/Foglalas - Új foglalás létrehozása
        // ====================================================================
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Foglalas>> CreateFoglalas([FromBody] FoglalasCreateDto dto)
        {
            // Eszköz létezik?
            var eszkoz = await _context.Eszkozok.FindAsync(dto.EszkozID);
            if (eszkoz == null)
            {
                return NotFound(new { message = "Eszköz nem található!" });
            }

            // Eszköz elérhető?
            if (eszkoz.Status != EszkozStatus.Elerheto)
            {
                return BadRequest(new { message = "Eszköz jelenleg nem elérhető!" });
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
                FoglalasKezdete = dto.KezdetDatum,
                Status = FoglalasStatus.Elofoglalas, // ← MINDIG ELŐFOGLALÁS!
                LetrehozasDatum = DateTime.UtcNow
            };

            _context.Foglalasok.Add(foglalas);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"[Foglalas] Új előfoglalás létrehozva: #{foglalas.FoglalasID}");

            return CreatedAtAction(nameof(GetFoglalasok), new { id = foglalas.FoglalasID }, new
            {
                id = foglalas.FoglalasID,
                message = "Foglalás sikeresen létrehozva!"
            });
        }

        // ====================================================================
        // PUT: api/Foglalas/{id}/kiadas - VÁRAKOZIK → KIADVA
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

            // Státusz validáció: CSAK VÁRAKOZIK → KIADVA
            if (foglalas.Status != FoglalasStatus.Varakozik)
            {
                return BadRequest(new
                {
                    message = "Csak VÁRAKOZIK státuszú foglalást lehet kiadni!",
                    currentStatus = foglalas.Status.ToString()
                });
            }

            // Státusz váltás
            foglalas.Status = FoglalasStatus.Kiadva; // 1 → 2
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
        // PUT: api/Foglalas/{id}/torles - VÁRAKOZIK → TÖRÖLT (NEM JÖTT)
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

            // Státusz validáció: CSAK VÁRAKOZIK → TÖRÖLT
            if (foglalas.Status != FoglalasStatus.Varakozik)
            {
                return BadRequest(new
                {
                    message = "Csak VÁRAKOZIK státuszú foglalást lehet törölni!",
                    currentStatus = foglalas.Status.ToString()
                });
            }

            // Státusz váltás
            foglalas.Status = FoglalasStatus.Torolt; // 1 → 4

            // Eszköz státusz: ELÉRHETŐ (felszabadul)
            if (foglalas.Eszkoz != null)
            {
                foglalas.Eszkoz.Status = EszkozStatus.Elerheto;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation($"[Foglalas] Foglalás törölve (nem jött): #{id}");

            // TODO: Email küldés user-nek
            // await _emailService.SendCancellationEmail(foglalas);

            return Ok(new { message = "Foglalás törölve!" });
        }

        // ====================================================================
        // PUT: api/Foglalas/{id}/lezaras - KIADVA → LEZÁRT (VISSZAHOZVA)
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
                var elapsedMinutes = (int)elapsed.TotalMinutes;

                // Elszámolható idő (percekben)
                foglalas.ElszamolhatoIdo = elapsedMinutes;

                // Fizetendő összeg (napi díj / 24 * órák)
                var elapsedHours = elapsed.TotalHours;
                var hourlyRate = foglalas.Eszkoz.KiadasiAr/ 24m;
                var totalPrice = (decimal)elapsedHours * hourlyRate;

                foglalas.FizetendoOsszeg = Math.Ceiling(totalPrice); // Felkerekítés
                foglalas.Bevetel = foglalas.FizetendoOsszeg;

                _logger.LogInformation($"[Foglalas] Díjszámítás: {elapsedMinutes} perc, {totalPrice:F2} Ft");
            }

            // Státusz váltás
            foglalas.Status = FoglalasStatus.Lezart; // 2 → 3

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
        // DELETE: api/Foglalas/{id} - Foglalás végleges törlése (ADMIN)
        // ====================================================================
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteFoglalas(int id)
        {
            var foglalas = await _context.Foglalasok.FindAsync(id);

            if (foglalas == null)
            {
                return NotFound(new { message = "Foglalás nem található!" });
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
        public DateTime KezdetDatum { get; set; }
    }
}