// ============================================================================
// Features/Auth/DTOs/LoginDto.cs
// ============================================================================

using System.ComponentModel.DataAnnotations;

namespace SzerszamKolcsonzo.Features.Auth.DTOs
{
    public record LoginDto(
        [Required][EmailAddress] string Email,
        [Required][MinLength(6)] string Password
    );
}