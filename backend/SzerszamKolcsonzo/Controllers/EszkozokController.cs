using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SzerszamKolcsonzo.Data;
using SzerszamKolcsonzo.Models;
using SzerszamKolcsonzo.DTOs;
using System.Linq;

namespace SzerszamKolcsonzo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EszkozokController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EszkozokController(AppDbContext context)
        {
            _context = context;
        }




        // 🔹 ADMIN: Összes eszköz részletes adatokkal
        [HttpGet("admin")]
        public async Task<ActionResult<IEnumerable<EszkozDetailDto>>> GetEszkozokAdmin()
        {
            var eszkozok = await _context.Eszkozok
                .Include(e => e.Kategoria)
                .Select(e => new EszkozDetailDto(
                    e.EszkozID,
                    e.KategoriaID,
                    e.Nev,
                    e.Leiras,
                    e.KepUrl,
                    e.Vetelar,
                    e.KiadasiAr,
                    e.BeszerzesiDatum,
                    e.Status,
                    e.Kategoria.Nev,
                    e.Megjegyzes  // ✅ ÚJ
                ))
                .ToListAsync();

            return Ok(eszkozok);
        }



        [HttpGet]
        public async Task<ActionResult<IEnumerable<EszkozListDto>>> GetEszkozok([FromQuery] int? kategoriaId = null)
        {
            var query = _context.Eszkozok
                .Include(e => e.Kategoria)
                .AsQueryable();

            if (kategoriaId.HasValue)
            {
                query = query.Where(e => e.KategoriaID == kategoriaId.Value);
            }

            var eszkozok = await query
                .Select(e => new EszkozListDto(
                    e.EszkozID,
                    e.Nev,
                    e.Leiras,
                    e.KepUrl,  // ✅ KepUrl hozzáadva
                    e.KiadasiAr,
                    e.Status,
                    e.Kategoria.Nev,
                    e.KategoriaID
                ))
                .ToListAsync();

            return Ok(eszkozok);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EszkozDetailDto>> GetEszkoz(int id)
        {
            var eszkoz = await _context.Eszkozok
                .Include(e => e.Kategoria)
                .FirstOrDefaultAsync(e => e.EszkozID == id);

            if (eszkoz == null)
            {
                return NotFound(new { message = "Eszköz nem található." });
            }

            var result = new EszkozDetailDto(
                eszkoz.EszkozID,
                eszkoz.KategoriaID,
                eszkoz.Nev,
                eszkoz.Leiras,
                eszkoz.KepUrl,  // ✅ KepUrl
                eszkoz.Vetelar,
                eszkoz.KiadasiAr,
                eszkoz.BeszerzesiDatum,
                eszkoz.Status,
                eszkoz.Kategoria.Nev,
                eszkoz.Megjegyzes  // ✅ ÚJ
            );

            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<EszkozDetailDto>> CreateEszkoz(
    [FromForm] CreateEszkozFormDto dto,
    IFormFile? kep)
        {
            // Validáció
            if (!await _context.Kategoriak.AnyAsync(k => k.KategoriaID == dto.kategoriaID))
            {
                return BadRequest(new { message = "A megadott kategória nem létezik." });
            }

            // ✅ Képfeltöltés
            string? kepUrl = null;
            if (kep != null)
            {
                var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                Directory.CreateDirectory(uploadsDir);

                var fileName = Guid.NewGuid() + Path.GetExtension(kep.FileName);
                var filePath = Path.Combine(uploadsDir, fileName);

                using var stream = System.IO.File.Create(filePath);
                await kep.CopyToAsync(stream);

                kepUrl = "/uploads/" + fileName;
            }

            // ✅ Eszköz létrehozása (lowercase DTO → PascalCase Model)
            var eszkoz = new Eszkoz
            {
                KategoriaID = dto.kategoriaID,      // ← lowercase
                Nev = dto.nev,                       // ← lowercase
                Leiras = dto.leiras,                 // ← lowercase
                KepUrl = kepUrl,
                Vetelar = dto.vetelar,               // ← lowercase
                KiadasiAr = dto.kiadasiAr,           // ← lowercase
                BeszerzesiDatum = dto.beszerzesiDatum, // ← lowercase
                Status = EszkozStatus.Elerheto,
                Megjegyzes = null  // ✅ Explicit null
            };

            _context.Eszkozok.Add(eszkoz);
            await _context.SaveChangesAsync();

            // ✅ Kategória betöltése
            await _context.Entry(eszkoz).Reference(e => e.Kategoria).LoadAsync();

            // ✅ Response
            return CreatedAtAction(nameof(GetEszkoz), new { id = eszkoz.EszkozID },
                new EszkozDetailDto(
                    eszkoz.EszkozID,
                    eszkoz.KategoriaID,
                    eszkoz.Nev,
                    eszkoz.Leiras,
                    eszkoz.KepUrl,
                    eszkoz.Vetelar,
                    eszkoz.KiadasiAr,
                    eszkoz.BeszerzesiDatum,
                    eszkoz.Status,
                    eszkoz.Kategoria.Nev,
                    eszkoz.Megjegyzes
                )
            );
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEszkoz(int id, UpdateEszkozDto dto)
        {
            var eszkoz = await _context.Eszkozok.FindAsync(id);

            if (eszkoz == null)
            {
                return NotFound(new { message = "Eszköz nem található." });
            }

            if (!await _context.Kategoriak.AnyAsync(k => k.KategoriaID == dto.KategoriaID))
            {
                return BadRequest(new { message = "A megadott kategória nem létezik." });
            }

            eszkoz.KategoriaID = dto.KategoriaID;
            eszkoz.Nev = dto.Nev;
            eszkoz.Leiras = dto.Leiras;
            eszkoz.KepUrl = dto.KepUrl;  // ✅ KepUrl frissítés
            eszkoz.Vetelar = dto.Vetelar;
            eszkoz.KiadasiAr = dto.KiadasiAr;
            eszkoz.BeszerzesiDatum = dto.BeszerzesiDatum;
            eszkoz.Status = dto.Status;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEszkoz(int id)
        {
            var eszkoz = await _context.Eszkozok
                .Include(e => e.Foglalasok)
                .FirstOrDefaultAsync(e => e.EszkozID == id);

            if (eszkoz == null)
            {
                return NotFound(new { message = "Eszköz nem található." });
            }

            if (eszkoz.Foglalasok.Any())
            {
                return BadRequest(new { message = "Nem törölhető az eszköz, mert vannak hozzá foglalások." });
            }

            _context.Eszkozok.Remove(eszkoz);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ============================================================================
        // ÚJ ENDPOINTOK - ADMIN ESZKÖZ STÁTUSZKEZELÉS
        // ============================================================================

        /// <summary>
        /// POST: api/eszkozok/{id}/visszahoz
        /// Admin visszaveszi az eszközt (ügyfél visszahozta)
        /// Automatikusan számol: kiadás → visszahozás idő alapján
        /// </summary>
        [HttpPost("{id}/visszahoz")]
        // [Authorize(Roles = "Admin")] // TODO: Uncomment when auth is ready
        public async Task<IActionResult> VisszahozEszkoz(int id)
        {
            var eszkoz = await _context.Eszkozok.FindAsync(id);

            if (eszkoz == null)
            {
                return NotFound(new { message = "Eszköz nem található." });
            }

            // Csak kiadott eszközt lehet visszahozni
            if (eszkoz.Status != EszkozStatus.Kiadva)
            {
                return BadRequest(new { message = "Csak kiadott eszközt lehet visszahozni." });
            }

            // Eszköz státusz visszaállítása
            eszkoz.Status = EszkozStatus.Elerheto;
            eszkoz.Megjegyzes = null;

            // Legutolsó kiadott foglalás lezárása + ELSZÁMOLÁS
            var aktivFoglalas = await _context.Foglalasok
                .Where(f => f.EszkozID == id && f.Status == FoglalasStatus.Kiadva)
                .OrderByDescending(f => f.KiadasIdopontja)
                .FirstOrDefaultAsync();

            if (aktivFoglalas != null)
            {
                // ✅ Visszahozás időpontja
                aktivFoglalas.VisszahozasIdopontja = DateTime.UtcNow;

                // ✅ Elszámolható idő (percekben)
                if (aktivFoglalas.KiadasIdopontja.HasValue)
                {
                    var elteltIdo = aktivFoglalas.VisszahozasIdopontja.Value - aktivFoglalas.KiadasIdopontja.Value;
                    aktivFoglalas.ElszamolhatoIdo = (int)Math.Ceiling(elteltIdo.TotalMinutes);

                    // ✅ Fizetendő összeg (óradíj alapján)
                    // Óradíj → percenkénti díj: KiadasiAr / 60
                    decimal percenkentiDij = eszkoz.KiadasiAr / 60m;
                    aktivFoglalas.FizetendoOsszeg = aktivFoglalas.ElszamolhatoIdo.Value * percenkentiDij;

                    // Bevétel is = FizetendoOsszeg
                    aktivFoglalas.Bevetel = aktivFoglalas.FizetendoOsszeg;
                }

                // Státusz lezárása
                aktivFoglalas.Status = FoglalasStatus.Lezart;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Eszköz sikeresen visszahozva",
                eszkozID = eszkoz.EszkozID,
                eszkozNev = eszkoz.Nev,
                ujStatus = eszkoz.Status,
                // ✅ Elszámolás adatok visszaküldése
                elszamolas = aktivFoglalas != null ? new
                {
                    foglalasID = aktivFoglalas.FoglalasID,
                    kiadasIdopontja = aktivFoglalas.KiadasIdopontja,
                    visszahozasIdopontja = aktivFoglalas.VisszahozasIdopontja,
                    elszamolhatoIdo = aktivFoglalas.ElszamolhatoIdo,
                    fizetendoOsszeg = aktivFoglalas.FizetendoOsszeg
                } : null
            });
        }
        

        /// <summary>
        /// POST: api/eszkozok/{id}/kivon
        /// Admin kivonja az eszközt forgalomból (szerviz, elveszett, stb.)
        /// Body: { "megjegyzes": "Szervizben van" }
        /// </summary>
        [HttpPost("{id}/kivon")]
        // [Authorize(Roles = "Admin")] // TODO: Uncomment when auth is ready
        public async Task<IActionResult> KivonEszkoz(int id, [FromBody] KivonasDto dto)
        {
            var eszkoz = await _context.Eszkozok.FindAsync(id);

            if (eszkoz == null)
            {
                return NotFound(new { message = "Eszköz nem található." });
            }

            // Validáció: megjegyzés nem lehet üres
            if (string.IsNullOrWhiteSpace(dto.Megjegyzes))
            {
                return BadRequest(new { message = "A megjegyzés megadása kötelező." });
            }

            // Eszköz kivonása
            eszkoz.Status = EszkozStatus.Kivonva;
            eszkoz.Megjegyzes = dto.Megjegyzes;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Eszköz sikeresen kivonva",
                eszkozID = eszkoz.EszkozID,
                eszkozNev = eszkoz.Nev,
                megjegyzes = eszkoz.Megjegyzes,
                ujStatus = eszkoz.Status
            });
        }

        /// <summary>
        /// POST: api/eszkozok/{id}/elerheto
        /// Admin visszaállítja az eszközt elérhetőre (pl. szervizből visszajött)
        /// </summary>
        [HttpPost("{id}/elerheto")]
        // [Authorize(Roles = "Admin")] // TODO: Uncomment when auth is ready
        public async Task<IActionResult> ElerhetovaTetelEszkoz(int id)
        {
            var eszkoz = await _context.Eszkozok.FindAsync(id);

            if (eszkoz == null)
            {
                return NotFound(new { message = "Eszköz nem található." });
            }

            // Eszköz visszaállítása elérhetőre
            eszkoz.Status = EszkozStatus.Elerheto;
            eszkoz.Megjegyzes = null; // Töröljük a megjegyzést

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Eszköz sikeresen elérhetővé téve",
                eszkozID = eszkoz.EszkozID,
                eszkozNev = eszkoz.Nev,
                ujStatus = eszkoz.Status
            });
        }
    }
}

// ============================================================================
// ÚJ DTO - KIVONÁS MEGJEGYZÉSSEL
// ============================================================================

/// <summary>
/// DTO eszköz kivonásához megjegyzéssel
/// </summary>
public record KivonasDto(
    string Megjegyzes
);