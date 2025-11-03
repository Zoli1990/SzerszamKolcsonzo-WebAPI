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

        [Required]
        public DateTime FoglalasVege { get; set; }

        [Required]
        public int OraSzam { get; set; }

        [Required]
        public int Bevetel { get; set; }

        [Required]
        public FoglalasStatus Status { get; set; } = FoglalasStatus.Aktiv;

        [Required]
        public DateTime LetrehozasDatum { get; set; } = DateTime.Now;

        // Navigation property
        [ForeignKey(nameof(EszkozID))]
        public Eszkoz Eszkoz { get; set; } = null!;
    }
}