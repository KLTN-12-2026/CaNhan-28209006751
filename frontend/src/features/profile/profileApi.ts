import { createApiClient, resolveApiBaseUrl } from '../../shared/apiClient';

const apiClient = createApiClient({ includeLegacyBaseUrl: false, json: false });
const apiBaseUrl = resolveApiBaseUrl(false);

export interface UserProfile {
  id: number;
  username: string;
  email: string;
  fullName?: string | null;
  avatar?: string | null;
  requiresEmailPasswordSet: boolean;
  vipExpiresAt?: string | null;
}

export function resolveAvatarUrl(value?: string | null) {
  if (!value) return null;
  if (/^https?:\/\//i.test(value)) return value;
  return `${apiBaseUrl}${value}`;
}

export async function getProfile() {
  return (await apiClient.get<UserProfile>('/api/profile')).data;
}
