using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace MediCare.API.Hubs
{

    // only logged in users can connect , SignalR finds the user by the NameIdentifier claim
    // مفيش ميثود لان الكلاينت مش هيبعت حاجة , بيستقبل بس 
    [Authorize]
    public class NotificationHub : Hub
    {
    }
}
