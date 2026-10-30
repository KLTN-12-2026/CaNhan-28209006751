import { getAuthSession, logout } from './authApi';
import { applyAuthSession, getAuthRevision, markAuthUnauthenticated } from './authSession';
import { invalidateCsrfToken } from '../../shared/apiClient';

let bootstrapRequest: Promise<void> | null = null;
let latestSessionRequest = 0;

async function loadSession(markAnonymousOnError = false) {
  const requestId = ++latestSessionRequest;
  const expectedRevision = getAuthRevision();
  try {
    const session = await getAuthSession();
    if (requestId === latestSessionRequest) {
      applyAuthSession(session, expectedRevision);
    }
    return session;
  } catch (error) {
    if (markAnonymousOnError
      && requestId === latestSessionRequest
      && expectedRevision === getAuthRevision()) {
      markAuthUnauthenticated();
    }
    throw error;
  }
}

export function bootstrapAuthSession() {
  if (!bootstrapRequest) {
    bootstrapRequest = loadSession(true)
      .then(() => undefined)
      .catch(() => undefined)
      .finally(() => {
        bootstrapRequest = null;
      });
  }
  return bootstrapRequest;
}

export async function refreshAuthSession() {
  const session = await loadSession();
  if (!session.authenticated || !session.user || !session.role) {
    throw new Error('The server did not establish an authenticated session.');
  }
  return session;
}

export async function logoutAuthSession() {
  latestSessionRequest += 1;
  try {
    await logout();
    markAuthUnauthenticated();
    invalidateCsrfToken();
  } catch (error: unknown) {
    const status = typeof error === 'object' && error !== null && 'response' in error
      ? (error as { response?: { status?: number } }).response?.status
      : undefined;
    if (status === 401) {
      markAuthUnauthenticated();
      invalidateCsrfToken();
      return;
    }
    throw error;
  }
}
