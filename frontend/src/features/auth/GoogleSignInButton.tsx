import { useEffect, useRef, useState } from 'react';
import { Button, Box } from '@mui/material';

interface GoogleSignInButtonProps {
  onCredential: (credential: string) => void;
  disabled?: boolean;
  label: string;
}

declare global {
  interface Window {
    google?: {
      accounts: {
        id: {
          initialize: (options: { client_id: string; callback: (response: { credential?: string }) => void }) => void;
          prompt: () => void;
        };
      };
    };
  }
}

const googleScriptId = 'google-identity-services';
const GoogleLogo = (
  <Box
    component="svg"
    viewBox="0 0 24 24"
    aria-hidden="true"
    sx={{ width: 22, height: 22, display: 'block' }}
  >
    <path
      fill="#4285F4"
      d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z"
    />
    <path
      fill="#34A853"
      d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"
    />
    <path
      fill="#FBBC05"
      d="M5.84 14.1c-.22-.66-.35-1.36-.35-2.1s.13-1.44.35-2.1V7.06H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.94l3.66-2.84z"
    />
    <path
      fill="#EA4335"
      d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.06L5.84 9.9C6.71 7.3 9.14 5.38 12 5.38z"
    />
  </Box>
);

export function GoogleSignInButton({ onCredential, disabled, label }: GoogleSignInButtonProps) {
  const containerRef = useRef<HTMLDivElement | null>(null);
  const [isReady, setIsReady] = useState(false);
  const clientId = import.meta.env.VITE_GOOGLE_CLIENT_ID;

  useEffect(() => {
    if (!clientId || disabled) {
      return undefined;
    }

    const existingScript = document.getElementById(googleScriptId) as HTMLScriptElement | null;

    const initialize = () => {
      if (!window.google || !containerRef.current) {
        return;
      }

      window.google.accounts.id.initialize({
        client_id: clientId,
        callback: (response) => {
          if (response.credential) {
            onCredential(response.credential);
          }
        },
      });
      setIsReady(true);
    };

    if (existingScript) {
      initialize();
      return undefined;
    }

    const script = document.createElement('script');
    script.id = googleScriptId;
    script.src = 'https://accounts.google.com/gsi/client';
    script.async = true;
    script.defer = true;
    script.onload = initialize;
    document.head.appendChild(script);

    return undefined;
  }, [clientId, disabled, onCredential]);

  if (!clientId) {
    return (
      <Button variant="outlined" size="large" disabled startIcon={GoogleLogo}>
        Chưa cấu hình Google Client ID
      </Button>
    );
  }

  return (
    <>
      <Button
        type="button"
        variant="outlined"
        size="large"
        fullWidth
        disabled={disabled || !isReady}
        startIcon={GoogleLogo}
        onClick={() => window.google?.accounts.id.prompt()}
        sx={{
          py: 1.25,
          borderColor: '#0a826a',
          color: '#064e3b',
          '&:hover': {
            borderColor: '#0a826a',
            bgcolor: 'rgba(6, 78, 59, 0.05)',
          },
        }}
      >
        {label}
      </Button>
      <Box ref={containerRef} sx={{ display: 'none' }} />
    </>
  );
}
