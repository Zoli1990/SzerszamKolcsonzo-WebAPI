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
        DateTime FoglalasVege,
        int OraSzam,
        int Bevetel,
        FoglalasStatus Status,
        DateTime LetrehozasDatum
    );

public record CreateFoglalasDto(
    int EszkozID,
    string Nev,
    string Email,
    string Telefonszam,
    string Cim,
    DateTime FoglalasKezdete,
    DateTime FoglalasVege,
    int OraSzam
);
