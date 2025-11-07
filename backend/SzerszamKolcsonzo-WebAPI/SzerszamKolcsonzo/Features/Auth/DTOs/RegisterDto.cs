// ============================================================================
// Features/Auth/DTOs/RegisterDto.cs
// ============================================================================

using System.ComponentModel.DataAnnotations;

namespace SzerszamKolcsonzo.Features.Auth.DTOs
{
    public record RegisterDto(
        [Required][EmailAddress] string Email,
        [Required][MinLength(6)] string Password
    );
}

