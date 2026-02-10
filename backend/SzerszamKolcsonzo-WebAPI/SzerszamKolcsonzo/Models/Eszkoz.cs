using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SzerszamKolcsonzo.Models
{
    public class Eszkoz
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EszkozID { get; set; }

        [Required]
        public int KategoriaID { get; set; }

        [Required]
        [MaxLength(150)]
        public string Nev { get; set; } = string.Empty;

        [Column(TypeName = "TEXT")]
        public string? Leiras { get; set; }

        [MaxLength(500)]
        public string? KepUrl { get; set; }  // ✅ ÚJ MEZŐ!

        [Required]
        public decimal Vetelar { get; set; }

        [Required]
        public decimal KiadasiAr { get; set; }

        [Required]
        public DateTime BeszerzesiDatum { get; set; }

        [Required]
        public EszkozStatus Status { get; set; } = EszkozStatus.Elerheto;

        /// <summary>
        /// Megjegyzés az eszköz állapotához (pl. "Szervizben", "Nem hozták vissza", "Leégett")
        /// </summary>
        [MaxLength(500)]
        public string? Megjegyzes { get; set; }

        // Navigation properties
        [ForeignKey(nameof(KategoriaID))]
        public Kategoria Kategoria { get; set; } = null!;

        public ICollection<Foglalas> Foglalasok { get; set; } = new List<Foglalas>();
    }
}