// ============================================================================
// Features/Push/Services/VapidSettings.cs
// ============================================================================

namespace SzerszamKolcsonzo.Features.Push.Services
{
    /// <summary>
    /// VAPID konfiguráció appsettings.json-ból
    /// </summary>
    public class VapidSettings
    {
        public const string SectionName = "Vapid";
        
        /// <summary>
        /// mailto: vagy https:// URL
        /// </summary>
        public string Subject { get; set; } = "";
        
        /// <summary>
        /// VAPID Public Key (Base64)
        /// </summary>
        public string PublicKey { get; set; } = "";
        
        /// <summary>
        /// VAPID Private Key (Base64) - TITKOS!
        /// </summary>
        public string PrivateKey { get; set; } = "";
    }
}
