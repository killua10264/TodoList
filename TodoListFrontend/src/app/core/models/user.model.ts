export interface UserResponse {
  id: number;
  username: string;
  email: string;
  avatarUrl?: string;
  displayName?: string;
  bio?: string;
  timezone?: string;
  theme?: string;
  language?: string;
  firstDayOfWeek?: string;
  createdAt: string;
}

export interface UserUpdateRequest {
  username?: string;
  email?: string;
  avatarUrl?: string;
  displayName?: string;
  bio?: string;
  timezone?: string;
  theme?: string;
  language?: string;
  firstDayOfWeek?: string;
}

export interface ChangePasswordRequest {
  oldPassword: string;
  newPassword: string;
  confirmNewPassword: string;
}
