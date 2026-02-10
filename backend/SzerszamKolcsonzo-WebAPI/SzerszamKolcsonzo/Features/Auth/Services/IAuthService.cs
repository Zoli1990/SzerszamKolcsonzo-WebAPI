// ============================================================================
// Features/Auth/Services/IAuthService.cs - FRISSÍTETT (Profil kezelés)
// ============================================================================

using SzerszamKolcsonzo.Features.Auth.DTOs;

namespace SzerszamKolcsonzo.Features.Auth.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
        Task<ProfileDto> GetProfileAsync(int userId);
        Task<ProfileDto> UpdateProfileAsync(int userId, UpdateProfileDto dto);
    }
}