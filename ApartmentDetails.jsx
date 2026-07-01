import { ArrowLeft, Building2, CreditCard, MapPin, UserRound } from 'lucide-react';
import { Link, useParams } from 'react-router-dom';
import ErrorState from '../components/ErrorState.jsx';
import LoadingState from '../components/LoadingState.jsx';
import { getRecord } from '../api/crudApi.js';
import useApi from '../hooks/useApi.js';
import { formatMoney, getStatusTone } from '../utils/formatters.js';

export default function ApartmentDetails() {
  const { id } = useParams();
  const { data, loading, error, reload } = useApi(() => getRecord('/apartments', id), [id]);

  if (loading) return <div className="container section-space"><LoadingState /></div>;
  if (error || !data) return <div className="container section-space"><ErrorState message={error} onRetry={reload} /></div>;

  return (
    <section className="section-space">
      <div className="container">
        <Link className="back-link" to="/apartments"><ArrowLeft size={17} /> Back to Apartments</Link>
        <div className="apartment-details-grid">
          <div className="details-visual">
            <Building2 size={100} />
            <span className={`status-badge ${getStatusTone(data.status)}`}>{data.status}</span>
          </div>
          <article className="details-card">
            <span className="eyebrow">{data.propertyType}</span>
            <h1>{data.propertyName}</h1>
            <p className="details-location"><MapPin size={18} /> {data.location}</p>
            <strong className="details-price">{formatMoney(data.price)}<small>/month</small></strong>
            <p>{data.description || 'A professionally managed apartment available through RentalPro RMS.'}</p>
            <div className="details-meta">
              <div><UserRound size={19} /><span><small>Owner</small>{data.ownerName || `Owner #${data.ownerID}`}</span></div>
              <div><CreditCard size={19} /><span><small>Payment</small>Managed through RMS</span></div>
            </div>
            <div className="details-actions">
              <Link className="button button-primary" to="/payments">Make Payment</Link>
              <Link className="button button-outline" to="/contact">Ask a Question</Link>
            </div>
          </article>
        </div>
      </div>
    </section>
  );
}
