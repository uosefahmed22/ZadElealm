export type NotificationType = 0 | 1 | 2 | 3 | 4 | 5 | 6;

export interface UserNotificationDto {
  id: number;
  title: string;
  description: string;
  type: NotificationType;
  createdAt: string;
  isRead: boolean;
}

export interface NotificationListDto {
  notifications: UserNotificationDto[];
  unreadCount: number;
  totalCount: number;
}
