import { createApiClient } from '../../shared/apiClient';
import type {
  ForgotPasswordRequest,
  ForgotPasswordResponse,
  GoogleAuthRequest,
  GoogleAuthResponse,
  LoginRequest,
  LoginResponse,
  RegisterRequest,
  RegisterResponse,
  ResetPasswordRequest,
  ResetPasswordResponse,
  VerifyResetCodeRequest,
  VerifyResetCodeResponse,
} from './auth.types';
import type { AuthSessionResponse } from './authSession';

const apiClient = createApiClient({ handleUnauthorized: false });

export async function register(request: RegisterRequest): Promise<RegisterResponse> {
  const response = await apiClient.post<RegisterResponse>('/api/auth/register', request);
  return response.data;
}

export async function login(request: LoginRequest): Promise<LoginResponse> {
  const response = await apiClient.post<LoginResponse>('/api/auth/login', request);
  return response.data;
}

export async function googleAuth(request: GoogleAuthRequest): Promise<GoogleAuthResponse> {
  const response = await apiClient.post<GoogleAuthResponse>('/api/auth/google', request);
  return response.data;
}

export async function forgotPassword(request: ForgotPasswordRequest): Promise<ForgotPasswordResponse> {
  const response = await apiClient.post<ForgotPasswordResponse>('/api/auth/forgot-password', request);
  return response.data;
}

export async function verifyResetCode(request: VerifyResetCodeRequest): Promise<VerifyResetCodeResponse> {
  const response = await apiClient.post<VerifyResetCodeResponse>('/api/auth/verify-reset-code', request);
  return response.data;
}

export async function resetPassword(request: ResetPasswordRequest): Promise<ResetPasswordResponse> {
  const response = await apiClient.post<ResetPasswordResponse>('/api/auth/reset-password', request);
  return response.data;
}

export async function getAuthSession(): Promise<AuthSessionResponse> {
  const response = await apiClient.get<AuthSessionResponse>('/api/auth/session');
  return response.data;
}

export async function logout(): Promise<void> {
  await apiClient.post('/api/auth/logout');
}
