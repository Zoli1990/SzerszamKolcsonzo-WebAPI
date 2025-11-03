using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SzerszamKolcsonzo.Models
{
    public class Kategoria
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int KategoriaID { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nev { get; set; } = string.Empty;

        // Navigation property
        public ICollection<Eszkoz> Eszkozok { get; set; } = new List<Eszkoz>();
    }
}