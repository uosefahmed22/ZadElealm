import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { appEnvironment } from '../config/app-environment';
import { ApiDataResponseEnvelope } from '../api/api-error.models';
import {
  AuthMessageResponse,
  LoginRequest,
  LoginResponse,
  RegisterRequest,
  RegisterResponse,
  ResetPasswordRequest,
  TokenRequest,
  UserDto
} from './auth.models';

@Injectable({ providedIn: 'root' })
export class AuthApiService {
  private readonly http = inject(HttpClient);
  private readonly accountUrl = `${appEnvironment.apiBaseUrl}/Account`;

  login(payload: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.accountUrl}/login`, payload);
  }

  register(payload: RegisterRequest): Observable<RegisterResponse> {
    return this.http.post<RegisterResponse>(`${this.accountUrl}/register`, payload);
  }

  resendConfirmationEmail(email: string): Observable<AuthMessageResponse> {
    const params = new HttpParams().set('email', email);
    return this.http.post<AuthMessageResponse>(`${this.accountUrl}/resend-confirmation-email`, null, {
      params
    });
  }

  forgotPassword(email: string): Observable<AuthMessageResponse> {
    const params = new HttpParams().set('email', email);
    return this.http.post<AuthMessageResponse>(`${this.accountUrl}/forget-password`, null, {
      params
    });
  }

  verifyOtp(email: string, otp: string): Observable<AuthMessageResponse> {
    const params = new HttpParams().set('email', email).set('otp', otp);
    return this.http.post<AuthMessageResponse>(`${this.accountUrl}/verify-otp`, null, {
      params
    });
  }

  resetPassword(payload: ResetPasswordRequest): Observable<AuthMessageResponse> {
    return this.http.post<AuthMessageResponse>(`${this.accountUrl}/reset-password`, payload);
  }

  refreshToken(payload: TokenRequest): Observable<ApiDataResponseEnvelope<UserDto>> {
    return this.http.post<ApiDataResponseEnvelope<UserDto>>(`${this.accountUrl}/refresh-token`, payload);
  }

  revokeToken(payload: TokenRequest): Observable<AuthMessageResponse> {
    return this.http.post<AuthMessageResponse>(`${this.accountUrl}/revoke-token`, payload);
  }
}
