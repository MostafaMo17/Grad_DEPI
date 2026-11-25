using MediCare.API.Hubs;
using MediCare.Services.Dtos;
using MediCare.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace MediCare.API.Services
{
    // Pushes a notification to one user through SignalR.
    public class SignalRNotifier : IRealTimeNotifier
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<SignalRNotifier> _logger;

        public SignalRNotifier(IHubContext<NotificationHub> hubContext, ILogger<SignalRNotifier> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task SendAsync(int userId, NotificationDto notification)
        {
            try
            {
                // Only the connections of this user receive the message.
                await _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", notification);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not push a real-time notification to user {UserId}.", userId);
            }
        }
    }
}
