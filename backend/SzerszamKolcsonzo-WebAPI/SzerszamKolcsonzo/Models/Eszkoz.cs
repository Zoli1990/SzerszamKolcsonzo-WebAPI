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

        [Required]
        public int Vetelar { get; set; }

        [Required]
        public int KiadasiAr { get; set; }

        [Required]
        public DateTime BeszerzesiDatum { get; set; }

        [Required]
        public EszkozStatus Status { get; set; } = EszkozStatus.Elerheto;

        // Navigation properties
        [ForeignKey(nameof(KategoriaID))]
        public Kategoria Kategoria { get; set; } = null!;

        public ICollection<Foglalas> Foglalasok { get; set; } = new List<Foglalas>();
    }
}