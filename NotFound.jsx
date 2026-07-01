import { ArrowLeft, Home } from 'lucide-react';
import { Link } from 'react-router-dom';

export default function NotFound() {
  return (
    <section className="not-found-section">
      <div className="not-found-card">
        <span>404</span>
        <h1>Page not found</h1>
        <p>The page you requested does not exist in RentalPro RMS.</p>
        <div>
          <Link className="button button-primary" to="/"><Home size={17} /> Home</Link>
          <Link className="button button-outline" to="/management"><ArrowLeft size={17} /> Management</Link>
        </div>
      </div>
    </section>
  );
}
