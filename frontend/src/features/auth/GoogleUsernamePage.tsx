import { useState } from 'react';
import type { FormEvent } from 'react';
import CheckCircleOutlineOutlinedIcon from '@mui/icons-material/CheckCircleOutlineOutlined';
import ScienceOutlinedIcon from '@mui/icons-material/ScienceOutlined';
import {
  Alert,
  Box,
  Button,
  CircularProgress,
  Container,
  Link,
  Paper,
  Stack,
  TextField,
  Typography,
} from '@mui/material';
import axios from 'axios';
import { Link as RouterLink, useLocation, useNavigate } from 'react-router-dom';
import { googleAuth } from './authApi';
import { refreshAuthSession } from './authBootstrap';
import { invalidateCsrfToken } from '../../shared/apiClient';
import type { ApiErrorResponse } from './auth.types';

interface GoogleUsernameLocationState {
  idToken?: string;
  mode?: 'login' | 'register';
  email?: string;
  fullName?: string;
}

const usernamePattern = /^[\p{L}\p{N}]+$/u;

function getGoogleUsernameErrorMessage(caughtError: unknown) {
  if (axios.isAxiosError<ApiErrorResponse>(caughtError) && caughtError.response?.data?.message) {
    return caughtError.response.data.message;
  }

  return 'Không thể hoàn tất tài khoản Google lúc này, vui lòng thử lại.';
}

export function GoogleUsernamePage() {
  const location = useLocation();
  const navigate = useNavigate();
  const locationState = (location.state ?? {}) as GoogleUsernameLocationState;
  const [username, setUsername] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(
    locationState.idToken ? null : 'Phiên đăng nhập Google không còn hợp lệ. Vui lòng thử lại.',
  );

  const backPath = locationState.mode === 'register' ? '/register' : '/login';
  const backLabel = locationState.mode === 'register' ? 'Quay lại đăng ký' : 'Quay lại đăng nhập';

  const validateUsername = () => {
    const normalizedUsername = username.trim();

    if (!normalizedUsername) {
      return 'Vui lòng nhập tên đăng nhập.';
    }

    if (normalizedUsername.length < 3 || normalizedUsername.length > 50) {
      return 'Tên đăng nhập phải từ 3 đến 50 ký tự.';
    }

    if (!usernamePattern.test(normalizedUsername)) {
      return 'Tên đăng nhập chỉ được gồm chữ và số.';
    }

    return null;
  };

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();

    if (!locationState.idToken) {
      setError('Phiên đăng nhập Google không còn hợp lệ. Vui lòng thử lại.');
      return;
    }

    const validationError = validateUsername();
    if (validationError) {
      setError(validationError);
      return;
    }

    setIsSubmitting(true);
    setError(null);
    setMessage(null);

    try {
      const response = await googleAuth({
        idToken: locationState.idToken,
        username: username.trim(),
      });

      invalidateCsrfToken();
      const session = await refreshAuthSession();
      setMessage(`Hoàn tất tài khoản ${response.username} thành công.`);
      setUsername('');
      navigate(session.role === 'Admin' ? '/admin' : '/home');
    } catch (caughtError) {
      setError(getGoogleUsernameErrorMessage(caughtError));
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Box
      component="main"
      sx={{
        minHeight: '100vh',
        display: 'flex',
        alignItems: 'center',
        py: { xs: 4, md: 7 },
      }}
    >
      <Container maxWidth="sm" sx={{ animation: 'authPageEnter 180ms cubic-bezier(0.2, 0, 0, 1) both' }}>
        <Paper
          elevation={0}
          sx={{
            p: { xs: 2.5, sm: 4 },
            borderRadius: '14px',
            border: '1px solid #0a826a',
            boxShadow: '0 24px 70px rgba(16, 35, 63, 0.1)',
          }}
        >
          <Stack spacing={3}>
            <Stack direction="row" spacing={1.4} alignItems="center">
              <Box
                sx={{
                  width: 54,
                  height: 54,
                  display: 'inline-flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  borderRadius: 999,
                  color: 'text.primary',
                  bgcolor: 'primary.main',
                  boxShadow: '0 10px 24px rgba(106, 235, 165, 0.34)',
                }}
              >
                <ScienceOutlinedIcon sx={{ fontSize: 27, display: 'block' }} />
              </Box>
              <Box>
                <Typography variant="h2">Hoàn tất tài khoản Google</Typography>
                <Typography color="text.secondary">
                  Chọn tên đăng nhập để Atomos nhận ra bạn.
                </Typography>
              </Box>
            </Stack>

            {locationState.email && (
              <Typography color="text.secondary">
                Email Google: <Box component="span" sx={{ color: 'text.primary', fontWeight: 700 }}>{locationState.email}</Box>
              </Typography>
            )}

            {message && <Alert severity="success">{message}</Alert>}
            {error && <Alert severity="error">{error}</Alert>}

            <Box component="form" onSubmit={handleSubmit} noValidate>
              <Stack spacing={2.2}>
                <TextField
                  label="Tên đăng nhập"
                  autoComplete="username"
                  value={username}
                  disabled={Boolean(message)}
                  onChange={(event) => {
                    setUsername(event.target.value);
                    setError(null);
                  }}
                  inputProps={{ maxLength: 50 }}
                  fullWidth
                />

                <Button
                  type="submit"
                  variant="contained"
                  size="large"
                  disabled={isSubmitting || Boolean(message) || !locationState.idToken}
                  startIcon={isSubmitting ? <CircularProgress size={18} color="inherit" /> : <CheckCircleOutlineOutlinedIcon />}
                  sx={{
                    py: 1.35,
                    bgcolor: 'primary.main',
                    color: 'text.primary',
                    boxShadow: '0 12px 26px rgba(106, 235, 165, 0.36)',
                    '&:hover': {
                      bgcolor: 'primary.dark',
                      color: '#ffffff',
                    },
                  }}
                >
                  Hoàn tất
                </Button>

                <Typography color="text.secondary" textAlign="center">
                  <Link
                    component={RouterLink}
                    to={backPath}
                    fontWeight={700}
                    underline="hover"
                    sx={{ color: '#059669', '&:hover': { color: '#047857' } }}
                  >
                    {backLabel}
                  </Link>
                </Typography>
              </Stack>
            </Box>
          </Stack>
        </Paper>
      </Container>
    </Box>
  );
}
