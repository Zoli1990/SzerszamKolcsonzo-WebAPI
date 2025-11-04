// ============================================================================
// 4. DTOs/EszkozDto.cs - FRISSÍTETT
// ============================================================================

using SzerszamKolcsonzo.Models;

namespace SzerszamKolcsonzo.DTOs
{
    // Eszköz lista (publikus - mindenki látja)
    public record EszkozListDto(
        int EszkozID,
        string Nev,
        string? Leiras,
        string? KepUrl,  // ✅ ÚJ!
        int KiadasiAr,
        EszkozStatus Status,
        string KategoriaNev,
        int KategoriaID
    );

    // Eszköz részletes (admin látja)
    public record EszkozDetailDto(
        int EszkozID,
        int KategoriaID,
        string Nev,
        string? Leiras,
        string? KepUrl,  // ✅ ÚJ!
        int Vetelar,
        int KiadasiAr,
        DateTime BeszerzesiDatum,
        EszkozStatus Status,
        string KategoriaNev
    );

    // Új eszköz létrehozása
    public record CreateEszkozDto(
        int KategoriaID,
        string Nev,
        string? Leiras,
        string? KepUrl,  // ✅ ÚJ!
        int Vetelar,
        int KiadasiAr,
        DateTime BeszerzesiDatum
    );

    // Eszköz módosítása
    public record UpdateEszkozDto(
        int KategoriaID,
        string Nev,
        string? Leiras,
        string? KepUrl,  // ✅ ÚJ!
        int Vetelar,
        int KiadasiAr,
        DateTime BeszerzesiDatum,
        EszkozStatus Status
    );
}
