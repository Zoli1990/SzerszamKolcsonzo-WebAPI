using Microsoft.AspNetCore.Mvc;
using SzerszamKolcsonzo.Data;
using SzerszamKolcsonzo.Models;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class EszkozokController : ControllerBase
{
    private readonly AppDbContext _context;

    public EszkozokController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/eszkozok (publikus - mindenki látja)
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
                e.KiadasiAr,
                e.Status,
                e.Kategoria.Nev
            ))
            .ToListAsync();

        return Ok(eszkozok);
    }

    // GET: api/eszkozok/5 (teljes adat - késõbb csak adminnak)
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
            eszkoz.Vetelar,
            eszkoz.KiadasiAr,
            eszkoz.BeszerzesiDatum,
            eszkoz.Status,
            eszkoz.Kategoria.Nev
        );

        return Ok(result);
    }

    // POST: api/eszkozok (admin)
    [HttpPost]
    public async Task<ActionResult<EszkozDetailDto>> CreateEszkoz(CreateEszkozDto dto)
    {
        // Kategória létezik-e?
        if (!await _context.Kategoriak.AnyAsync(k => k.KategoriaID == dto.KategoriaID))
        {
            return BadRequest(new { message = "A megadott kategória nem létezik." });
        }

        var eszkoz = new Eszkoz
        {
            KategoriaID = dto.KategoriaID,
            Nev = dto.Nev,
            Leiras = dto.Leiras,
            Vetelar = dto.Vetelar,
            KiadasiAr = dto.KiadasiAr,
            BeszerzesiDatum = dto.BeszerzesiDatum,
            Status = EszkozStatus.Elerheto
        };

        _context.Eszkozok.Add(eszkoz);
        await _context.SaveChangesAsync();

        // Betöltjük a kategóriát a válaszhoz
        await _context.Entry(eszkoz).Reference(e => e.Kategoria).LoadAsync();

        var result = new EszkozDetailDto(
            eszkoz.EszkozID,
            eszkoz.KategoriaID,
            eszkoz.Nev,
            eszkoz.Leiras,
            eszkoz.Vetelar,
            eszkoz.KiadasiAr,
            eszkoz.BeszerzesiDatum,
            eszkoz.Status,
            eszkoz.Kategoria.Nev
        );

        return CreatedAtAction(nameof(GetEszkoz), new { id = eszkoz.EszkozID }, result);
    }

    // PUT: api/eszkozok/5 (admin)
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEszkoz(int id, UpdateEszkozDto dto)
    {
        var eszkoz = await _context.Eszkozok.FindAsync(id);

        if (eszkoz == null)
        {
            return NotFound(new { message = "Eszköz nem található." });
        }

        // Kategória létezik-e?
        if (!await _context.Kategoriak.AnyAsync(k => k.KategoriaID == dto.KategoriaID))
        {
            return BadRequest(new { message = "A megadott kategória nem létezik." });
        }

        eszkoz.KategoriaID = dto.KategoriaID;
        eszkoz.Nev = dto.Nev;
        eszkoz.Leiras = dto.Leiras;
        eszkoz.Vetelar = dto.Vetelar;
        eszkoz.KiadasiAr = dto.KiadasiAr;
        eszkoz.BeszerzesiDatum = dto.BeszerzesiDatum;
        eszkoz.Status = dto.Status;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/eszkozok/5 (admin)
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

        // Ne engedjük törölni, ha van foglalása
        if (eszkoz.Foglalasok.Any())
        {
            return BadRequest(new { message = "Nem törölhetõ az eszköz, mert vannak hozzá foglalások." });
        }

        _context.Eszkozok.Remove(eszkoz);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}