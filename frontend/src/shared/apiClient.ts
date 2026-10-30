import axios, {
  AxiosInstance,
  AxiosRequestConfig,
  InternalAxiosRequestConfig,
} from 'axios';
import { handleUnauthorizedApiResponse } from '../features/auth/authSession';

export interface ApiClientOptions {
  handleUnauthorized?: boolean;
  includeLegacyBaseUrl?: boolean;
  json?: boolean;
}

interface AtomosRequestConfig extends AxiosRequestConfig {
  atomosRequiresCsrf?: boolean;
}

interface AtomosInternalRequestConfig extends InternalAxiosRequestConfig {
  atomosRequiresCsrf?: boolean;
  atomosCsrfRetried?: boolean;
}

const unsafeMethods = new Set(['POST', 'PUT', 'PATCH', 'DELETE']);
let csrfToken: string | null = null;
let csrfRequest: Promise<string> | null = null;

export function resolveApiBaseUrl(includeLegacyBaseUrl = true): string {
  return (
    import.meta.env.VITE_API_URL ??
    (includeLegacyBaseUrl ? import.meta.env.VITE_API_BASE_URL : undefined) ??
    'https://localhost:7001'
  );
}

export function invalidateCsrfToken() {
  csrfToken = null;
  csrfRequest = null;
}

async function getCsrfToken() {
  if (csrfToken) return csrfToken;
  if (!csrfRequest) {
    csrfRequest = axios
      .get<{ token: string }>('/api/auth/csrf', {
        baseURL: resolveApiBaseUrl(),
        withCredentials: true,
      })
      .then(({ data }) => {
        csrfToken = data.token;
        return data.token;
      })
      .finally(() => {
        csrfRequest = null;
      });
  }
  return csrfRequest;
}

export function csrfProtectedGet(config: AxiosRequestConfig = {}): AxiosRequestConfig {
  return { ...config, atomosRequiresCsrf: true } as AtomosRequestConfig;
}

function requiresCsrf(config: AtomosInternalRequestConfig) {
  return unsafeMethods.has((config.method ?? 'GET').toUpperCase()) || config.atomosRequiresCsrf === true;
}

function isCsrfFailure(error: unknown) {
  return axios.isAxiosError<{ message?: string }>(error)
    && error.response?.status === 400
    && error.response.data?.message === 'CSRF validation failed.';
}

export function createApiClient({
  handleUnauthorized = true,
  includeLegacyBaseUrl = true,
  json = true,
}: ApiClientOptions = {}): AxiosInstance {
  const client = axios.create({
    baseURL: resolveApiBaseUrl(includeLegacyBaseUrl),
    withCredentials: true,
    ...(json ? { headers: { 'Content-Type': 'application/json' } } : {}),
  });

  client.interceptors.request.use(async (requestConfig) => {
    const config = requestConfig as AtomosInternalRequestConfig;
    if (requiresCsrf(config)) {
      config.headers['X-CSRF-TOKEN'] = await getCsrfToken();
    }
    return config;
  });

  client.interceptors.response.use(
    (response) => response,
    async (error) => {
      if (isCsrfFailure(error) && error.config) {
        const config = error.config as AtomosInternalRequestConfig;
        if (!config.atomosCsrfRetried) {
          config.atomosCsrfRetried = true;
          invalidateCsrfToken();
          config.headers['X-CSRF-TOKEN'] = await getCsrfToken();
          return client.request(config);
        }
      }

      if (handleUnauthorized && axios.isAxiosError(error)) {
        handleUnauthorizedApiResponse(error.response?.status);
      }
      return Promise.reject(error);
    },
  );

  return client;
}
