using MediCare.Services.Dtos;
using MediCare.Services.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediCare.Services.Interfaces
{
    public interface INotificationService
    {
        Task<PagedResultDto<NotificationDto>> GetMyNotificationsAsync(int userId, bool unreadOnly, int page, int pageSize);
        Task<int> GetUnreadCountAsync(int userId);

        // Throws NotFoundException if the notification does not exist or to someone else
        Task<NotificationDto> MarkAsReadAsync(int userId, int notificationId);

        Task MarkAllAsReadAsync(int userId);

        // Used by the other modules (appointments, doctor approval, prescriptions) to notify a user, The message must not contain medical details
        Task CreateAsync(int userId, NotificationType type, string title, string message);
    }
}
