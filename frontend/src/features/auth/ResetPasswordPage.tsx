import { useEffect, useRef, useState } from 'react';
import type { ChangeEvent, FormEvent, RefObject } from 'react';
import ArrowBackOutlinedIcon from '@mui/icons-material/ArrowBackOutlined';
import LockResetOutlinedIcon from '@mui/icons-material/LockResetOutlined';
import ScienceOutlinedIcon from '@mui/icons-material/ScienceOutlined';
import VisibilityOffOutlinedIcon from '@mui/icons-material/VisibilityOffOutlined';
import VisibilityOutlinedIcon from '@mui/icons-material/VisibilityOutlined';
import {
  Alert,
  Box,
  Button,
  CircularProgress,
  Container,
  IconButton,
  InputAdornment,
  Link,
  Paper,
  Stack,
  TextField,
  Typography,
} from '@mui/material';
import axios from 'axios';
import { Link as RouterLink, useLocation, useNavigate } from 'react-router-dom';
import { resetPassword } from './authApi';
import type { ApiErrorResponse } from './auth.types';

interface ResetPasswordLocationState {
  usernameOrEmail?: string;
  code?: string;
}

const fallbackResetPasswordErrorMessage = 'Không thể đặt lại mật khẩu lúc này, vui lòng thử lại.';

function getResetPasswordErrorMessage(caughtError: unknown) {
  if (!axios.isAxiosError<ApiErrorResponse>(caughtError)) {
    return fallbackResetPasswordErrorMessage;
  }

  const status = caughtError.response?.status;
  const code = caughtError.response?.data?.code;
  const message = caughtError.response?.data?.message;

  if (status && status >= 500) {
    return `Lỗi đặt lại mật khẩu. Mã lỗi: ${code ?? status}.`;
  }

  if (message) {
    return message;
  }

  return fallbackResetPasswordErrorMessage;
}

function validatePassword(password: string, confirmPassword: string) {
  if (!password) {
    return 'Vui lòng nhập mật khẩu mới.';
  }

  if (password.length < 8) {
    return 'Mật khẩu phải có ít nhất 8 ký tự.';
  }

  if (!/[A-Za-zÀ-ỹ]/u.test(password) || !/\d/.test(password)) {
    return 'Mật khẩu phải có cả chữ và số.';
  }

  if (password !== confirmPassword) {
    return 'Mật khẩu nhập lại chưa trùng khớp.';
  }

  return null;
}

