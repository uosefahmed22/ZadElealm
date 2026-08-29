import { ApiDataResponseEnvelope, ApiResponseEnvelope } from '../api/api-error.models';

export interface UserDto {
  displayName: string;
  email: string;
  token: string;
  refreshToken: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  displayName: string;
  email: string;
  password: string;
}

export interface ResetPasswordRequest {
  email: string;
  newPassword: string;
  confirmPassword: string;
}

export interface TokenRequest {
  token: string;
  refreshToken: string;
}

export type LoginResponse = ApiDataResponseEnvelope<UserDto>;
export type RegisterResponse = ApiResponseEnvelope;
export type AuthMessageResponse = ApiResponseEnvelope;
