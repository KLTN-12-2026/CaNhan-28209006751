import { useEffect, useRef, useState } from 'react';
import type { ChangeEvent, FormEvent } from 'react';
import AutoStoriesOutlinedIcon from '@mui/icons-material/AutoStoriesOutlined';
import BiotechOutlinedIcon from '@mui/icons-material/BiotechOutlined';
import LoginOutlinedIcon from '@mui/icons-material/LoginOutlined';
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
import { googleAuth, login } from './authApi';
import { GoogleSignInButton } from './GoogleSignInButton';
import { refreshAuthSession } from './authBootstrap';
import { invalidateCsrfToken } from '../../shared/apiClient';
import type { ApiErrorResponse, GoogleAuthResponse } from './auth.types';

interface FormState {
  usernameOrEmail: string;
  password: string;
}

const initialForm: FormState = {
  usernameOrEmail: '',
  password: '',
};

const invalidCredentialsMessage = 'Tên tài khoản/email hoặc mật khẩu không trùng khớp.';
const fallbackLoginErrorMessage = 'Đăng nhập thất bại, vui lòng thử lại.';

function getLoginErrorMessage(caughtError: unknown) {
  if (!axios.isAxiosError<ApiErrorResponse>(caughtError)) {
    return fallbackLoginErrorMessage;
  }

  const status = caughtError.response?.status;
  const code = caughtError.response?.data?.code;
  const message = caughtError.response?.data?.message;

  if (code === 'INVALID_CREDENTIALS') {
    return invalidCredentialsMessage;
  }

  if (status && status >= 500) {
    return `Lỗi đăng nhập. Mã lỗi: ${code ?? status}.`;
  }

  if (message) {
    return message;
  }

  return fallbackLoginErrorMessage;
}

