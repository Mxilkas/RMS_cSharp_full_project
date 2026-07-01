import { Building2, LockKeyhole, LogIn, UserRound } from 'lucide-react';
import { useState } from 'react';
import { Navigate, useLocation, useNavigate } from 'react-router-dom';
import BrandLogo from '../components/BrandLogo.jsx';
import { useAuth } from '../context/AuthContext.jsx';

export default function Login() {
  const { session, login } = useAuth();
  const location = useLocation();
  const navigate = useNavigate();
  const [username, setUsername] = useState('admin');
  const [password, setPassword] = useState('admin123');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  if (session) return <Navigate to="/management" replace />;

  async function handleSubmit(event) {
    event.preventDefault();
    setLoading(true);
    setError('');

    try {
      await login(username, password);
      navigate(location.state?.from || '/management', { replace: true });
    } catch (requestError) {
      setError(requestError.message);
    } finally {
      setLoading(false);
    }
  }

  return (
    <section className="login-section">
      <div className="login-card">
        <BrandLogo />
        <div className="login-heading">
          <span><Building2 size={26} /></span>
          <h1>Sign in to Management</h1>
          <p>Use a user account stored in the SQL Server Users table.</p>
        </div>
        {error && <div className="alert error-alert">{error}</div>}
        <form onSubmit={handleSubmit}>
          <label>
            <span>Username</span>
            <div className="input-with-icon"><UserRound size={18} /><input value={username} onChange={(event) => setUsername(event.target.value)} required /></div>
          </label>
          <label>
            <span>Password</span>
            <div className="input-with-icon"><LockKeyhole size={18} /><input type="password" value={password} onChange={(event) => setPassword(event.target.value)} required /></div>
          </label>
          <button className="button button-primary button-full" type="submit" disabled={loading}>
            <LogIn size={18} /> {loading ? 'Signing in...' : 'Sign In'}
          </button>
        </form>
        <div className="demo-accounts">
          <strong>Sample accounts</strong>
          <span>Admin: admin / admin123</span>
          <span>Manager: manager / manager123</span>
          <span>User: user1 / user123</span>
        </div>
      </div>
    </section>
  );
}
