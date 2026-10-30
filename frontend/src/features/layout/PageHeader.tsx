import { AccountCircleOutlined, ArrowBack } from '@mui/icons-material';
import { ReactNode } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuthSession } from '../auth/authSession';
import { resolveAvatarUrl } from '../profile/profileApi';
import './pageHeader.css';

interface PageHeaderProps {
  kicker: string;
  title: string;
  subtitle?: string;
  onBack?: () => void;
  status?: ReactNode;
}

export function PageHeader({ kicker, title, subtitle, onBack, status }: PageHeaderProps) {
  const navigate = useNavigate();
  const auth = useAuthSession();
  const displayName = auth.user?.fullName?.trim() || auth.user?.username || 'Học viên';
  const avatarUrl = resolveAvatarUrl(auth.user?.avatar ?? null);

  return (
    <div className="page-header">
      <div className="home-topbar">
        {status}
        <button className="profile-name" type="button" onClick={() => navigate('/profile')}>
          {avatarUrl ? <img className="profile-name-avatar" src={avatarUrl} alt={displayName} /> : <AccountCircleOutlined />}
          <span>{displayName}</span>
        </button>
      </div>
      <header className="chapter-banner">
        {onBack && (
          <button className="chapter-back" type="button" aria-label="Quay lại" onClick={onBack}>
            <ArrowBack />
          </button>
        )}
        <div>
          <span className="chapter-kicker">{kicker}</span>
          <h1>{title}</h1>
          {subtitle && <p>{subtitle}</p>}
        </div>
        <div className="chapter-orbit" aria-hidden="true"><span /><span /><span /><i /></div>
      </header>
    </div>
  );
}
