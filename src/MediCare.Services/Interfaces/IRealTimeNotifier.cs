using MediCare.Services.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediCare.Services.Interfaces
{
    // Pushes a notification to the user open browser tabs right now
    //implementation (SignalR) in the API
    public interface IRealTimeNotifier
    {
        Task SendAsync(int userId, NotificationDto notification);
    }
}
