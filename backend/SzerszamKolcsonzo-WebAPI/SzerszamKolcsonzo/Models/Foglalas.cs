using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SzerszamKolcsonzo.Models
{
    public class Foglalas
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FoglalasID { get; set; }

        [Required]
        public int EszkozID { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nev { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Telefonszam { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Cim { get; set; } = string.Empty;

        [Required]
        public DateTime FoglalasKezdete { get; set; }

        // ❌ FoglalasVege - TÖRÖLVE (már nincs befejező időpont)
        // ❌ OraSzam - TÖRÖLVE (automatikusan számolódik visszahozáskor)

        // ✅ Bevetel még létezik de nullable, mert visszahozáskor számolódik
        public decimal? Bevetel { get; set; }

        [Required]
        public FoglalasStatus Status { get; set; } = FoglalasStatus.Elofoglalas; // ← JAVÍTVA! (Aktiv → Elofoglalas)

        [Required]
        public DateTime LetrehozasDatum { get; set; } = DateTime.Now;

        /// <summary>
        /// Amikor az admin jóváhagyta és kiadta az eszközt (null, ha még nem hagyta jóvá)
        /// </summary>
        public DateTime? KiadasIdopontja { get; set; }

        /// <summary>
        /// Amikor az ügyfél visszahozta az eszközt (null, ha még nem hozta vissza)
        /// </summary>
        public DateTime? VisszahozasIdopontja { get; set; }

        /// <summary>
        /// Elszámolható idő percekben (kiadás → visszahozás)
        /// </summary>
        public int? ElszamolhatoIdo { get; set; }

        /// <summary>
        /// Fizetendő összeg (ElszamolhatoIdo * óradíj alapján számolva)
        /// </summary>
        public decimal? FizetendoOsszeg { get; set; }

        // Navigation property
        [ForeignKey(nameof(EszkozID))]
        public Eszkoz Eszkoz { get; set; } = null!;
    }
}