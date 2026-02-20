// ============================================================================
// Features/Auth/Services/AuthService.cs - JAVÍTOTT VERZIÓ
// ============================================================================

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SzerszamKolcsonzo.Features.Auth.Data;
using SzerszamKolcsonzo.Features.Auth.DTOs;
using SzerszamKolcsonzo.Features.Auth.Models;

namespace SzerszamKolcsonzo.Features.Auth.Services
{
    public class AuthService : IAuthService
    {
        private readonly AuthDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(AuthDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // ==========================================================================================
        // LOGIN
        // ==========================================================================================

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Hibás email vagy jelszó.");

            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(user);
            var expiresAt = DateTime.UtcNow.AddMinutes(GetJwtExpirationMinutes());

            return new AuthResponseDto(
                token,
                user.Email,
                user.Role,
                expiresAt,
                user.Iranyitoszam,
                user.Telepules,
                user.Utca,
                user.Hazszam,
                user.Telefonszam,
                user.Cim
            );
        }

        // ==========================================================================================
        // REGISZTRÁCIÓ
        // ==========================================================================================

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                throw new InvalidOperationException("Ez az email cím már regisztrálva van.");

            var cimOsszeallitva =
                $"{dto.Iranyitoszam} {dto.Telepules}, {dto.Utca} {dto.Hazszam}".Trim();

            var user = new User
            {
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = "User",
                Iranyitoszam = dto.Iranyitoszam,
                Telepules = dto.Telepules,
                Utca = dto.Utca,
                Hazszam = dto.Hazszam,
                Telefonszam = dto.Telefonszam,
                Cim = dto.Cim ?? cimOsszeallitva,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(user);
            var expiresAt = DateTime.UtcNow.AddMinutes(GetJwtExpirationMinutes());

            return new AuthResponseDto(
                token,
                user.Email,
                user.Role,
                expiresAt,
                user.Iranyitoszam,
                user.Telepules,
                user.Utca,
                user.Hazszam,
                user.Telefonszam,
                user.Cim
            );
        }

        // ==========================================================================================
        // PROFIL LEKÉRÉSE
        // ==========================================================================================

        public async Task<ProfileDto> GetProfileAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                throw new InvalidOperationException("Felhasználó nem található.");

            return new ProfileDto(
                user.Email,
                user.Nev,
                user.Telefonszam,
                user.Iranyitoszam,
                user.Telepules,
                user.Utca,
                user.Hazszam,
                user.Cim
            );
        }

        // ==========================================================================================
        // PROFIL FRISSÍTÉSE
        // ==========================================================================================

        public async Task<ProfileDto> UpdateProfileAsync(int userId, UpdateProfileDto dto)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                throw new InvalidOperationException("Felhasználó nem található.");

            // Összes mező frissítése
            user.Nev = dto.Nev;
            user.Telefonszam = dto.Telefonszam;
            user.Iranyitoszam = dto.Iranyitoszam;
            user.Telepules = dto.Telepules;
            user.Utca = dto.Utca;
            user.Hazszam = dto.Hazszam;
            user.Cim = dto.Cim;

            await _context.SaveChangesAsync();

            return new ProfileDto(
                user.Email,
                user.Nev,
                user.Telefonszam,
                user.Iranyitoszam,
                user.Telepules,
                user.Utca,
                user.Hazszam,
                user.Cim
            );
        }

        // ==========================================================================================
        // JWT TOKEN GENERÁLÁS
        // ==========================================================================================

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey hiányzik!");
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(GetJwtExpirationMinutes()),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private int GetJwtExpirationMinutes()
        {
            var minutes = _configuration.GetValue<int>("Jwt:ExpirationMinutes");
            return minutes > 0 ? minutes : 60;
        }
    }
}