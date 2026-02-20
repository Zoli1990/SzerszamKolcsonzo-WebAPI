// ============================================================================
// Features/Push/DTOs/PushDtos.cs
// ============================================================================

namespace SzerszamKolcsonzo.Features.Push.DTOs
{
    public record SubscribeRequestDto(
        string Endpoint,
        string P256dh,
        string Auth,
        string? Email = null
    );

    public record UnsubscribeRequestDto(string Endpoint);

    public record SubscriptionStatusDto(
        bool IsSubscribed,
        bool IsAdmin,
        string? Email,
        DateTime? CreatedAt
    );

    public record PushPayloadDto
    {
        public string Title { get; init; } = "Szerszámkölcsönző";
        public string Body { get; init; } = "";
        public string? Icon { get; init; } = "/icons/icon-192x192.png";
        public string? Badge { get; init; } = "/icons/badge-72x72.png";
        public string? Tag { get; init; }
        public string? Type { get; init; }
        public bool RequireInteraction { get; init; } = false;
        public Dictionary<string, object>? Data { get; init; }
        public List<NotificationAction>? Actions { get; init; }
    }

    public record NotificationAction(
        string Action,
        string Title,
        string? Icon = null
    );

    public record SendTestPushDto(
        string? Email = null,
        string? Message = null
    );
}
