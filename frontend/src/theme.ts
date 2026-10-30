import { createTheme } from '@mui/material/styles';

export const theme = createTheme({
  palette: {
    mode: 'light',
    primary: {
      main: '#6aeba5',
      dark: '#059669',
      light: '#d8ffe5',
      contrastText: '#064e3b',
    },
    secondary: {
      main: '#18B7F0',
      dark: '#18B7F0',
      light: '#BEEFFF',
      contrastText: '#082C42',
    },
    error: {
      main: '#F25F70',
    },
    success: {
      main: '#059669',
    },
    background: {
      default: '#f4fff8',
      paper: '#ffffff',
    },
    text: {
      primary: '#064e3b',
      secondary: '#49685B',
    },
  },
  shape: {
    borderRadius: 8,
  },
  typography: {
    fontFamily: ['Roboto', 'Segoe UI', 'Arial', 'sans-serif'].join(','),
    h1: {
      fontSize: '2.5rem',
      fontWeight: 800,
      lineHeight: 1.1,
      letterSpacing: 0,
    },
    h2: {
      fontSize: '1.45rem',
      fontWeight: 800,
      letterSpacing: 0,
    },
    button: {
      fontWeight: 700,
      letterSpacing: 0,
      textTransform: 'none',
    },
  },
});
