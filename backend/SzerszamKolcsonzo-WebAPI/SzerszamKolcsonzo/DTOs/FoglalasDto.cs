using SzerszamKolcsonzo.Models;

public record FoglalasDto(
        int FoglalasID,
        int EszkozID,
        string EszkozNev,
        string Nev,
        string Email,
        string Telefonszam,
        string Cim,
        DateTime FoglalasKezdete,
        // ❌ FoglalasVege - TÖRÖLVE
        // ❌ OraSzam - TÖRÖLVE
        decimal? Bevetel,  // Nullable, mert visszahozáskor számolódik
        FoglalasStatus Status,
        DateTime LetrehozasDatum,
        DateTime? KiadasIdopontja,      // Amikor admin kiadta
        DateTime? VisszahozasIdopontja, // ✅ ÚJ - Amikor visszahozták
        int? ElszamolhatoIdo,           // ✅ ÚJ - Percekben
        decimal? FizetendoOsszeg        // ✅ ÚJ - Számolt összeg
    );

public record CreateFoglalasDto(
    int EszkozID,
    string Nev,
    string Email,
    string Telefonszam,
    string Cim,
    DateTime FoglalasKezdete
// ❌ FoglalasVege - TÖRÖLVE
// ❌ OraSzam - TÖRÖLVE (automatikusan számolódik visszahozáskor)
);