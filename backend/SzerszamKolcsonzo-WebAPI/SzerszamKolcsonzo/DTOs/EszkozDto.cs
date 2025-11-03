using SzerszamKolcsonzo.Models;

public record EszkozListDto(
    int EszkozID,
    string Nev,
    string? Leiras,
    int KiadasiAr,
    EszkozStatus Status,
    string KategoriaNev
);

public record EszkozDetailDto(
    int EszkozID,
    int KategoriaID,
    string Nev,
    string? Leiras,
    int Vetelar,
    int KiadasiAr,
    DateTime BeszerzesiDatum,
    EszkozStatus Status,
    string KategoriaNev
);

public record CreateEszkozDto(
    int KategoriaID,
    string Nev,
    string? Leiras,
    int Vetelar,
    int KiadasiAr,
    DateTime BeszerzesiDatum
);

public record UpdateEszkozDto(
    int KategoriaID,
    string Nev,
    string? Leiras,
    int Vetelar,
    int KiadasiAr,
    DateTime BeszerzesiDatum,
    EszkozStatus Status
);