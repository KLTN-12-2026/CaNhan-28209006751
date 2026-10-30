import { useEffect, useMemo, useRef, useState } from 'react';
import type { ChangeEvent, FormEvent } from 'react';
import AutoStoriesOutlinedIcon from '@mui/icons-material/AutoStoriesOutlined';
import BiotechOutlinedIcon from '@mui/icons-material/BiotechOutlined';
import PersonAddAlt1OutlinedIcon from '@mui/icons-material/PersonAddAlt1Outlined';
import SchoolOutlinedIcon from '@mui/icons-material/SchoolOutlined';
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
import { Link as RouterLink, useNavigate } from 'react-router-dom';
import { googleAuth, register } from './authApi';
import { GoogleSignInButton } from './GoogleSignInButton';
import { refreshAuthSession } from './authBootstrap';
import { invalidateCsrfToken } from '../../shared/apiClient';
import type { ApiErrorResponse, GoogleAuthResponse } from './auth.types';

interface FormState {
  fullName: string;
  username: string;
  email: string;
  password: string;
}

const initialForm: FormState = {
  fullName: '',
  username: '',
  email: '',
  password: '',
};

const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
const usernamePattern = /^[\p{L}\p{N}]+$/u;
const fullNameAllowedPattern = /[^\p{L}\s]/gu;

function sanitizeFullName(value: string) {
  return value.replace(fullNameAllowedPattern, '').replace(/\s{2,}/g, ' ');
}

