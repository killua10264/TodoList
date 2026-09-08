export interface LoginRequest {
  email?: string;
  usernameOrEmail?: string;
  username?: string;
  password: string;
}

export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
}

import { UserResponse } from './user.model';

export interface AuthResponse {
  accessToken: string;
  expiresAt: string;
  user?: UserResponse;
}
