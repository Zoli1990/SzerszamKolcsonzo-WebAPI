// ============================================================================
// Features/Auth/DTOs/AuthResponseDto.cs - FRISSÍTETT (kibővített cím mezőkkel)
// ============================================================================

namespace SzerszamKolcsonzo.Features.Auth.DTOs
{
    public record AuthResponseDto(
        string Token,
        string Email,
        string Role,
        DateTime ExpiresAt,

        // === KIBŐVÍTETT CÍM MEZŐK ===
        string? Iranyitoszam,
        string? Telepules,
        string? Utca,
        string? Hazszam,
        string? Telefonszam,
        string? Cim
    );
}