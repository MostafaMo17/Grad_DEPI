using MediCare.Services.Dtos;
using MediCare.Services.Entities;
using MediCare.Services.Enums;
using MediCare.Services.Exceptions;
using MediCare.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediCare.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _repository;
        private readonly IRealTimeNotifier _realTimeNotifier;

        public NotificationService(INotificationRepository repository, IRealTimeNotifier realTimeNotifier)
        {
            _repository = repository;
            _realTimeNotifier = realTimeNotifier;
        }

        public async Task<PagedResultDto<NotificationDto>> GetMyNotificationsAsync(
            int userId, bool unreadOnly, int page, int pageSize)
        {
            // paging values in a safe range instead of failing.
            if (page < 1)
            {
                page = 1;
            }

            if (pageSize < 1)
            {
                pageSize = 20;
            }

            if (pageSize > 100)
            {
                pageSize = 100;
            }

            List<Notification> notifications = await _repository.GetListAsync(userId, unreadOnly, page, pageSize);
            int totalCount = await _repository.CountAsync(userId, unreadOnly);

            PagedResultDto<NotificationDto> result = new PagedResultDto<NotificationDto>();
            result.Items = notifications.Select(n => ToDto(n)).ToList();
            result.Page = page;
            result.PageSize = pageSize;
            result.TotalCount = totalCount;
            return result;
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _repository.CountAsync(userId, true);
        }

        public async Task<NotificationDto> MarkAsReadAsync(int userId, int notificationId)
        {
            // A notification of another user is treated as not found
            Notification? notification = await _repository.GetByIdAsync(notificationId, userId);
            if (notification == null)
            {
                throw new NotFoundException("Notification not found.");
            }

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                await _repository.SaveChangesAsync();
            }

            return ToDto(notification);
        }

        public async Task MarkAllAsReadAsync(int userId)
        {
            await _repository.MarkAllAsReadAsync(userId);
        }

        public async Task CreateAsync(int userId, NotificationType type, string title, string message)
        {
            Notification notification = new Notification();
            notification.UserId = userId;
            notification.Type = type;
            notification.Title = title;
            notification.Message = message;

            // الاول هنعملها save , so it is never lost
            await _repository.AddAsync(notification);

            // Then push it live
            await _realTimeNotifier.SendAsync(userId, ToDto(notification));
        }

        private NotificationDto ToDto(Notification notification)
        {
            NotificationDto dto = new NotificationDto();
            dto.NotificationId = notification.NotificationId;
            dto.Type = notification.Type.ToString();
            dto.Title = notification.Title;
            dto.Message = notification.Message;
            dto.IsRead = notification.IsRead;

            dto.CreatedAtUtc = notification.CreatedAt;
            return dto;
        }
    }
}
