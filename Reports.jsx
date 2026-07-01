import { BarChart3, CreditCard, PieChart } from 'lucide-react';
import ErrorState from '../components/ErrorState.jsx';
import LoadingState from '../components/LoadingState.jsx';
import PageHeading from '../components/PageHeading.jsx';
import StatCard from '../components/StatCard.jsx';
import {
  getPaymentsByType,
  getPropertiesByStatus,
  getReportsSummary,
} from '../api/dashboardApi.js';
import useApi from '../hooks/useApi.js';
import { formatMoney } from '../utils/formatters.js';

export default function Reports() {
  const { data, loading, error, reload } = useApi(async () => {
    const [summary, propertyStatus, paymentTypes] = await Promise.all([
      getReportsSummary(),
      getPropertiesByStatus(),
      getPaymentsByType(),
    ]);
    return { summary, propertyStatus, paymentTypes };
  }, []);

  if (loading) return <div className="container section-space"><LoadingState /></div>;
  if (error || !data) return <div className="container section-space"><ErrorState message={error} onRetry={reload} /></div>;

  const maxPropertyCount = Math.max(...data.propertyStatus.map((item) => item.count), 1);
  const maxPaymentCount = Math.max(...data.paymentTypes.map((item) => item.paymentCount), 1);

  return (
    <section className="section-space">
      <div className="container">
        <PageHeading
          eyebrow="LINQ reports"
          title="RentalPro performance reports"
          description="Report data is grouped and calculated by the ASP.NET Core API."
        />

        <div className="stats-grid compact-stats">
          <StatCard icon={BarChart3} label="Properties" value={data.summary.properties} helper="All property records" />
          <StatCard icon={BarChart3} label="Rentals" value={data.summary.rentals} helper="Rental agreements" />
          <StatCard icon={CreditCard} label="Payments" value={data.summary.payments} helper="Payment records" tone="accent" />
          <StatCard icon={PieChart} label="Apartments" value={data.summary.apartments} helper="Apartment listings" />
        </div>

        <div className="report-grid">
          <article className="report-card">
            <h2>Properties by Status</h2>
            <p>LINQ query syntax groups property records by their current status.</p>
            <div className="bar-list">
              {data.propertyStatus.map((item) => (
                <div className="bar-item" key={item.status}>
                  <div><span>{item.status}</span><strong>{item.count}</strong></div>
                  <div className="bar-track"><span style={{ width: `${(item.count / maxPropertyCount) * 100}%` }} /></div>
                </div>
              ))}
            </div>
          </article>

          <article className="report-card">
            <h2>Payments by Type</h2>
            <p>LINQ method syntax groups rent and sale payments.</p>
            <div className="bar-list">
              {data.paymentTypes.map((item) => (
                <div className="bar-item" key={item.paymentType}>
                  <div><span>{item.paymentType}</span><strong>{item.paymentCount}</strong></div>
                  <div className="bar-track accent-track"><span style={{ width: `${(item.paymentCount / maxPaymentCount) * 100}%` }} /></div>
                  <small>{formatMoney(item.totalPaid)} paid · {formatMoney(item.totalRemaining)} remaining</small>
                </div>
              ))}
            </div>
          </article>
        </div>
      </div>
    </section>
  );
}
