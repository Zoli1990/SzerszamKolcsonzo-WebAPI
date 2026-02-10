namespace SzerszamKolcsonzo.Models
{
    public enum EszkozStatus
    {
        Elerheto = 0,   // Szabad, foglalható
        Foglalva = 1,   // Lefoglalták, még nem vették át
        Kiadva = 2,     // Fizikailag kiadva az ügyfélnek
        Kivonva = 3     // Ideiglenesen vagy végleg nem használható (szerviz, elveszett, selejt)
    }
}