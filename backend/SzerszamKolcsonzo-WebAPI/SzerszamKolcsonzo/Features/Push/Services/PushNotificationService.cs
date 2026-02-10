// ============================================================================
// Features/Push/Services/PushNotificationService.cs
// ============================================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json;
using WebPush;
using SzerszamKolcsonzo.Features.Push.Models;
using SzerszamKolcsonzo.Features.Push.DTOs;
using SzerszamKolcsonzo.Data;

namespace SzerszamKolcsonzo.Features.Push.Services
{
    public class PushNotificationService
    {
        private readonly AppDbContext _context;
        private readonly VapidSettings _vapidSettings;
        private readonly ILogger<PushNotificationService> _logger;
        private readonly WebPushClient _webPushClient;

        public PushNotificationService(
            AppDbContext context,
            IOptions<VapidSettings> vapidSettings,
            ILogger<PushNotificationService> logger)
        {
            _context = context;
            _vapidSettings = vapidSettings.Value;
            _logger = logger;
            _webPushClient = new WebPushClient();
        }

        // ============================================================================
        // SUBSCRIPTION KEZELÉS
        // ============================================================================

        public async Task<Models.PushSubscription> SubscribeAsync(
            SubscribeRequestDto dto,
            int? userId = null,
            bool isAdmin = false,
            string? userAgent = null)
        {
            var existing = await _context.PushSubscriptions
                .FirstOrDefaultAsync(s => s.Endpoint == dto.Endpoint);

            if (existing != null)
            {
                existing.P256dh = dto.P256dh;
                existing.Auth = dto.Auth;
                existing.Email = dto.Email;
                existing.UserId = userId;
                existing.IsAdmin = isAdmin;
                existing.IsActive = true;
                existing.UserAgent = userAgent;

                _logger.LogInformation("📝 Push subscription frissítve: {Email}", dto.Email);
            }
            else
            {
                existing = new Models.PushSubscription
                {
                    Endpoint = dto.Endpoint,
                    P256dh = dto.P256dh,
                    Auth = dto.Auth,
                    Email = dto.Email,
                    UserId = userId,
                    IsAdmin = isAdmin,
                    IsActive = true,
                    UserAgent = userAgent,
                    CreatedAt = DateTime.UtcNow
                };

                _context.PushSubscriptions.Add(existing);
                _logger.LogInformation("✅ Új push subscription: {Email}", dto.Email);
            }

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> UnsubscribeAsync(string endpoint)
        {
            var subscription = await _context.PushSubscriptions
                .FirstOrDefaultAsync(s => s.Endpoint == endpoint);

            if (subscription == null)
                return false;

            subscription.IsActive = false;
            await _context.SaveChangesAsync();

            _logger.LogInformation("🔕 Leiratkozott");
            return true;
        }

        public async Task<SubscriptionStatusDto?> GetStatusAsync(string endpoint)
        {
            var subscription = await _context.PushSubscriptions
                .FirstOrDefaultAsync(s => s.Endpoint == endpoint && s.IsActive);

            if (subscription == null)
                return null;

            return new SubscriptionStatusDto(
                IsSubscribed: true,
                IsAdmin: subscription.IsAdmin,
                Email: subscription.Email,
                CreatedAt: subscription.CreatedAt
            );
        }

        // ============================================================================
        // PUSH KÜLDÉS
        // ============================================================================

        public async Task<bool> SendPushAsync(Models.PushSubscription subscription, PushPayloadDto payload)
        {
            if (!subscription.IsActive)
                return false;

            try
            {
                var webPushSubscription = new WebPush.PushSubscription(
                    subscription.Endpoint,
                    subscription.P256dh,
                    subscription.Auth
                );

                var vapidDetails = new VapidDetails(
                    _vapidSettings.Subject,
                    _vapidSettings.PublicKey,
                    _vapidSettings.PrivateKey
                );

                var payloadJson = JsonSerializer.Serialize(payload, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                await _webPushClient.SendNotificationAsync(
                    webPushSubscription,
                    payloadJson,
                    vapidDetails
                );

                subscription.LastPushAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("📤 Push elküldve: {Title}", payload.Title);
                return true;
            }
            catch (WebPushException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Gone)
            {
                subscription.IsActive = false;
                await _context.SaveChangesAsync();
                _logger.LogWarning("⚠️ Subscription expired, inaktiválva");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Push küldés sikertelen");
                return false;
            }
        }

        public async Task<int> SendToEmailAsync(string email, PushPayloadDto payload)
        {
            var subscriptions = await _context.PushSubscriptions
                .Where(s => s.Email == email && s.IsActive)
                .ToListAsync();

            int successCount = 0;
            foreach (var sub in subscriptions)
            {
                if (await SendPushAsync(sub, payload))
                    successCount++;
            }

            return successCount;
        }

        public async Task<int> SendToAdminsAsync(PushPayloadDto payload)
        {
            var adminSubscriptions = await _context.PushSubscriptions
                .Where(s => s.IsAdmin && s.IsActive)
                .ToListAsync();

            int successCount = 0;
            foreach (var sub in adminSubscriptions)
            {
                if (await SendPushAsync(sub, payload))
                    successCount++;
            }

            _logger.LogInformation("📤 Admin push küldve: {Success}/{Total}",
                successCount, adminSubscriptions.Count);
            return successCount;
        }

        // ============================================================================
        // SPECIFIKUS ÉRTESÍTÉSEK
        // ============================================================================

        /// <summary>
        /// Új foglalás értesítés (adminoknak)
        /// </summary>
        public async Task NotifyNewBookingAsync(int foglalasId, string eszkozNev, string ugyfelNev)
        {
            var payload = new PushPayloadDto
            {
                Title = "🆕 Új foglalás érkezett!",
                Body = $"{eszkozNev} - {ugyfelNev}",
                Tag = $"foglalas-{foglalasId}",
                Type = "uj_foglalas",
                RequireInteraction = true,
                Data = new Dictionary<string, object>
        {
            { "foglalasId", foglalasId },
            { "url", "https://szerszamkolcsonzo.tryasp.net/#/admin?tab=foglalasok" }  // ← JAVÍTVA!
        },
                Actions = new List<NotificationAction>
        {
            new("view", "👁️ Megtekintés"),
            new("dismiss", "❌ Bezárás")
        }
            };

            await SendToAdminsAsync(payload);
        }

        /// <summary>
        /// Foglalás kiadva értesítés (ügyfélnek)
        /// </summary>
        public async Task NotifyBookingIssuedAsync(int foglalasId, string email, string eszkozNev)
        {
            var payload = new PushPayloadDto
            {
                Title = "✅ Eszköz kiadva!",
                Body = $"{eszkozNev} - Az óra elkezdődött",
                Tag = $"kiadva-{foglalasId}",
                Type = "foglalas_kiadva",
                Data = new Dictionary<string, object>
                {
                    { "foglalasId", foglalasId }
                }
            };

            await SendToEmailAsync(email, payload);
        }

        /// <summary>
        /// Foglalás lezárva értesítés (ügyfélnek)
        /// </summary>
        public async Task NotifyBookingClosedAsync(
            int foglalasId,
            string email,
            string eszkozNev,
            int percek,
            decimal osszeg)
        {
            var orak = percek / 60;
            var maradekPerc = percek % 60;

            var payload = new PushPayloadDto
            {
                Title = "🏁 Foglalás lezárva",
                Body = $"{eszkozNev}\n⏱️ {orak}h {maradekPerc}m\n💰 {osszeg:N0} Ft",
                Tag = $"lezarva-{foglalasId}",
                Type = "foglalas_lezarva",
                Data = new Dictionary<string, object>
                {
                    { "foglalasId", foglalasId },
                    { "osszeg", osszeg }
                }
            };

            await SendToEmailAsync(email, payload);
        }

        /// <summary>
        /// Foglalás törölve (nem jelent meg 15 percen belül)
        /// </summary>
        public async Task NotifyBookingDeletedAsync(int foglalasId, string email, string eszkozNev)
        {
            // Ügyfélnek
            var userPayload = new PushPayloadDto
            {
                Title = "❌ Foglalás törölve",
                Body = $"{eszkozNev} - Nem jelent meg 15 percen belül",
                Tag = $"torolve-{foglalasId}",
                Type = "foglalas_torolt"
            };
            await SendToEmailAsync(email, userPayload);

            // Adminoknak
            var adminPayload = new PushPayloadDto
            {
                Title = "🗑️ Foglalás automatikusan törölve",
                Body = $"{eszkozNev} - Ügyfél nem jelent meg",
                Tag = $"admin-torolve-{foglalasId}",
                Type = "foglalas_lejart"
            };
            await SendToAdminsAsync(adminPayload);
        }

        /// <summary>
        /// Statisztika
        /// </summary>
        public async Task<object> GetStatisticsAsync()
        {
            var total = await _context.PushSubscriptions.CountAsync();
            var active = await _context.PushSubscriptions.CountAsync(s => s.IsActive);
            var admins = await _context.PushSubscriptions.CountAsync(s => s.IsAdmin && s.IsActive);

            return new
            {
                Total = total,
                Active = active,
                Admins = admins
            };
        }

        internal Task SendNewBookingNotification(object foglalas)   //remove async
        {
            throw new NotImplementedException();
        }
    }
}
