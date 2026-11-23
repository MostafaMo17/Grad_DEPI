using MediCare.Services.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediCare.Services.Interfaces
{
    public interface INotificationRepository
    {
        Task AddAsync(Notification notification);

        // One page of the user notifications and newest first
        Task<List<Notification>> GetListAsync(int userId, bool unreadOnly, int page, int pageSize);

        Task<int> CountAsync(int userId, bool unreadOnly);

        // Return the notification for this user,otherwise null
        Task<Notification?> GetByIdAsync(int notificationId, int userId);

        Task SaveChangesAsync();

        Task MarkAllAsReadAsync(int userId);
    }
}
