using System;
using System.Collections.Generic;
using System.Text;

namespace MediCare.Services.Dtos
{
    // short lived token the browser uses to connect to the notification hub
    public class HubTokenDto
    {
        public string Token { get; set; } = string.Empty;

        // token lives 5 minutes, The connection ask for a new one after that
        public DateTime ExpiresAtUtc { get; set; }
    }
}
