import {
  InfoOutlined,
  LogoutOutlined,
  MenuBookOutlined,
  ScienceOutlined,
  SearchOutlined,
} from '@mui/icons-material';
import { ReactNode } from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { logoutAuthSession } from '../auth/authBootstrap';
import './appShell.css';

const navItems = [
  { label: 'Lý thuyết', icon: MenuBookOutlined, to: '/home' },
  { label: 'Tra cứu', icon: SearchOutlined, to: '/home?view=lookup' },
  { label: 'Về chúng tôi', icon: InfoOutlined, to: '/home?view=about' },
];

export function AppShell({ children, sidebar = true }: { children: ReactNode; sidebar?: boolean }) {
  return (
    <div className={sidebar ? 'app-shell' : 'app-shell is-bare'}>
      {sidebar && <MainSidebar />}
      <div className="app-shell-content">{children}</div>
    </div>
  );
}

function MainSidebar() {
  const navigate = useNavigate();
  const { pathname, search } = useLocation();
  const here = pathname + search;

  const handleLogout = async () => {
    try {
      await logoutAuthSession();
      navigate('/login', { replace: true });
    } catch {
      window.alert('Không thể đăng xuất. Vui lòng thử lại.');
    }
  };

  return (
    <aside className="home-sidebar" aria-label="Điều hướng chính">
      <Link className="home-brand" to="/home">
        <span className="home-brand-mark"><ScienceOutlined /></span>
        <span>Atomos</span>
      </Link>

      <nav className="home-nav">
        {navItems.map(({ label, icon: Icon, to }) => (
          <Link
            className={here === to ? 'home-nav-item is-active' : 'home-nav-item'}
            aria-current={here === to ? 'page' : undefined}
            key={label}
            to={to}
          >
            <Icon />
            <span>{label}</span>
          </Link>
        ))}
      </nav>

      <button className="home-logout" type="button" onClick={() => void handleLogout()}>
        <LogoutOutlined />
        <span>Đăng xuất</span>
      </button>
    </aside>
  );
}
