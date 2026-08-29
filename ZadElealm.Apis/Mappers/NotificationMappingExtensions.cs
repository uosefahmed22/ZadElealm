using System;
using System.Collections.Generic;
using System.Linq;
using ZadElealm.Apis.Dtos;
using ZadElealm.Core.Models;

namespace ZadElealm.Apis.Mappers
{
    public static class NotificationMappingExtensions
    {
        public static NotificationDto ToDto(this Notification notification, bool isRead = false)
        {
            ArgumentNullException.ThrowIfNull(notification);

            return new NotificationDto
            {
                Id = notification.Id,
                Title = notification.Title,
                Description = notification.Description,
                Type = notification.Type,
                CreatedAt = notification.CreatedAt,
                IsRead = isRead
            };
        }

        public static NotificationDto ToDto(this UserNotification userNotification)
        {
            ArgumentNullException.ThrowIfNull(userNotification);

            if (userNotification.Notification is null)
            {
                return new NotificationDto
                {
                    Id = userNotification.NotificationId,
                    IsRead = userNotification.IsRead
                };
            }

            return userNotification.Notification.ToDto(userNotification.IsRead);
        }

        public static IReadOnlyList<NotificationDto> ToDtos(this IEnumerable<UserNotification> userNotifications)
        {
            if (userNotifications is null) return [];
            return userNotifications.Select(un => un.ToDto()).ToList();
        }
    }
}
