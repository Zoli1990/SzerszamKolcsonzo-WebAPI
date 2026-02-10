// ============================================================================
// Features/Auth/Models/User.cs - FRISSÍTETT (kibővített cím mezőkkel)
// ============================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SzerszamKolcsonzo.Features.Auth.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Role { get; set; } = "User";

        // === CÍM MEZŐK (KIBŐVÍTETT) ===

        /// <summary>
        /// 4 számjegyű magyar irányítószám (kötelező regisztrációnál)
        /// </summary>
        [MaxLength(4)]
        public string? Iranyitoszam { get; set; }

        /// <summary>
        /// Település neve (automatikusan kitöltődik az irányítószám alapján)
        /// </summary>
        [MaxLength(100)]
        public string? Telepules { get; set; }

        /// <summary>
        /// Utca neve (kötelező regisztrációnál)
        /// </summary>
        [MaxLength(150)]
        public string? Utca { get; set; }

        /// <summary>
        /// Házszám (kötelező regisztrációnál)
        /// </summary>
        [MaxLength(20)]
        public string? Hazszam { get; set; }

        /// <summary>
        /// Telefonszám (opcionális)
        /// </summary>
        [MaxLength(20)]
        public string? Telefonszam { get; set; }

        /// <summary>
        /// Teljes cím összeállítva (pl. "6720 Szeged, Kossuth utca 12")
        /// </summary>
        [MaxLength(300)]
        public string? Cim { get; set; }

        // === KOMPATIBILITÁS - régi Nev mező eltávolítható ha nem kell ===
        // Ha a Foglalas-nál is használtad, akkor hagyd meg
        [MaxLength(100)]
        public string? Nev { get; set; }

        // === METAADATOK ===
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastLoginAt { get; set; }
    }
}