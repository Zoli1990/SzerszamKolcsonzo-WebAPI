using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SzerszamKolcsonzo.Data;
using SzerszamKolcsonzo.Models;
using SzerszamKolcsonzo.DTOs;

namespace SzerszamKolcsonzo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class KategoriakController : ControllerBase
    {
        private readonly AppDbContext _context;

        public KategoriakController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<KategoriaDto>>> GetKategoriak()
        {
            var kategoriak = await _context.Kategoriak
                .Select(k => new KategoriaDto(k.KategoriaID, k.Nev, k.KepUrl))  // ✅ KepUrl hozzáadva
                .ToListAsync();

            return Ok(kategoriak);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<KategoriaDto>> GetKategoria(int id)
        {
            var kategoria = await _context.Kategoriak.FindAsync(id);

            if (kategoria == null)
            {
                return NotFound(new { message = "Kategória nem található." });
            }

            return Ok(new KategoriaDto(kategoria.KategoriaID, kategoria.Nev, kategoria.KepUrl));  // ✅ KepUrl
        }

        [HttpPost]
        public async Task<ActionResult<KategoriaDto>> CreateKategoria(CreateKategoriaDto dto)
        {
            if (await _context.Kategoriak.AnyAsync(k => k.Nev == dto.Nev))
            {
                return BadRequest(new { message = "Ez a kategória már létezik." });
            }

            var kategoria = new Kategoria
            {
                Nev = dto.Nev,
                KepUrl = dto.KepUrl  // ✅ KepUrl
            };

            _context.Kategoriak.Add(kategoria);
            await _context.SaveChangesAsync();

            var result = new KategoriaDto(kategoria.KategoriaID, kategoria.Nev, kategoria.KepUrl);

            return CreatedAtAction(nameof(GetKategoria), new { id = kategoria.KategoriaID }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateKategoria(int id, UpdateKategoriaDto dto)
        {
            var kategoria = await _context.Kategoriak.FindAsync(id);

            if (kategoria == null)
            {
                return NotFound(new { message = "Kategória nem található." });
            }

            if (await _context.Kategoriak.AnyAsync(k => k.Nev == dto.Nev && k.KategoriaID != id))
            {
                return BadRequest(new { message = "Ez a kategórianév már foglalt." });
            }

            kategoria.Nev = dto.Nev;
            kategoria.KepUrl = dto.KepUrl;  // ✅ KepUrl frissítés

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteKategoria(int id)
        {
            var kategoria = await _context.Kategoriak
                .Include(k => k.Eszkozok)
                .FirstOrDefaultAsync(k => k.KategoriaID == id);

            if (kategoria == null)
            {
                return NotFound(new { message = "Kategória nem található." });
            }

            if (kategoria.Eszkozok.Any())
            {
                return BadRequest(new { message = "Nem törölhető a kategória, mert vannak hozzá eszközök." });
            }

            _context.Kategoriak.Remove(kategoria);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}