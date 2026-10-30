import { useEffect, useState } from 'react';
import type { ChangeEvent, FormEvent } from 'react';
import ArrowBackOutlinedIcon from '@mui/icons-material/ArrowBackOutlined';
import CheckCircleOutlineOutlinedIcon from '@mui/icons-material/CheckCircleOutlineOutlined';
import MarkEmailReadOutlinedIcon from '@mui/icons-material/MarkEmailReadOutlined';
import ReplayOutlinedIcon from '@mui/icons-material/ReplayOutlined';
import ScienceOutlinedIcon from '@mui/icons-material/ScienceOutlined';
import SendOutlinedIcon from '@mui/icons-material/SendOutlined';
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
import { Link as RouterLink, useLocation, useNavigate, useSearchParams } from 'react-router-dom';
import { forgotPassword, verifyResetCode } from './authApi';
import type { ApiErrorResponse } from './auth.types';

const fallbackForgotPasswordErrorMessage = 'Không thể gửi mã lúc này, vui lòng thử lại.';
const fallbackVerifyCodeErrorMessage = 'Không thể xác nhận mã lúc này, vui lòng thử lại.';
const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
const usernamePattern = /^[\p{L}\p{N}]+$/u;
const cooldownSeconds = 60;
const cooldownStorageKeyPrefix = 'atomos-password-reset-cooldown';

type ForgotPasswordStep = 'request-code' | 'verify-code';

interface ForgotPasswordLocationState {
  account?: string;
}

function normalizeCooldownAccount(value: string) {
  return value.trim().toLowerCase();
}

function getCooldownStorageKey(account: string) {
  return `${cooldownStorageKeyPrefix}:${normalizeCooldownAccount(account)}`;
}

function getStoredCooldown(account: string) {
  const normalizedAccount = normalizeCooldownAccount(account);

  if (!normalizedAccount) {
    return 0;
  }

  try {
    const storedValue = window.sessionStorage.getItem(getCooldownStorageKey(normalizedAccount));
    const expiresAt = storedValue ? Number(storedValue) : 0;
    const remainingSeconds = Math.ceil((expiresAt - Date.now()) / 1000);

    if (!Number.isFinite(expiresAt) || remainingSeconds <= 0) {
      window.sessionStorage.removeItem(getCooldownStorageKey(normalizedAccount));
      return 0;
    }

    return remainingSeconds;
  } catch {
    return 0;
  }
}

function rememberCooldown(account: string) {
  const normalizedAccount = normalizeCooldownAccount(account);

  if (!normalizedAccount) {
    return;
  }

  try {
    window.sessionStorage.setItem(
      getCooldownStorageKey(normalizedAccount),
      String(Date.now() + cooldownSeconds * 1000),
    );
  } catch {
    // Cooldown is also enforced by the backend, so storage failures can be ignored.
  }
}

function getAuthErrorMessage(caughtError: unknown, fallbackMessage: string, serverErrorLabel: string) {
  if (!axios.isAxiosError<ApiErrorResponse>(caughtError)) {
    return fallbackMessage;
  }

  const status = caughtError.response?.status;
  const code = caughtError.response?.data?.code;
  const message = caughtError.response?.data?.message;

  if (status && status >= 500) {
    return `${serverErrorLabel}. Mã lỗi: ${code ?? status}.`;
  }

  if (message) {
    return message;
  }

  return fallbackMessage;
}

function validateUsernameOrEmail(value: string) {
  const trimmedValue = value.trim();

  if (!trimmedValue) {
    return 'Vui lòng nhập tên đăng nhập hoặc email.';
  }

  if (trimmedValue.includes('@')) {
    if (/\s/.test(value) || !emailPattern.test(trimmedValue)) {
      return 'Email không hợp lệ.';
    }

    return null;
  }

  if (trimmedValue.length < 3 || trimmedValue.length > 50) {
    return 'Tên đăng nhập phải từ 3 đến 50 ký tự.';
  }

  if (!usernamePattern.test(trimmedValue)) {
    return 'Tên đăng nhập chỉ được gồm chữ và số.';
  }

  return null;
}

