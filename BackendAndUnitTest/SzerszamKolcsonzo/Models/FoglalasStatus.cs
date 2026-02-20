// ============================================================================
// Models/FoglalasStatus.cs - EGYSZERŰSÍTETT (4 státusz)
// ============================================================================

namespace SzerszamKolcsonzo.Models
{
    /// <summary>
    /// Foglalás státuszok (egyszerűsített)
    /// 
    /// Foglalva  → Kiadva  → Lezárt
    ///    ↓          ↓
    ///  Törölt     Törölt
    /// </summary>
    public enum FoglalasStatus
    {
        /// <summary>
        /// Foglalva - User leadta a foglalást, admin még nem hagyta jóvá
        /// Eszköz blokkolva, admin látja a foglalást
        /// 15 perc után automatikusan törlődik ha nem hagyja jóvá az admin
        /// </summary>
        Foglalva = 0,

        /// <summary>
        /// Kiadva - Admin jóváhagyta, eszköz fizikailag kiadva az ügyfélnek
        /// </summary>
        Kiadva = 1,

        /// <summary>
        /// Lezárt - Eszköz visszahozva, elszámolás megtörtént
        /// Eszköz felszabadul
        /// </summary>
        Lezart = 2,

        /// <summary>
        /// Törölt - Admin kézzel törölte vagy automatikus törlés (15 perc)
        /// Eszköz felszabadul
        /// </summary>
        Torolt = 3
    }
}