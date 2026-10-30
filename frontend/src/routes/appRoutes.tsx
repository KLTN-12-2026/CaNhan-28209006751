import React, { lazy } from 'react';
import { createBrowserRouter, Navigate } from 'react-router-dom';
import { useAuthSession } from '../features/auth/authSession';
import { AppShell } from '../features/layout/AppShell';

const ForgotPasswordPage = lazy(() => import('../features/auth/ForgotPasswordPage').then((module) => ({ default: module.ForgotPasswordPage })));
const GoogleUsernamePage = lazy(() => import('../features/auth/GoogleUsernamePage').then((module) => ({ default: module.GoogleUsernamePage })));
const LoginPage = lazy(() => import('../features/auth/LoginPage').then((module) => ({ default: module.LoginPage })));
const RegisterPage = lazy(() => import('../features/auth/RegisterPage').then((module) => ({ default: module.RegisterPage })));
const ResetPasswordPage = lazy(() => import('../features/auth/ResetPasswordPage').then((module) => ({ default: module.ResetPasswordPage })));
const HomePage = lazy(() => import('../features/home/HomePage').then((module) => ({ default: module.HomePage })));

function LandingRoute() {
  const auth = useAuthSession();
  if (auth.status === 'loading') return null;
  if (!auth.authenticated) return <Navigate to="/register" replace />;
  return <Navigate to={auth.role === 'Admin' ? '/admin' : '/home'} replace />;
}

function HomeRoute() {
  const auth = useAuthSession();
  if (auth.status === 'loading') return null;
  if (!auth.authenticated) return <Navigate to="/login" replace />;
  return auth.role === 'Admin' ? <Navigate to="/admin" replace /> : <AppShell><HomePage /></AppShell>;
}

export const router = createBrowserRouter([
  {
    path: '/',
    element: <LandingRoute />,
  },
  {
    path: '/home',
    element: <HomeRoute />,
  },
  {
    path: '/register',
    element: <RegisterPage />,
  },
  {
    path: '/login',
    element: <LoginPage />,
  },
  {
    path: '/google-username',
    element: <GoogleUsernamePage />,
  },
  {
    path: '/forgot-password',
    element: <ForgotPasswordPage />,
  },
  {
    path: '/reset-password',
    element: <ResetPasswordPage />,
  },
]);
