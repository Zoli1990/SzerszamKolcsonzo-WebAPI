// ============================================================================
// Features/Auth/DTOs/ProfileDto.cs - BŐVÍTETT VERZIÓ
// ============================================================================

using System.ComponentModel.DataAnnotations;

namespace SzerszamKolcsonzo.Features.Auth.DTOs
{
    // Profil lekérésekor ezt adjuk vissza
    public record ProfileDto(
        string Email,
        string? Nev,
        string? Telefonszam,
        string? Iranyitoszam,
        string? Telepules,
        string? Utca,
        string? Hazszam,
        string? Cim
    );

    // Profil frissítésekor ezt fogadjuk
    public record UpdateProfileDto(
        [MaxLength(100)] string? Nev,
        [MaxLength(20)] string? Telefonszam,
        [MaxLength(4)] string? Iranyitoszam,
        [MaxLength(100)] string? Telepules,
        [MaxLength(150)] string? Utca,
        [MaxLength(20)] string? Hazszam,
        [MaxLength(200)] string? Cim
    );
}