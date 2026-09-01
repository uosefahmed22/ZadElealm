import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { ApiDataResponseEnvelope, ApiResponseEnvelope } from '../api/api-error.models';
import { appEnvironment } from '../config/app-environment';
import {
  ChangePasswordRequest,
  SendEmailOtpRequest,
  UpdateEmailRequest,
  UpdateProfileRequest,
  UserProfileDto,
} from './account.models';

@Injectable({ providedIn: 'root' })
export class AccountApiService {
  private readonly http = inject(HttpClient);
  private readonly accountUrl = `${appEnvironment.apiBaseUrl}/Account`;

  getProfile(): Observable<ApiDataResponseEnvelope<UserProfileDto>> {
    return this.http.get<ApiDataResponseEnvelope<UserProfileDto>>(
      `${this.accountUrl}/get-User-Profile`,
    );
  }

  updateProfile(payload: UpdateProfileRequest): Observable<ApiResponseEnvelope> {
    return this.http.post<ApiResponseEnvelope>(`${this.accountUrl}/update-profile`, payload);
  }

  changePassword(payload: ChangePasswordRequest): Observable<ApiResponseEnvelope> {
    return this.http.post<ApiResponseEnvelope>(`${this.accountUrl}/change-password`, payload);
  }

  updateProfileImage(file: File): Observable<ApiResponseEnvelope> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<ApiResponseEnvelope>(`${this.accountUrl}/update-profile-image`, formData);
  }

  removeProfileImage(): Observable<ApiResponseEnvelope> {
    return this.http.post<ApiResponseEnvelope>(`${this.accountUrl}/update-profile-image`, null);
  }

  sendEmailOtp(payload: SendEmailOtpRequest): Observable<ApiResponseEnvelope> {
    return this.http.post<ApiResponseEnvelope>(`${this.accountUrl}/send-email-otp`, payload);
  }

  updateEmail(payload: UpdateEmailRequest): Observable<ApiResponseEnvelope> {
    return this.http.post<ApiResponseEnvelope>(`${this.accountUrl}/update-email`, payload);
  }

  deleteAccount(password: string): Observable<ApiResponseEnvelope> {
    return this.http.delete<ApiResponseEnvelope>(`${this.accountUrl}/delete-account`, {
      body: { password },
    });
  }
}
