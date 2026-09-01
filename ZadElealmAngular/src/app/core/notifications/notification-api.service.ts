import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { ApiDataResponseEnvelope, ApiResponseEnvelope } from '../api/api-error.models';
import { appEnvironment } from '../config/app-environment';
import { NotificationListDto } from './notification.models';

@Injectable({ providedIn: 'root' })
export class NotificationApiService {
  private readonly http = inject(HttpClient);
  private readonly api = appEnvironment.apiBaseUrl;

  getNotifications(): Observable<ApiDataResponseEnvelope<NotificationListDto>> {
    return this.http.get<ApiDataResponseEnvelope<NotificationListDto>>(`${this.api}/Notification`);
  }

  markAsRead(notificationId: number): Observable<ApiResponseEnvelope> {
    return this.http.post<ApiResponseEnvelope>(
      `${this.api}/Notification/mark-as-read/${notificationId}`,
      null,
    );
  }

  markAllAsRead(): Observable<ApiResponseEnvelope> {
    return this.http.post<ApiResponseEnvelope>(`${this.api}/Notification/mark-all-as-read`, null);
  }

  deleteNotification(notificationId: number): Observable<ApiResponseEnvelope> {
    return this.http.delete<ApiResponseEnvelope>(`${this.api}/Notification/${notificationId}`);
  }
}
