namespace SzerszamKolcsonzo.DTOs
{
    public record KategoriaDto(int KategoriaID, string Nev, string? KepUrl);  // ✅ KepUrl hozzáadva

    public record CreateKategoriaDto(string Nev, string? KepUrl);  // ✅ KepUrl hozzáadva

    public record UpdateKategoriaDto(string Nev, string? KepUrl);  // ✅ KepUrl hozzáadva
}
