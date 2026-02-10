// ============================================================================
// Features/Auth/DTOs/AuthResponseDto.cs - FRISSÍTETT (kibővített cím mezőkkel)
// ============================================================================


namespace SzerszamKolcsonzo.Features.Auth.DTOs
{
    public interface IAuthResponseDto
    {
        string? Cim { get; init; }
        string Email { get; init; }
        DateTime ExpiresAt { get; init; }
        string? Hazszam { get; init; }
        string? Iranyitoszam { get; init; }
        string Role { get; init; }
        string? Telefonszam { get; init; }
        string? Telepules { get; init; }
        string Token { get; init; }
        string? Utca { get; init; }

        void Deconstruct(out string Token, out string Email, out string Role, out DateTime ExpiresAt, out string? Iranyitoszam, out string? Telepules, out string? Utca, out string? Hazszam, out string? Telefonszam, out string? Cim);
        bool Equals(AuthResponseDto? other);
        bool Equals(object? obj);
        int GetHashCode();
        string ToString();
    }
}