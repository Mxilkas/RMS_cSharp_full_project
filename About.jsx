import { Building2, CheckCircle2, Eye, HeartHandshake, Target, UsersRound } from 'lucide-react';
import ErrorState from '../components/ErrorState.jsx';
import LoadingState from '../components/LoadingState.jsx';
import PageHeading from '../components/PageHeading.jsx';
import StatCard from '../components/StatCard.jsx';
import { getAboutContent } from '../api/websiteApi.js';
import useApi from '../hooks/useApi.js';

export default function About() {
  const { data, loading, error, reload } = useApi(getAboutContent, []);

  if (loading) return <div className="container section-space"><LoadingState /></div>;
  if (error || !data) return <div className="container section-space"><ErrorState message={error} onRetry={reload} /></div>;

  const valueIcons = [CheckCircle2, HeartHandshake, Target];

  return (
    <section className="section-space">
      <div className="container">
        <PageHeading eyebrow="About RentalPro RMS" title={data.title} description={data.introduction} />

        <div className="about-story-grid">
          <article className="about-panel primary-panel">
            <span><Target size={25} /></span>
            <h2>Our Mission</h2>
            <p>{data.mission}</p>
          </article>
          <article className="about-panel accent-panel">
            <span><Eye size={25} /></span>
            <h2>Our Vision</h2>
            <p>{data.vision}</p>
          </article>
        </div>

        <div className="stats-grid about-stats">
          <StatCard icon={Building2} label="Properties" value={data.stats.properties} helper="Managed records" />
          <StatCard icon={Building2} label="Available" value={data.stats.availableProperties} helper="Open listings" tone="accent" />
          <StatCard icon={UsersRound} label="Owners" value={data.stats.owners} helper="Property partners" />
          <StatCard icon={UsersRound} label="Customers" value={data.stats.customers} helper="Tenants and buyers" />
        </div>

        <div className="section-heading centered-heading values-heading">
          <span className="eyebrow">What guides the system</span>
          <h2>Practical values for reliable management</h2>
        </div>
        <div className="feature-grid">
          {data.values.map((value, index) => {
            const Icon = valueIcons[index] || CheckCircle2;
            return (
              <article className="feature-card" key={value.title}>
                <span><Icon size={24} /></span>
                <h3>{value.title}</h3>
                <p>{value.description}</p>
              </article>
            );
          })}
        </div>
      </div>
    </section>
  );
}
