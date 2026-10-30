export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
  fullName?: string;
}

export interface RegisterResponse {
  userId: number;
  username: string;
  email: string;
  fullName?: string;
  role: string;
}

export interface LoginRequest {
  usernameOrEmail: string;
  password: string;
}

export interface LoginResponse {
  userId: number;
  username: string;
  email: string;
  fullName?: string;
  role: string;
}

export interface GoogleAuthRequest {
  idToken: string;
  username?: string;
}

export interface GoogleAuthResponse {
  requiresUsername: boolean;
  userId?: number;
  username: string;
  email: string;
  fullName?: string;
  role: string;
}

export interface ForgotPasswordRequest {
  usernameOrEmail: string;
}

export interface ForgotPasswordResponse {
  message: string;
}

export interface VerifyResetCodeRequest {
  usernameOrEmail: string;
  code: string;
}

export interface VerifyResetCodeResponse {
  message: string;
  resetToken: string;
}

export interface ResetPasswordRequest {
  usernameOrEmail: string;
  code: string;
  password: string;
}

export interface ResetPasswordResponse {
  message: string;
}

export interface ApiErrorResponse {
  message: string;
  code: string;
}
