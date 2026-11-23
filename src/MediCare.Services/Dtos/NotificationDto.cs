using System;


namespace MediCare.Services.Dtos
{
    public class NotificationDto
    {
        public int NotificationId { get; set; }

        // The event name (AppointmentBooked) 
        public string Type { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }

        // UTC time, The client shows it in Cairo time
        public DateTime CreatedAtUtc { get; set; }
    }
}