function validateResetCode(value: string) {
  if (!value.trim()) {
    return 'Vui lòng nhập mã xác thực.';
  }

  if (!/^\d{6}$/.test(value)) {
    return 'Mã xác thực phải gồm 6 chữ số.';
  }

  return null;
}

export function ForgotPasswordPage() {
  const location = useLocation();
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const locationState = (location.state ?? {}) as ForgotPasswordLocationState;
  const initialAccount = locationState.account ?? searchParams.get('account') ?? '';
  const [usernameOrEmail, setUsernameOrEmail] = useState(initialAccount);
  const [sentAccount, setSentAccount] = useState('');
  const [resetCode, setResetCode] = useState('');
  const [step, setStep] = useState<ForgotPasswordStep>('request-code');
  const [cooldown, setCooldown] = useState(() => getStoredCooldown(initialAccount));
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isResending, setIsResending] = useState(false);
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (location.search) {
      navigate('/forgot-password', {
        replace: true,
        state: initialAccount ? { account: initialAccount } : undefined,
      });
    }
  }, [initialAccount, location.search, navigate]);

  useEffect(() => {
    if (cooldown <= 0) {
      return undefined;
    }

    const intervalId = window.setInterval(() => {
      setCooldown((current) => Math.max(0, current - 1));
    }, 1000);

    return () => window.clearInterval(intervalId);
  }, [cooldown]);

  const loginPath = '/login';

  const handleAccountChange = (event: ChangeEvent<HTMLInputElement>) => {
    const nextValue = event.target.value;

    setUsernameOrEmail(nextValue);
    setSentAccount('');
    setResetCode('');
    setStep('request-code');
    setCooldown(getStoredCooldown(nextValue));
    setMessage(null);
    setError(null);
  };

  const handleCodeChange = (event: ChangeEvent<HTMLInputElement>) => {
    setResetCode(event.target.value.replace(/\D/g, '').slice(0, 6));
    setError(null);
  };

  const sendCode = async (account: string, isResend = false) => {
    if (isResend) {
      setIsResending(true);
    } else {
      setIsSubmitting(true);
    }

    try {
      const response = await forgotPassword({
        usernameOrEmail: account,
      });

      setSentAccount(account);
      setStep('verify-code');
      setResetCode('');
      rememberCooldown(account);
      setCooldown(cooldownSeconds);
      setMessage(response.message);
      setError(null);
    } catch (caughtError) {
      setError(
        getAuthErrorMessage(
          caughtError,
          fallbackForgotPasswordErrorMessage,
          'Lỗi gửi mã xác thực',
        ),
      );
    } finally {
      setIsSubmitting(false);
      setIsResending(false);
    }
  };

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setMessage(null);
    setError(null);

    if (step === 'request-code') {
      const validationError = validateUsernameOrEmail(usernameOrEmail);
      if (validationError) {
        setError(validationError);
        return;
      }

      if (cooldown > 0) {
        setError(`Vui lòng chờ ${cooldown} giây trước khi gửi lại mã.`);
        return;
      }

      await sendCode(usernameOrEmail.trim());
      return;
    }

    const codeValidationError = validateResetCode(resetCode);
    if (codeValidationError) {
      setError(codeValidationError);
      return;
    }

    const account = sentAccount || usernameOrEmail.trim();
    setIsSubmitting(true);

    try {
      const verified = await verifyResetCode({
        usernameOrEmail: account,
        code: resetCode,
      });

      navigate('/reset-password', {
        state: {
          usernameOrEmail: account,
          code: verified.resetToken,
        },
      });
    } catch (caughtError) {
      setError(
        getAuthErrorMessage(
          caughtError,
          fallbackVerifyCodeErrorMessage,
          'Lỗi xác nhận mã',
        ),
      );
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleResendCode = async () => {
    const account = sentAccount || usernameOrEmail.trim();
    const validationError = validateUsernameOrEmail(account);

    setMessage(null);
    setError(null);

    if (validationError) {
      setError(validationError);
      return;
    }

    await sendCode(account, true);
  };

  const handleChangeAccount = () => {
    setSentAccount('');
    setResetCode('');
    setStep('request-code');
    setCooldown(getStoredCooldown(usernameOrEmail));
    setMessage(null);
    setError(null);
  };

  const isVerifyStep = step === 'verify-code';
  const isRequestBlockedByCooldown = !isVerifyStep && cooldown > 0;

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
                <Typography variant="h2">Quên mật khẩu</Typography>
                <Typography color="text.secondary">
                  {isVerifyStep
                    ? 'Nhập mã xác thực để tiếp tục đặt lại mật khẩu.'
                    : 'Atomos sẽ gửi mã xác thực đến email của bạn.'}
                </Typography>
              </Box>
            </Stack>

            {message && <Alert severity="success">{message}</Alert>}
            {error && <Alert severity="error">{error}</Alert>}

            <Box component="form" onSubmit={handleSubmit} noValidate>
              <Stack spacing={2.2}>
                <TextField
                  label="Tên đăng nhập hoặc email"
                  autoComplete="username"
                  value={usernameOrEmail}
                  onChange={handleAccountChange}
                  inputProps={{ maxLength: 100 }}
                  disabled={isVerifyStep}
                  fullWidth
                />

                {isVerifyStep && (
                  <Stack spacing={1.2}>
                    <TextField
                      label="Mã xác thực 6 chữ số"
                      autoComplete="one-time-code"
                      value={resetCode}
                      onChange={handleCodeChange}
                      inputProps={{
                        inputMode: 'numeric',
                        maxLength: 6,
                        pattern: '[0-9]*',
                      }}
                      autoFocus
                      fullWidth
                    />

                    <Stack
                      direction={{ xs: 'column', sm: 'row' }}
                      spacing={1}
                      justifyContent="space-between"
                      alignItems={{ xs: 'stretch', sm: 'center' }}
                    >
                      <Button
                        type="button"
                        variant="text"
                        onClick={handleChangeAccount}
                        disabled={isSubmitting || isResending}
                        sx={{
                          justifyContent: { xs: 'flex-start', sm: 'center' },
                          color: '#059669',
                        }}
                      >
                        Đổi tài khoản
                      </Button>

                      <Button
                        type="button"
                        variant="outlined"
                        onClick={handleResendCode}
                        disabled={cooldown > 0 || isSubmitting || isResending}
                        startIcon={isResending ? <CircularProgress size={18} color="inherit" /> : <ReplayOutlinedIcon />}
                        sx={{
                          borderColor: 'rgba(5, 150, 105, 0.32)',
                          color: '#059669',
                        }}
                      >
                        {cooldown > 0 ? `Gửi lại sau ${cooldown}s` : 'Gửi lại mã'}
                      </Button>
                    </Stack>
                  </Stack>
                )}

                <Button
                  type="submit"
                  variant="contained"
                  size="large"
                  disabled={isSubmitting || isRequestBlockedByCooldown}
                  startIcon={
                    isSubmitting ? (
                      <CircularProgress size={18} color="inherit" />
                    ) : isVerifyStep ? (
                      <CheckCircleOutlineOutlinedIcon />
                    ) : (
                      <SendOutlinedIcon />
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
                  {isVerifyStep ? 'Xác nhận' : cooldown > 0 ? `Gửi lại sau ${cooldown}s` : 'Gửi mã'}
                </Button>
              </Stack>
            </Box>

            {isVerifyStep && (
              <Stack
                direction="row"
                spacing={1}
                alignItems="center"
                sx={{ color: 'text.secondary' }}
              >
                <MarkEmailReadOutlinedIcon sx={{ color: '#18B7F0' }} />
                <Typography variant="body2">
                  Mã gồm 6 chữ số. Nếu chưa thấy email, bạn nhớ kiểm tra cả mục Spam nhé.
                </Typography>
              </Stack>
            )}

            <Typography color="text.secondary" textAlign="center">
              <Link
                component={RouterLink}
                to={loginPath}
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
                Quay lại đăng nhập
              </Link>
            </Typography>
          </Stack>
        </Paper>
      </Container>
    </Box>
  );
}
