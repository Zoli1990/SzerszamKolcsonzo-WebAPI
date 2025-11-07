// ============================================================================
// Features/Auth/DTOs/AuthResponseDto.cs
// ============================================================================

namespace SzerszamKolcsonzo.Features.Auth.DTOs
{
    public record AuthResponseDto(
        string Token,
        string Email,
        string Role,
        DateTime ExpiresAt
    );
}