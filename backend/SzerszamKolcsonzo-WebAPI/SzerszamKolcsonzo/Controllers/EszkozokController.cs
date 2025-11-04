// ============================================================================
// 6. Controllers/EszkozokController.cs - FRISSÍTETT
// ============================================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SzerszamKolcsonzo.Data;
using SzerszamKolcsonzo.Models;
using SzerszamKolcsonzo.DTOs;

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
                eszkoz.Kategoria.Nev
            );

            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<EszkozDetailDto>> CreateEszkoz(CreateEszkozDto dto)
        {
            if (!await _context.Kategoriak.AnyAsync(k => k.KategoriaID == dto.KategoriaID))
            {
                return BadRequest(new { message = "A megadott kategória nem létezik." });
            }

            var eszkoz = new Eszkoz
            {
                KategoriaID = dto.KategoriaID,
                Nev = dto.Nev,
                Leiras = dto.Leiras,
                KepUrl = dto.KepUrl,  // ✅ KepUrl
                Vetelar = dto.Vetelar,
                KiadasiAr = dto.KiadasiAr,
                BeszerzesiDatum = dto.BeszerzesiDatum,
                Status = EszkozStatus.Elerheto
            };

            _context.Eszkozok.Add(eszkoz);
            await _context.SaveChangesAsync();

            await _context.Entry(eszkoz).Reference(e => e.Kategoria).LoadAsync();

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
                eszkoz.Kategoria.Nev
            );

            return CreatedAtAction(nameof(GetEszkoz), new { id = eszkoz.EszkozID }, result);
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
    }
}