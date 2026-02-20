/// <summary>
/// DTO eszköz létrehozásához FormData-val (kép feltöltéssel)
/// Property nevek: lowercase camelCase (frontend kompatibilitás)
/// </summary>
public class CreateEszkozFormDto
{
    public int kategoriaID { get; set; }
    public string nev { get; set; } = string.Empty;
    public string? leiras { get; set; }
    public decimal vetelar { get; set; }
    public decimal kiadasiAr { get; set; }
    public DateTime beszerzesiDatum { get; set; }
}