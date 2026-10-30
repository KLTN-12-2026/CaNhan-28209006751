import React from 'react';
import ReactDOM from 'react-dom/client';
import { CssBaseline, ThemeProvider } from '@mui/material';
import { RouterProvider } from 'react-router-dom';
import { router } from './routes/appRoutes';
import { theme } from './theme';
import { AuthBootstrapProvider } from './features/auth/AuthBootstrapProvider';
import './styles.css';

// Browsers replay earlier entries (old AI tutor prompts, answers, searches) as suggestions.
// Every field opts out unless it declares its own autocomplete, as the login and password fields do.
document.addEventListener('focusin', (event) => {
  const field = event.target;
  if ((field instanceof HTMLInputElement || field instanceof HTMLTextAreaElement) && !field.hasAttribute('autocomplete')) {
    field.setAttribute('autocomplete', 'off');
  }
});

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <AuthBootstrapProvider>
        <React.Suspense fallback={null}>
          <RouterProvider router={router} />
        </React.Suspense>
      </AuthBootstrapProvider>
    </ThemeProvider>
  </React.StrictMode>,
);
