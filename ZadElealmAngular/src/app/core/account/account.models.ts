export interface UserProfileDto {
  id: string;
  displayName: string;
  email: string;
  imageUrl: string | null;
  userName: string;
  phoneNumber: string | null;
}

export interface UpdateProfileRequest {
  displayName: string;
  phoneNumber: string | null;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}

export interface SendEmailOtpRequest {
  newEmail: string;
  password: string;
}

export interface UpdateEmailRequest {
  newEmail: string;
  token: string;
}