export function ResetPasswordPage() {
  const location = useLocation();
  const navigate = useNavigate();
  const locationState = (location.state ?? {}) as ResetPasswordLocationState;
  const passwordInputRef = useRef<HTMLInputElement | null>(null);
  const confirmPasswordInputRef = useRef<HTMLInputElement | null>(null);
  const passwordHideTimerRef = useRef<number | null>(null);
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  const usernameOrEmail = locationState.usernameOrEmail ?? '';
  const code = locationState.code ?? '';
  const canResetPassword = Boolean(usernameOrEmail && code);
  const loginPath = '/login';

  useEffect(() => {
    return () => {
      if (passwordHideTimerRef.current) {
        window.clearTimeout(passwordHideTimerRef.current);
      }
    };
  }, []);

  const focusInputEnd = (inputRef: RefObject<HTMLInputElement | null>, onlyWhenFocused = false) => {
    requestAnimationFrame(() => {
      const passwordInput = inputRef.current;
      if (!passwordInput) {
        return;
      }

      if (onlyWhenFocused && document.activeElement !== passwordInput) {
        return;
      }

      const cursorPosition = passwordInput.value.length;
      passwordInput.focus();
      passwordInput.setSelectionRange(cursorPosition, cursorPosition);
    });
  };

  const handleRevealPassword = (inputRef: RefObject<HTMLInputElement | null>) => {
    if (!password && !confirmPassword) {
      return;
    }

    setShowPassword(true);
    focusInputEnd(inputRef);

    if (passwordHideTimerRef.current) {
      window.clearTimeout(passwordHideTimerRef.current);
    }

    passwordHideTimerRef.current = window.setTimeout(() => {
      setShowPassword(false);
      focusInputEnd(inputRef, true);
      passwordHideTimerRef.current = null;
    }, 1000);
  };

  const handlePasswordChange = (event: ChangeEvent<HTMLInputElement>) => {
    setPassword(event.target.value);
    setMessage(null);
    setError(null);
  };

  const handleConfirmPasswordChange = (event: ChangeEvent<HTMLInputElement>) => {
    setConfirmPassword(event.target.value);
    setMessage(null);
    setError(null);
  };

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setMessage(null);
    setError(null);

    if (!canResetPassword) {
      setError('Vui lòng xác nhận mã trước khi đặt lại mật khẩu.');
      return;
    }

    const validationError = validatePassword(password, confirmPassword);
    if (validationError) {
      setError(validationError);
      return;
    }

    setIsSubmitting(true);

    try {
      const response = await resetPassword({
        usernameOrEmail,
        code,
        password,
      });

      setMessage(response.message);
      setPassword('');
      setConfirmPassword('');
      setShowPassword(false);
      navigate('/home');
    } catch (caughtError) {
      setError(getResetPasswordErrorMessage(caughtError));
    } finally {
      setIsSubmitting(false);
    }
  };

  const getPasswordAdornment = (inputRef: RefObject<HTMLInputElement | null>) => (
    <InputAdornment position="end">
      <IconButton
        type="button"
        aria-label={showPassword ? 'Đang hiện mật khẩu' : 'Hiện mật khẩu trong 1 giây'}
        edge="end"
        onMouseDown={(event) => event.preventDefault()}
        onClick={() => handleRevealPassword(inputRef)}
      >
        {showPassword ? <VisibilityOffOutlinedIcon /> : <VisibilityOutlinedIcon />}
      </IconButton>
    </InputAdornment>
  );

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
      <Container
        maxWidth="sm"
        sx={{
          animation: 'authPageEnter 180ms cubic-bezier(0.2, 0, 0, 1) both',
        }}
      >
        <Paper
          elevation={0}
          sx={{
            p: { xs: 2.5, sm: 4 },
            borderRadius: '11px',
            border: '1px solid rgba(106, 235, 165, 0.42)',
            boxShadow: '0 24px 70px rgba(16, 35, 63, 0.1)',
          }}
        >
          <Stack spacing={3}>
            <Stack direction="row" spacing={1.2} alignItems="center">
              <Box
                sx={{
                  width: 46,
                  height: 46,
                  display: 'inline-flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  borderRadius: 999,
                  color: 'text.primary',
                  bgcolor: 'primary.main',
                  boxShadow: '0 10px 24px rgba(106, 235, 165, 0.34)',
                  flex: '0 0 auto',
                }}
              >
                <ScienceOutlinedIcon sx={{ fontSize: 24, display: 'block' }} />
              </Box>
              <Box>
                <Typography variant="h2">Đặt lại mật khẩu</Typography>
                <Typography color="text.secondary">Tạo mật khẩu mới cho tài khoản Atomos.</Typography>
              </Box>
            </Stack>

            {!canResetPassword && (
              <Alert severity="warning">
                Phiên xác nhận mã chưa sẵn sàng. Vui lòng quay lại bước quên mật khẩu để nhận mã mới.
              </Alert>
            )}
            {message && <Alert severity="success">{message}</Alert>}
            {error && <Alert severity="error">{error}</Alert>}

            <Box component="form" onSubmit={handleSubmit} noValidate>
              <Stack spacing={2.2}>
                <TextField
                  label="Mật khẩu mới"
                  autoComplete="new-password"
                  type={showPassword ? 'text' : 'password'}
                  value={password}
                  onChange={handlePasswordChange}
                  inputRef={passwordInputRef}
                  disabled={!canResetPassword || Boolean(message)}
                  helperText="Mật khẩu có ít nhất 8 ký tự, gồm cả chữ và số"
                  InputProps={{ endAdornment: getPasswordAdornment(passwordInputRef) }}
                  fullWidth
                />

                <TextField
                  label="Nhập lại mật khẩu mới"
                  autoComplete="new-password"
                  type={showPassword ? 'text' : 'password'}
                  value={confirmPassword}
                  onChange={handleConfirmPasswordChange}
                  inputRef={confirmPasswordInputRef}
                  disabled={!canResetPassword || Boolean(message)}
                  InputProps={{ endAdornment: getPasswordAdornment(confirmPasswordInputRef) }}
                  fullWidth
                />

                <Button
                  type="submit"
                  variant="contained"
                  size="large"
                  disabled={isSubmitting || !canResetPassword || Boolean(message)}
                  startIcon={isSubmitting ? <CircularProgress size={18} color="inherit" /> : <LockResetOutlinedIcon />}
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
                  Đặt lại mật khẩu
                </Button>
              </Stack>
            </Box>

            <Typography color="text.secondary" textAlign="center">
              <Link
                component={RouterLink}
                to={message ? loginPath : '/forgot-password'}
                fontWeight={700}
                underline="hover"
                sx={{
                  display: 'inline-flex',
                  alignItems: 'center',
                  gap: 0.6,
                  color: '#059669',
                  '&:hover': { color: '#047857' },
                }}
              >
                <ArrowBackOutlinedIcon sx={{ fontSize: 18 }} />
                {message ? 'Quay lại đăng nhập' : 'Quay lại quên mật khẩu'}
              </Link>
            </Typography>
          </Stack>
        </Paper>
      </Container>
    </Box>
  );
}
