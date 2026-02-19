// ============================================================================
// Features/Auth/DTOs/RegisterDto.cs - FRISSÍTETT (kibővített cím mezőkkel)
// ============================================================================

using System.ComponentModel.DataAnnotations;

namespace SzerszamKolcsonzo.Features.Auth.DTOs
{
    public record RegisterDto(
        [Required(ErrorMessage = "Az email cím megadása kötelező")]
        [EmailAddress(ErrorMessage = "Érvénytelen email cím formátum")]
        string Email,

        [Required(ErrorMessage = "A jelszó megadása kötelező")]
        [MinLength(6, ErrorMessage = "A jelszónak legalább 6 karakter hosszúnak kell lennie")]
        string Password,

        // === KIBŐVÍTETT CÍM MEZŐK ===

        [Required(ErrorMessage = "Az irányítószám megadása kötelező")]
        [RegularExpression(@"^[1-9]\d{3}$", ErrorMessage = "Az irányítószám 4 számjegyű kell legyen (pl. 6720)")]
        string Iranyitoszam,

        /// <summary>
        /// Település - automatikusan kitöltődik frontend-en az irányítószám alapján
        /// </summary>
        string? Telepules,

        [Required(ErrorMessage = "Az utca megadása kötelező")]
        [MaxLength(150, ErrorMessage = "Az utca neve maximum 150 karakter lehet")]
        string Utca,

        [Required(ErrorMessage = "A házszám megadása kötelező")]
        [MaxLength(20, ErrorMessage = "A házszám maximum 20 karakter lehet")]
        string Hazszam,

        /// <summary>
        /// Telefonszám - opcionális
        /// </summary>
        [MaxLength(20, ErrorMessage = "A telefonszám maximum 20 karakter lehet")]
        string? Telefonszam,

        /// <summary>
        /// Teljes cím - opcionális, összeállítható a többi mezőből
        /// </summary>
        string? Cim
    );
}