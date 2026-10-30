import { useSyncExternalStore } from 'react';

export interface AuthenticatedUser {
  id: number;
  username: string;
  email: string;
  fullName?: string | null;
  avatar?: string | null;
}

export interface AuthSessionResponse {
  authenticated: boolean;
  user: AuthenticatedUser | null;
  role: string | null;
}

export interface AuthState extends AuthSessionResponse {
  status: 'loading' | 'authenticated' | 'unauthenticated';
}

const publicAuthPaths = new Set(['/', '/login', '/register', '/forgot-password', '/reset-password', '/google-username']);
const listeners = new Set<() => void>();
let authRedirectInProgress = false;
let revision = 0;
let state: AuthState = {
  status: 'loading',
  authenticated: false,
  user: null,
  role: null,
};

function publish(next: AuthState) {
  state = next;
  revision += 1;
  listeners.forEach((listener) => listener());
}

export function getAuthState() {
  return state;
}

export function getAuthRevision() {
  return revision;
}

export function subscribeAuthState(listener: () => void) {
  listeners.add(listener);
  return () => listeners.delete(listener);
}

export function useAuthSession() {
  return useSyncExternalStore(subscribeAuthState, getAuthState, getAuthState);
}

export function applyAuthSession(session: AuthSessionResponse, expectedRevision?: number) {
  if (expectedRevision !== undefined && expectedRevision !== revision) return false;

  if (!session.authenticated || !session.user || !session.role) {
    markAuthUnauthenticated();
    return true;
  }

  authRedirectInProgress = false;
  publish({
    status: 'authenticated',
    authenticated: true,
    user: session.user,
    role: session.role,
  });
  return true;
}

export function markAuthUnauthenticated() {
  publish({
    status: 'unauthenticated',
    authenticated: false,
    user: null,
    role: null,
  });
}

export function updateAuthenticatedUser(patch: Partial<AuthenticatedUser>) {
  if (!state.authenticated || !state.user) return;
  publish({ ...state, user: { ...state.user, ...patch } });
}

export function handleUnauthorizedApiResponse(status?: number) {
  if (status !== 401) return;

  markAuthUnauthenticated();
  if (authRedirectInProgress || publicAuthPaths.has(window.location.pathname)) return;

  authRedirectInProgress = true;
  window.location.replace('/login');
}
