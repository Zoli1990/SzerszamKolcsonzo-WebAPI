// ============================================================================
// Hubs/EszkozHub.cs — SignalR Hub eszköz státusz változásokhoz
// ============================================================================

using Microsoft.AspNetCore.SignalR;

namespace SzerszamKolcsonzo.Hubs
{
    public class EszkozHub : Hub
    {
        private readonly ILogger<EszkozHub> _logger;

        public EszkozHub(ILogger<EszkozHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation($"[SignalR] Kliens csatlakozott: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation($"[SignalR] Kliens lecsatlakozott: {Context.ConnectionId}");
            await base.OnDisconnectedAsync(exception);
        }
    }
}