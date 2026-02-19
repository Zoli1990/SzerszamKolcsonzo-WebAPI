// ============================================================================
// Features/Auth/Controllers/AuthController.cs - FRISSÍTETT (Profil kezelés)
// ============================================================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SzerszamKolcsonzo.Features.Auth.DTOs;
using SzerszamKolcsonzo.Features.Auth.Services;
using System.Security.Claims;

namespace SzerszamKolcsonzo.Features.Auth.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
        {
            try
            {
                var response = await _authService.LoginAsync(dto);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto)
        {
            try
            {
                var response = await _authService.RegisterAsync(dto);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/auth/profile - Profil lekérése
        [HttpGet("profile")]
        [Authorize]
        public async Task<ActionResult<ProfileDto>> GetProfile()
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null)
                {
                    return Unauthorized(new { message = "Érvénytelen token." });
                }

                var profile = await _authService.GetProfileAsync(userId.Value);
                return Ok(profile);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/auth/profile - Profil frissítése
        [HttpPut("profile")]
        [Authorize]
        public async Task<ActionResult<ProfileDto>> UpdateProfile(UpdateProfileDto dto)
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null)
                {
                    return Unauthorized(new { message = "Érvénytelen token." });
                }

                var profile = await _authService.UpdateProfileAsync(userId.Value, dto);
                return Ok(profile);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Helper: User ID kinyerése a tokenbõl
        private int? GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                ?? User.FindFirst("sub");

            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return null;
        }
    }
}