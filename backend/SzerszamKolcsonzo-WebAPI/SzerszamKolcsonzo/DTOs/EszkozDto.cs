using SzerszamKolcsonzo.Models;

namespace SzerszamKolcsonzo.DTOs
{
    // ═══════════════════════════════════════════════════════════════════
    // ESZKÖZ LISTA DTO (Publikus - mindenki látja)
    // ═══════════════════════════════════════════════════════════════════

    public record EszkozListDto(
        int EszkozID,
        string Nev,
        string? Leiras,
        string? KepUrl,
        decimal KiadasiAr,        // ✅ decimal
        EszkozStatus Status,
        string KategoriaNev,
        int KategoriaID
    );

    // ═══════════════════════════════════════════════════════════════════
    // ESZKÖZ RÉSZLETES DTO (Admin látja)
    // ═══════════════════════════════════════════════════════════════════

    public record EszkozDetailDto(
        int EszkozID,
        int KategoriaID,
        string Nev,
        string? Leiras,
        string? KepUrl,
        decimal Vetelar,          // ✅ decimal
        decimal KiadasiAr,        // ✅ decimal
        DateTime BeszerzesiDatum,
        EszkozStatus Status,
        string KategoriaNev,
        string? Megjegyzes
    );

    // ═══════════════════════════════════════════════════════════════════
    // ✨ ÚJ ESZKÖZ LÉTREHOZÁSA - FORMDATA + KÉPFELTÖLTÉS
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// DTO eszköz létrehozásához FormData-val (kép feltöltéssel)
    /// 
    /// FONTOS: Property nevek lowercase camelCase formátumban,
    /// hogy megfeleljenek a frontend által küldött FormData key-eknek.
    /// 
    /// Frontend FormData key-ek:
    /// - kategoriaID
    /// - nev
    /// - leiras
    /// - vetelar
    /// - kiadasiAr
    /// - beszerzesiDatum
    /// - kep (IFormFile)
    /// </summary>
    public class CreateEszkozFormDto
    {
        /// <summary>
        /// Kategória azonosító (kötelező)
        /// </summary>
        public int kategoriaID { get; set; }

        /// <summary>
        /// Eszköz neve (kötelező)
        /// </summary>
        public string nev { get; set; } = string.Empty;

        /// <summary>
        /// Eszköz leírása (opcionális)
        /// </summary>
        public string? leiras { get; set; }

        /// <summary>
        /// Vételár (opcionális, alapértelmezett: 0)
        /// </summary>
        public decimal vetelar { get; set; } = 0;

        /// <summary>
        /// Kiadási ár Ft/óra (kötelező)
        /// </summary>
        public decimal kiadasiAr { get; set; }

        /// <summary>
        /// Beszerzési dátum (opcionális, alapértelmezett: most)
        /// </summary>
        public DateTime beszerzesiDatum { get; set; } = DateTime.UtcNow;
    }

    // ═══════════════════════════════════════════════════════════════════
    // ESZKÖZ MÓDOSÍTÁSA
    // ═══════════════════════════════════════════════════════════════════

    public record UpdateEszkozDto(
        int KategoriaID,
        string Nev,
        string? Leiras,
        string? KepUrl,
        decimal Vetelar,          // ✅ decimal
        decimal KiadasiAr,        // ✅ decimal
        DateTime BeszerzesiDatum,
        EszkozStatus Status
    );
}