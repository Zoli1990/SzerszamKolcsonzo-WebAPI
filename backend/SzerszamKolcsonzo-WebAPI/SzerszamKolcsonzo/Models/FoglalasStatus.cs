// ============================================================================
// Models/FoglalasStatus.cs - FRISSÍTETT (5 státusz)
// ============================================================================

namespace SzerszamKolcsonzo.Models
{
    /// <summary>
    /// Foglalás státuszok
    /// </summary>
    public enum FoglalasStatus
    {
        /// <summary>
        /// Elõfoglalás - Jövõbeli foglalás, még nem átvehetõ
        /// NEM blokkol eszközt, NEM jelenik meg Admin PWA-ban
        /// </summary>
        Elofoglalas = 0,

        /// <summary>
        /// Várakozik - Most átvehetõ, admin látja PWA-ban
        /// Blokkol eszközt, Admin PWA-ban látható
        /// </summary>
        Varakozik = 1,

        /// <summary>
        /// Kiadva - User-nél van az eszköz
        /// Blokkol eszközt, Admin PWA-ban látható
        /// </summary>
        Kiadva = 2,

        /// <summary>
        /// Lezárt - Visszahozva, foglalás kész
        /// NEM blokkol eszközt, NEM jelenik meg Admin PWA-ban
        /// </summary>
        Lezart = 3,

        /// <summary>
        /// Törölt - User nem jött / automatikus törlés
        /// NEM blokkol eszközt, NEM jelenik meg Admin PWA-ban
        /// </summary>
        Torolt = 4
    }
}