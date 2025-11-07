// ============================================================================
// Features/Auth/Services/IAuthService.cs
// ============================================================================

using SzerszamKolcsonzo.Features.Auth.DTOs;

namespace SzerszamKolcsonzo.Features.Auth.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
    }
}
