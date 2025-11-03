namespace SzerszamKolcsonzo.DTOs
{
    public record KategoriaDto(int KategoriaID, string Nev);

    public record CreateKategoriaDto(string Nev);

    public record UpdateKategoriaDto(string Nev);
}