export function RegisterPage() {
  const navigate = useNavigate();
  const [form, setForm] = useState<FormState>(initialForm);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [showPassword, setShowPassword] = useState(false);
  const passwordHideTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const passwordInputRef = useRef<HTMLInputElement | null>(null);

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

  const passwordHint = useMemo(() => {
    if (!form.password) {
      return 'Mật khẩu phải có ít nhất 8 ký tự, gồm cả chữ và số';
    }

    const hasEnoughLength = form.password.length >= 8;
    const hasLetter = /[A-Za-zÀ-ỹ]/.test(form.password);
    const hasDigit = /\d/.test(form.password);

    if (hasEnoughLength && hasLetter && hasDigit) {
      return 'Mật khẩu ổn';
    }

    return 'Mật khẩu phải có ít nhất 8 ký tự, gồm cả chữ và số';
  }, [form.password]);

  const handleChange =
    (field: keyof FormState) => (event: ChangeEvent<HTMLInputElement>) => {
      const nextValue = field === 'fullName' ? sanitizeFullName(event.target.value) : event.target.value;

      setForm((current) => ({
        ...current,
        [field]: nextValue,
      }));
      setError(null);
      setMessage(null);
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
    const username = form.username.trim();
    const rawEmail = form.email;
    const email = form.email.trim();
    const password = form.password;

    if (!username) {
      return 'Vui lòng nhập tên đăng nhập.';
    }

    if (username.length < 3 || username.length > 50) {
      return 'Tên đăng nhập phải từ 3 đến 50 ký tự.';
    }

    if (!usernamePattern.test(username)) {
      return 'Tên đăng nhập chỉ được gồm chữ và số.';
    }

    if (!email) {
      return 'Vui lòng nhập email.';
    }

    if (/\s/.test(rawEmail)) {
      return 'Email không được chứa khoảng trắng.';
    }

    if (!emailPattern.test(email)) {
      return 'Email không hợp lệ.';
    }

    if (!password) {
      return 'Vui lòng nhập mật khẩu.';
    }

    if (password.length < 8 || !/[A-Za-zÀ-ỹ]/.test(password) || !/\d/.test(password)) {
      return 'Mật khẩu phải có ít nhất 8 ký tự, gồm cả chữ và số.';
    }

    if (form.fullName.trim().length > 100) {
      return 'Họ tên không được vượt quá 100 ký tự.';
    }

    return null;
  };

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setMessage(null);
    setError(null);

    const validationError = validateForm();
    if (validationError) {
      setError(validationError);
      return;
    }

    setIsSubmitting(true);

    try {
      const response = await register({
        fullName: form.fullName.trim() || undefined,
        username: form.username.trim(),
        email: form.email.trim(),
        password: form.password,
      });

      setMessage(`Tạo tài khoản ${response.username} thành công.`);
      setForm(initialForm);
      navigate('/login', { replace: true });
    } catch (caughtError) {
      if (axios.isAxiosError<ApiErrorResponse>(caughtError) && caughtError.response?.data?.message) {
        setError(caughtError.response.data.message);
      } else {
        setError('Đăng ký thất bại, vui lòng thử lại.');
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleGoogleResponse = async (response: GoogleAuthResponse, pendingToken?: string) => {
    if (response.requiresUsername) {
      if (!pendingToken) {
        setError('Không thể hoàn tất đăng ký Google lúc này, vui lòng thử lại.');
        return;
      }

      navigate('/google-username', {
        state: {
          idToken: pendingToken,
          mode: 'register',
          email: response.email,
          fullName: response.fullName,
        },
      });
      setMessage(null);
      return;
    }

    invalidateCsrfToken();
    const session = await refreshAuthSession();
    setMessage(`Tạo tài khoản ${response.username} bằng Google thành công.`);
    setError(null);
    setForm(initialForm);
    navigate(session.role === 'Admin' ? '/admin' : '/home');
  };

  const handleGoogleCredential = async (credential: string) => {
    setIsSubmitting(true);
    setMessage(null);
    setError(null);

    try {
      await handleGoogleResponse(await googleAuth({ idToken: credential }), credential);
    } catch (caughtError) {
      if (axios.isAxiosError<ApiErrorResponse>(caughtError) && caughtError.response?.data?.message) {
        setError(caughtError.response.data.message);
      } else {
        setError('Đăng ký bằng Google thất bại, vui lòng thử lại.');
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  const brandPillSx = {
    height: 42,
    display: 'inline-flex',
    alignItems: 'center',
    justifyContent: 'center',
    gap: 0.9,
    px: 1.6,
    borderRadius: 999,
    flexShrink: 0,
    fontFamily: 'inherit',
  };

  const brandTextSx = {
    fontFamily: 'inherit',
    fontSize: '1rem',
    fontWeight: 800,
    lineHeight: 1,
    letterSpacing: 0,
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
      <Container
        maxWidth="lg"
        sx={{
          animation: 'authPageEnter 180ms cubic-bezier(0.2, 0, 0, 1) both',
        }}
      >
        <Box
          sx={{
            display: 'grid',
            gridTemplateColumns: { xs: '1fr', md: '1.02fr 0.98fr' },
            gap: { xs: 3, md: 5 },
            alignItems: 'center',
          }}
        >
          <Stack spacing={3}>
            <Stack direction="row" spacing={1.2} flexWrap="wrap" useFlexGap alignItems="center">
              <Box
                sx={{
                  ...brandPillSx,
                  color: 'text.primary',
                  bgcolor: 'primary.main',
                  boxShadow: '0 10px 24px rgba(106, 235, 165, 0.34)',
                }}
              >
                <ScienceOutlinedIcon sx={{ fontSize: 24, display: 'block' }} />
                <Typography sx={brandTextSx}>Atomos</Typography>
              </Box>

              <Box
                sx={{
                  ...brandPillSx,
                  color: 'secondary.main',
                  bgcolor: 'rgba(255, 255, 255, 0.62)',
                  border: '1px solid rgba(24, 183, 240, 0.36)',
                }}
              >
                <SchoolOutlinedIcon sx={{ fontSize: 24, display: 'block' }} />
                <Typography sx={brandTextSx}>THCS</Typography>
              </Box>
            </Stack>

            <Box>
              <Typography variant="h1" sx={{ maxWidth: 620, mb: 2 }}>
                Biến đổi Hóa học thành "thuật giả kim" của riêng bạn
              </Typography>
              <Typography sx={{ maxWidth: 580, color: 'text.secondary', fontSize: '1.05rem' }}>
                Atomos biến đống lý thuyết khô khan thành một thế giới tương tác siêu cuốn. Học nhẹ nhàng,
                luyện dễ dàng và tràn đầy cảm hứng.
              </Typography>
            </Box>

            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1.5}>
              <Paper
                elevation={0}
                sx={{
                  flex: 1,
                  minWidth: 0,
                  p: 2,
                  border: '1px solid rgba(106, 235, 165, 0.48)',
                  bgcolor: 'rgba(255,255,255,0.78)',
                }}
              >
                <BiotechOutlinedIcon color="primary" />
                <Typography fontWeight={800}>Xưởng Phản Ứng</Typography>
                <Typography color="text.secondary" fontSize="0.92rem">
                  Tự do "mix & match" các chất, kích hoạt mọi điều kiện để tận mắt chứng kiến những phản ứng ảo diệu nhất.
                </Typography>
              </Paper>

              <Paper
                elevation={0}
                sx={{
                  flex: 1,
                  minWidth: 0,
                  p: 2,
                  border: '1px solid rgba(24, 183, 240, 0.28)',
                  bgcolor: 'rgba(255,255,255,0.78)',
                }}
              >
                <AutoStoriesOutlinedIcon color="secondary" />
                <Typography fontWeight={800}>Lộ trình học tập</Typography>
                <Typography color="text.secondary" fontSize="0.92rem">
                  Nơi lưu trữ toàn bộ tiến độ, bài tập và điểm số. Theo dõi hành trình thăng cấp từ "newbie" thành bậc thầy Hóa học.
                </Typography>
              </Paper>
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
                <Typography variant="h2">Đăng ký tài khoản</Typography>
              </Box>

              {message && <Alert severity="success">{message}</Alert>}
              {error && <Alert severity="error">{error}</Alert>}

              <Box component="form" onSubmit={handleSubmit} noValidate>
                <Stack spacing={2.2}>
                  <TextField
                    label="Họ tên"
                    autoComplete="name"
                    value={form.fullName}
                    onChange={handleChange('fullName')}
                    inputProps={{ maxLength: 100 }}
                    fullWidth
                  />

                  <TextField
                    label="Tên đăng nhập"
                    autoComplete="username"
                    value={form.username}
                    onChange={handleChange('username')}
                    inputProps={{ maxLength: 50 }}
                    fullWidth
                  />

                  <TextField
                    label="Email"
                    autoComplete="email"
                    type="email"
                    value={form.email}
                    onChange={handleChange('email')}
                    inputProps={{ maxLength: 100 }}
                    fullWidth
                  />

                  <TextField
                    label="Mật khẩu"
                    autoComplete="new-password"
                    inputRef={passwordInputRef}
                    type={showPassword ? 'text' : 'password'}
                    value={form.password}
                    onChange={handleChange('password')}
                    helperText={passwordHint}
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

                  <Button
                    type="submit"
                    variant="contained"
                    size="large"
                    disabled={isSubmitting}
                    startIcon={
                      isSubmitting ? (
                        <CircularProgress size={18} color="inherit" />
                      ) : (
                        <PersonAddAlt1OutlinedIcon />
                      )
                    }
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
                    Tạo tài khoản
                  </Button>

                  <GoogleSignInButton
                    disabled={isSubmitting}
                    label="Đăng ký với Google"
                    onCredential={handleGoogleCredential}
                  />

                  <Typography color="text.secondary" textAlign="center">
                    Đã có tài khoản?{' '}
                    <Link
                      component={RouterLink}
                      to="/login"
                      fontWeight={700}
                      underline="hover"
                      sx={{ color: '#059669', '&:hover': { color: '#047857' } }}
                    >
                      Đăng nhập
                    </Link>
                  </Typography>
                </Stack>
              </Box>
            </Stack>
          </Paper>
        </Box>
      </Container>
    </Box>
  );
}