export function LoginPage() {
  const location = useLocation();
  const navigate = useNavigate();
  const [form, setForm] = useState<FormState>(initialForm);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [showPassword, setShowPassword] = useState(false);
  const passwordHideTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const passwordInputRef = useRef<HTMLInputElement | null>(null);

  useEffect(() => {
    if (location.search) {
      navigate('/login', { replace: true });
    }
  }, [location.search, navigate]);

  useEffect(() => {
    return () => {
      if (passwordHideTimerRef.current) {
        clearTimeout(passwordHideTimerRef.current);
      }
    };
  }, []);

  useEffect(() => {
    if (form.password) {
      return;
    }

    setShowPassword(false);

    if (passwordHideTimerRef.current) {
      clearTimeout(passwordHideTimerRef.current);
      passwordHideTimerRef.current = null;
    }
  }, [form.password]);

  const handleChange =
    (field: keyof FormState) => (event: ChangeEvent<HTMLInputElement>) => {
      setForm((current) => ({
        ...current,
        [field]: event.target.value,
      }));
      setError(null);
    };

  const focusPasswordEnd = (onlyWhenFocused = false) => {
    requestAnimationFrame(() => {
      const passwordInput = passwordInputRef.current;
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

  const handleRevealPassword = () => {
    if (!form.password) {
      return;
    }

    setShowPassword(true);
    focusPasswordEnd();

    if (passwordHideTimerRef.current) {
      clearTimeout(passwordHideTimerRef.current);
    }

    passwordHideTimerRef.current = setTimeout(() => {
      setShowPassword(false);
      focusPasswordEnd(true);
      passwordHideTimerRef.current = null;
    }, 1000);
  };

  const validateForm = () => {
    if (!form.usernameOrEmail.trim()) {
      return 'Vui lòng nhập tên đăng nhập hoặc email.';
    }

    if (!form.password) {
      return 'Vui lòng nhập mật khẩu.';
    }

    return null;
  };

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError(null);

    const validationError = validateForm();
    if (validationError) {
      setError(validationError);
      return;
    }

    setIsSubmitting(true);

    try {
      await login({
        usernameOrEmail: form.usernameOrEmail.trim(),
        password: form.password,
      });

      invalidateCsrfToken();
      const session = await refreshAuthSession();
      setForm(initialForm);
      navigate(session.role === 'Admin' ? '/admin' : '/home');
    } catch (caughtError) {
      setError(getLoginErrorMessage(caughtError));
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleGoogleResponse = async (response: GoogleAuthResponse, pendingToken?: string) => {
    if (response.requiresUsername) {
      if (!pendingToken) {
        setError('Không thể hoàn tất đăng nhập Google lúc này, vui lòng thử lại.');
        return;
      }

      navigate('/google-username', {
        state: {
          idToken: pendingToken,
          mode: 'login',
          email: response.email,
          fullName: response.fullName,
        },
      });
      return;
    }

    invalidateCsrfToken();
    const session = await refreshAuthSession();
    setForm(initialForm);
    navigate(session.role === 'Admin' ? '/admin' : '/home');
  };

  const handleGoogleCredential = async (credential: string) => {
    setIsSubmitting(true);
    setError(null);

    try {
      await handleGoogleResponse(await googleAuth({ idToken: credential }), credential);
    } catch (caughtError) {
      setError(getLoginErrorMessage(caughtError));
    } finally {
      setIsSubmitting(false);
    }
  };

  const forgotPasswordState = form.usernameOrEmail.trim()
    ? { account: form.usernameOrEmail.trim() }
    : undefined;

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
        maxWidth="lg"
        sx={{
          animation: 'authPageEnter 180ms cubic-bezier(0.2, 0, 0, 1) both',
        }}
      >
        <Box
          sx={{
            display: 'grid',
            gridTemplateColumns: { xs: '1fr', md: '0.9fr 1fr' },
            gap: { xs: 3, md: 5 },
            alignItems: 'center',
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
                }}
              >
                <ScienceOutlinedIcon sx={{ fontSize: 24, display: 'block' }} />
              </Box>
              <Box>
                <Typography variant="h2">Atomos</Typography>
                <Typography color="text.secondary">Học Hóa cùng Atomos</Typography>
              </Box>
            </Stack>

            <Box>
              <Typography variant="h1" sx={{ maxWidth: 560, mb: 2 }}>
                Học Hóa cùng Atomos
              </Typography>
              <Typography sx={{ maxWidth: 520, color: 'text.secondary', fontSize: '1.05rem' }}>
                Quay lại xưởng học tập của bạn, tiếp tục luyện phản ứng, theo dõi tiến độ và giữ nhịp học mỗi ngày.
              </Typography>
            </Box>

            <Stack spacing={1.5}>
              <Box
                sx={{
                  display: 'grid',
                  gridTemplateColumns: '42px 1fr',
                  gap: 1.5,
                  alignItems: 'center',
                  p: 2,
                  border: '1px solid rgba(106, 235, 165, 0.42)',
                  bgcolor: 'rgba(255, 255, 255, 0.72)',
                  borderRadius: 2,
                }}
              >
                <Box
                  sx={{
                    width: 42,
                    height: 42,
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                  }}
                >
                  <BiotechOutlinedIcon color="primary" sx={{ fontSize: 25 }} />
                </Box>
                <Box>
                  <Typography fontWeight={800}>Xưởng Phản Ứng</Typography>
                  <Typography color="text.secondary" fontSize="0.92rem">
                    Vào nhanh không gian thử nghiệm và luyện tập phản ứng.
                  </Typography>
                </Box>
              </Box>

              <Box
                sx={{
                  display: 'grid',
                  gridTemplateColumns: '42px 1fr',
                  gap: 1.5,
                  alignItems: 'center',
                  p: 2,
                  border: '1px solid rgba(24, 183, 240, 0.28)',
                  bgcolor: 'rgba(255, 255, 255, 0.72)',
                  borderRadius: 2,
                }}
              >
                <Box
                  sx={{
                    width: 42,
                    height: 42,
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                  }}
                >
                  <AutoStoriesOutlinedIcon color="secondary" sx={{ fontSize: 25 }} />
                </Box>
                <Box>
                  <Typography fontWeight={800}>Lộ trình học tập</Typography>
                  <Typography color="text.secondary" fontSize="0.92rem">
                    Tiếp tục nơi bạn dừng lại và giữ trọn mạch tiến bộ.
                  </Typography>
                </Box>
              </Box>
            </Stack>
          </Stack>

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
              <Box>
                <Typography variant="h2">Đăng nhập</Typography>
                <Typography color="text.secondary">Học Hóa cùng Atomos</Typography>
              </Box>

              {error && <Alert severity="error">{error}</Alert>}

              <Box component="form" onSubmit={handleSubmit} noValidate>
                <Stack spacing={2.2}>
                  <TextField
                    label="Tên đăng nhập hoặc email"
                    autoComplete="username"
                    value={form.usernameOrEmail}
                    onChange={handleChange('usernameOrEmail')}
                    inputProps={{ maxLength: 100 }}
                    fullWidth
                  />

                  <TextField
                    label="Mật khẩu"
                    autoComplete="current-password"
                    inputRef={passwordInputRef}
                    type={showPassword ? 'text' : 'password'}
                    value={form.password}
                    onChange={handleChange('password')}
                    InputProps={{
                      endAdornment: (
                        <InputAdornment position="end">
                          <IconButton
                            aria-label={showPassword ? 'Đang hiện mật khẩu' : 'Hiện mật khẩu trong 1 giây'}
                            edge="end"
                            disabled={!form.password}
                            onClick={handleRevealPassword}
                            onMouseDown={(event) => event.preventDefault()}
                            color="secondary"
                          >
                            {showPassword ? <VisibilityOffOutlinedIcon /> : <VisibilityOutlinedIcon />}
                          </IconButton>
                        </InputAdornment>
                      ),
                    }}
                    fullWidth
                  />

                  <Box sx={{ display: 'flex', justifyContent: 'flex-end', mt: -1 }}>
                    <Link
                      component={RouterLink}
                      to="/forgot-password"
                      state={forgotPasswordState}
                      fontWeight={700}
                      underline="hover"
                      sx={{ color: '#059669', '&:hover': { color: '#047857' } }}
                    >
                      Quên mật khẩu?
                    </Link>
                  </Box>

                  <Button
                    type="submit"
                    variant="contained"
                    size="large"
                    disabled={isSubmitting}
                    startIcon={isSubmitting ? <CircularProgress size={18} color="inherit" /> : <LoginOutlinedIcon />}
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
                    Đăng nhập
                  </Button>

                  <GoogleSignInButton
                    disabled={isSubmitting}
                    label="Đăng nhập với Google"
                    onCredential={handleGoogleCredential}
                  />
                </Stack>
              </Box>

              <Typography color="text.secondary" textAlign="center">
                Chưa có tài khoản?{' '}
                <Link
                  component={RouterLink}
                  to="/register"
                  fontWeight={700}
                  underline="hover"
                  sx={{ color: '#059669', '&:hover': { color: '#047857' } }}
                >
                  Đăng ký
                </Link>
              </Typography>
            </Stack>
          </Paper>
        </Box>
      </Container>
    </Box>
  );
}
