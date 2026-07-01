import {
  ArrowRight,
  Building2,
  CheckCircle2,
  CreditCard,
  KeyRound,
  ShieldCheck,
  TrendingDown,
  TrendingUp,
  UsersRound,
} from 'lucide-react';
import { Link } from 'react-router-dom';
import ErrorState from '../components/ErrorState.jsx';
import LoadingState from '../components/LoadingState.jsx';
import OwnerCard from '../components/OwnerCard.jsx';
import PropertyCard from '../components/PropertyCard.jsx';
import StatCard from '../components/StatCard.jsx';
import useHomeData from '../hooks/useHomeData.js';
import { formatMoney } from '../utils/formatters.js';
import heroBuildings from '../assets/hero-buildings.svg';

export default function Home() {
  const { data, loading, error, reload } = useHomeData();

  if (loading) return <div className="container section-space"><LoadingState /></div>;
  if (error || !data) return <div className="container section-space"><ErrorState message={error} onRetry={reload} /></div>;

  const { content, dashboard, properties, owners } = data;

  return (
    <>
      <section className="hero-section">
        <div className="container hero-grid">
          <div className="hero-copy">
            <span className="eyebrow">Professional rental management</span>
            <h1>{content.heroTitle}</h1>
            <p>{content.heroDescription}</p>
            <div className="hero-actions">
              <Link className="button button-primary" to="/apartments">
                {content.primaryAction} <ArrowRight size={18} />
              </Link>
              <Link className="button button-outline" to="/management">
                {content.secondaryAction}
              </Link>
            </div>
            <div className="hero-trust">
              <span><CheckCircle2 size={17} /> Live SQL data</span>
              <span><CheckCircle2 size={17} /> Responsive design</span>
              <span><CheckCircle2 size={17} /> Simple role access</span>
            </div>
          </div>

          <div className="hero-visual">
            <img src={heroBuildings} alt="Modern property buildings illustration" />
            <div className="hero-floating-card hero-card-top">
              <Building2 size={21} />
              <div><strong>{dashboard.availableProperties}</strong><span>Available properties</span></div>
            </div>
            <div className="hero-floating-card hero-card-bottom">
              <CreditCard size={21} />
              <div><strong>{formatMoney(dashboard.totalPaymentReceived)}</strong><span>Total received</span></div>
            </div>
          </div>
        </div>
      </section>

      <section className="stats-strip">
        <div className="container stats-grid">
          <StatCard icon={Building2} label="Properties" value={dashboard.totalProperties} helper="Live property records" />
          <StatCard icon={UsersRound} label="Verified Owners" value={dashboard.totalOwners} helper="Connected owners" />
          <StatCard icon={KeyRound} label="Active Rentals" value={dashboard.totalRentals} helper="Managed agreements" />
          <StatCard icon={CreditCard} label="Payments" value={dashboard.totalPayments} helper="Recorded transactions" tone="accent" />
        </div>
      </section>

      <section className="section-space">
        <div className="container">
          <div className="section-heading centered-heading">
            <span className="eyebrow">One connected platform</span>
            <h2>Everything your rental team needs</h2>
            <p>Each section uses Axios to collect live records from your ASP.NET Core Web API.</p>
          </div>
          <div className="feature-grid">
            {content.highlights.map((item, index) => {
              const icons = [Building2, TrendingUp, ShieldCheck];
              const Icon = icons[index] || Building2;
              return (
                <article className="feature-card" key={item.title}>
                  <span><Icon size={24} /></span>
                  <h3>{item.title}</h3>
                  <p>{item.description}</p>
                </article>
              );
            })}
          </div>
        </div>
      </section>

      <section className="section-space light-section">
        <div className="container financial-grid">
          <div className="section-heading">
            <span className="eyebrow">Business overview</span>
            <h2>Understand performance at a glance</h2>
            <p>Dashboard values come directly from payments, rentals, sales and properties in SQL Server.</p>
            <Link className="text-link" to="/reports">Open complete reports <ArrowRight size={17} /></Link>
          </div>
          <article className="finance-card revenue-card">
            <span><TrendingUp size={24} /></span>
            <p>Total payment received</p>
            <strong>{formatMoney(dashboard.totalPaymentReceived)}</strong>
            <small>Combined paid amounts from payment records</small>
          </article>
          <article className="finance-card outstanding-card">
            <span><TrendingDown size={24} /></span>
            <p>Outstanding balance</p>
            <strong>{formatMoney(dashboard.totalOutstandingBalance)}</strong>
            <small>Balance still expected from customers</small>
          </article>
        </div>
      </section>

      <section className="section-space">
        <div className="container">
          <div className="section-heading section-heading-row">
            <div>
              <span className="eyebrow">Available apartments</span>
              <h2>Featured places ready for booking</h2>
            </div>
            <Link className="button button-outline" to="/apartments">See all apartments</Link>
          </div>
          {properties.length > 0 ? (
            <div className="property-grid">
              {properties.map((property, index) => (
                <PropertyCard key={property.propertyID} property={property} index={index} />
              ))}
            </div>
          ) : (
            <p className="muted-box">No available apartments are currently returned by the API.</p>
          )}
        </div>
      </section>

      <section className="section-space light-section">
        <div className="container">
          <div className="section-heading centered-heading">
            <span className="eyebrow">Verified owners</span>
            <h2>Trusted property partners</h2>
            <p>Owner contact details below are collected from the Owners API endpoint.</p>
          </div>
          <div className="owner-grid">
            {owners.map((owner) => <OwnerCard key={owner.ownerID} owner={owner} />)}
          </div>
        </div>
      </section>

      <section className="section-space">
        <div className="container cta-panel">
          <div>
            <span className="eyebrow">Ready to organize your work?</span>
            <h2>Open the management workspace and work with live records.</h2>
          </div>
          <Link className="button button-accent" to="/management">Go to Management <ArrowRight size={18} /></Link>
        </div>
      </section>
    </>
  );
}
