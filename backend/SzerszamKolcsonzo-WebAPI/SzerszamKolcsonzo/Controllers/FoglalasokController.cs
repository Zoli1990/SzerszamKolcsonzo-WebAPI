using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using SzerszamKolcsonzo.Data;
using SzerszamKolcsonzo.Models;

[ApiController]
[Route("api/[controller]")]
public class FoglalasokController : ControllerBase
{
    private readonly AppDbContext _context;

    public FoglalasokController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/foglalasok (admin látja az összeset)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<FoglalasDto>>> GetFoglalasok([FromQuery] int? eszkozId = null)
    {
        var query = _context.Foglalasok
            .Include(f => f.Eszkoz)
            .AsQueryable();

        if (eszkozId.HasValue)
        {
            query = query.Where(f => f.EszkozID == eszkozId.Value);
        }

        var foglalasok = await query
            .Select(f => new FoglalasDto(
                f.FoglalasID,
                f.EszkozID,
                f.Eszkoz.Nev,
                f.Nev,
                f.Email,
                f.Telefonszam,
                f.Cim,
                f.FoglalasKezdete,
                f.FoglalasVege,
                f.OraSzam,
                f.Bevetel,
                f.Status,
                f.LetrehozasDatum
            ))
            .OrderByDescending(f => f.LetrehozasDatum)
            .ToListAsync();

        return Ok(foglalasok);
    }

    // GET: api/foglalasok/5
    [HttpGet("{id}")]
    public async Task<ActionResult<FoglalasDto>> GetFoglalas(int id)
    {
        var foglalas = await _context.Foglalasok
            .Include(f => f.Eszkoz)
            .FirstOrDefaultAsync(f => f.FoglalasID == id);

        if (foglalas == null)
        {
            return NotFound(new { message = "Foglalás nem található." });
        }

        var result = new FoglalasDto(
            foglalas.FoglalasID,
            foglalas.EszkozID,
            foglalas.Eszkoz.Nev,
            foglalas.Nev,
            foglalas.Email,
            foglalas.Telefonszam,
            foglalas.Cim,
            foglalas.FoglalasKezdete,
            foglalas.FoglalasVege,
            foglalas.OraSzam,
            foglalas.Bevetel,
            foglalas.Status,
            foglalas.LetrehozasDatum
        );

        return Ok(result);
    }

    // POST: api/foglalasok (bárki, regisztráció nélkül is)
    [HttpPost]
    public async Task<ActionResult<FoglalasDto>> CreateFoglalas(CreateFoglalasDto dto)
    {
        var eszkoz = await _context.Eszkozok.FindAsync(dto.EszkozID);

        if (eszkoz == null)
        {
            return NotFound(new { message = "Az eszköz nem található." });
        }

        // Ellenõrzés: eszköz elérhetõ-e?
        if (eszkoz.Status == EszkozStatus.Kiadva)
        {
            return BadRequest(new { message = "Az eszköz jelenleg ki van adva, nem foglalható." });
        }

        // Bevétel automatikus számítása
        int bevetel = dto.OraSzam * eszkoz.KiadasiAr;

        var foglalas = new Foglalas
        {
            EszkozID = dto.EszkozID,
            Nev = dto.Nev,
            Email = dto.Email,
            Telefonszam = dto.Telefonszam,
            Cim = dto.Cim,
            FoglalasKezdete = dto.FoglalasKezdete,
            FoglalasVege = dto.FoglalasVege,
            OraSzam = dto.OraSzam,
            Bevetel = bevetel,
            Status = FoglalasStatus.Aktiv,
            LetrehozasDatum = DateTime.Now
        };

        // Eszköz státuszának frissítése
        eszkoz.Status = EszkozStatus.Kiadva;

        _context.Foglalasok.Add(foglalas);
        await _context.SaveChangesAsync();

        // Betöltjük az eszközt a válaszhoz
        await _context.Entry(foglalas).Reference(f => f.Eszkoz).LoadAsync();

        var result = new FoglalasDto(
            foglalas.FoglalasID,
            foglalas.EszkozID,
            foglalas.Eszkoz.Nev,
            foglalas.Nev,
            foglalas.Email,
            foglalas.Telefonszam,
            foglalas.Cim,
            foglalas.FoglalasKezdete,
            foglalas.FoglalasVege,
            foglalas.OraSzam,
            foglalas.Bevetel,
            foglalas.Status,
            foglalas.LetrehozasDatum
        );

        return CreatedAtAction(nameof(GetFoglalas), new { id = foglalas.FoglalasID }, result);
    }

    // PUT: api/foglalasok/5/lezaras (admin - visszahozás)
    [HttpPut("{id}/lezaras")]
    public async Task<IActionResult> LezarasFoglalas(int id)
    {
        var foglalas = await _context.Foglalasok
            .Include(f => f.Eszkoz)
            .FirstOrDefaultAsync(f => f.FoglalasID == id);

        if (foglalas == null)
        {
            return NotFound(new { message = "Foglalás nem található." });
        }

        if (foglalas.Status != FoglalasStatus.Aktiv)
        {
            return BadRequest(new { message = "Csak aktív foglalás zárható le." });
        }

        // Foglalás lezárása
        foglalas.Status = FoglalasStatus.Lezart;

        // Eszköz státuszának visszaállítása
        foglalas.Eszkoz.Status = EszkozStatus.Elerheto;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/foglalasok/5 (admin - törlés/stornó)
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFoglalas(int id)
    {
        var foglalas = await _context.Foglalasok
            .Include(f => f.Eszkoz)
            .FirstOrDefaultAsync(f => f.FoglalasID == id);

        if (foglalas == null)
        {
            return NotFound(new { message = "Foglalás nem található." });
        }

        // Ha aktív foglalást törlünk, az eszközt is szabaddá kell tenni
        if (foglalas.Status == FoglalasStatus.Aktiv)
        {
            foglalas.Eszkoz.Status = EszkozStatus.Elerheto;
        }

        // Soft delete: státusz Torolt-re
        foglalas.Status = FoglalasStatus.Torolt;

        await _context.SaveChangesAsync();

        return NoContent();
    }
}