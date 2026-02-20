// ============================================================================
// Features/Push/Controllers/PushController.cs
// ============================================================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using SzerszamKolcsonzo.Features.Push.DTOs;
using SzerszamKolcsonzo.Features.Push.Services;

namespace SzerszamKolcsonzo.Features.Push.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PushController : ControllerBase
    {
        private readonly PushNotificationService _pushService;
        private readonly VapidSettings _vapidSettings;
        private readonly ILogger<PushController> _logger;

        public PushController(
            PushNotificationService pushService,
            IOptions<VapidSettings> vapidSettings,
            ILogger<PushController> logger)
        {
            _pushService = pushService;
            _vapidSettings = vapidSettings.Value;
            _logger = logger;
        }

        /// <summary>
        /// GET: api/push/vapid-public-key
        /// VAPID public key lekérése (frontend-nek kell)
        /// </summary>
        [HttpGet("vapid-public-key")]
        public ActionResult<object> GetVapidPublicKey()
        {
            return Ok(new { publicKey = _vapidSettings.PublicKey });
        }

        /// <summary>
        /// POST: api/push/subscribe
        /// Feliratkozás push értesítésekre
        /// </summary>
        [HttpPost("subscribe")]
        public async Task<ActionResult> Subscribe([FromBody] SubscribeRequestDto dto)
        {
            if (string.IsNullOrEmpty(dto.Endpoint))
                return BadRequest(new { message = "Endpoint megadása kötelező" });

            int? userId = null;
            bool isAdmin = false;
            string? email = dto.Email;

            if (User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out int parsedUserId))
                    userId = parsedUserId;

                email = User.FindFirst(ClaimTypes.Email)?.Value ?? dto.Email;
                isAdmin = User.IsInRole("Admin");
            }

            var userAgent = Request.Headers.UserAgent.ToString();

            try
            {
                var subscription = await _pushService.SubscribeAsync(dto, userId, isAdmin, userAgent);

                _logger.LogInformation("✅ Push subscription: {Email}, Admin: {IsAdmin}", email, isAdmin);

                return Ok(new
                {
                    message = "Sikeresen feliratkoztál az értesítésekre",
                    subscriptionId = subscription.Id,
                    isAdmin = isAdmin
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Subscribe hiba");
                return StatusCode(500, new { message = "Feliratkozás sikertelen" });
            }
        }

        /// <summary>
        /// POST: api/push/unsubscribe
        /// Leiratkozás
        /// </summary>
        [HttpPost("unsubscribe")]
        public async Task<ActionResult> Unsubscribe([FromBody] UnsubscribeRequestDto dto)
        {
            if (string.IsNullOrEmpty(dto.Endpoint))
                return BadRequest(new { message = "Endpoint megadása kötelező" });

            var result = await _pushService.UnsubscribeAsync(dto.Endpoint);

            if (result)
                return Ok(new { message = "Sikeresen leiratkoztál" });

            return NotFound(new { message = "Subscription nem található" });
        }

        /// <summary>
        /// POST: api/push/status
        /// Subscription státusz lekérése
        /// </summary>
        [HttpPost("status")]
        public async Task<ActionResult<SubscriptionStatusDto>> GetStatus([FromBody] UnsubscribeRequestDto dto)
        {
            if (string.IsNullOrEmpty(dto.Endpoint))
                return BadRequest(new { message = "Endpoint megadása kötelező" });

            var status = await _pushService.GetStatusAsync(dto.Endpoint);

            if (status == null)
            {
                return Ok(new SubscriptionStatusDto(
                    IsSubscribed: false,
                    IsAdmin: false,
                    Email: null,
                    CreatedAt: null
                ));
            }

            return Ok(status);
        }

        /// <summary>
        /// POST: api/push/send-test
        /// Teszt értesítés küldése (admin)
        /// </summary>
        [HttpPost("send-test")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> SendTestPush([FromBody] SendTestPushDto dto)
        {
            var payload = new PushPayloadDto
            {
                Title = "🧪 Teszt értesítés",
                Body = dto.Message ?? "Ez egy teszt értesítés a Szerszámkölcsönzőtől!",
                Tag = "test",
                Type = "test"
            };

            int sent;
            if (!string.IsNullOrEmpty(dto.Email))
            {
                sent = await _pushService.SendToEmailAsync(dto.Email, payload);
            }
            else
            {
                sent = await _pushService.SendToAdminsAsync(payload);
            }

            return Ok(new { message = $"Teszt értesítés elküldve {sent} eszközre" });
        }

        /// <summary>
        /// GET: api/push/stats
        /// Subscription statisztika (admin)
        /// </summary>
        [HttpGet("stats")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> GetStatistics()
        {
            var stats = await _pushService.GetStatisticsAsync();
            return Ok(stats);
        }
    }
}
