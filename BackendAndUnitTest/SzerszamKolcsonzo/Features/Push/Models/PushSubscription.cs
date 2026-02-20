// ============================================================================
// Features/Push/Models/PushSubscription.cs
// ============================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SzerszamKolcsonzo.Features.Push.Models
{
    public class PushSubscription
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Felhasználó ID (null = vendég)
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// Email cím azonosításhoz
        /// </summary>
        [MaxLength(150)]
        public string? Email { get; set; }

        /// <summary>
        /// Push subscription endpoint URL
        /// </summary>
        [Required]
        public string Endpoint { get; set; } = string.Empty;

        /// <summary>
        /// P256DH kulcs (Base64)
        /// </summary>
        [Required]
        public string P256dh { get; set; } = string.Empty;

        /// <summary>
        /// Auth kulcs (Base64)
        /// </summary>
        [Required]
        public string Auth { get; set; } = string.Empty;

        /// <summary>
        /// Létrehozás dátuma
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Utolsó sikeres push
        /// </summary>
        public DateTime? LastPushAt { get; set; }

        /// <summary>
        /// Admin-e (minden admin értesítést kap)
        /// </summary>
        public bool IsAdmin { get; set; } = false;

        /// <summary>
        /// Aktív-e
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// User agent (debug)
        /// </summary>
        [MaxLength(500)]
        public string? UserAgent { get; set; }
    }
}
