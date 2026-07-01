import {
  BarChart3,
  Building,
  Building2,
  CreditCard,
  KeyRound,
  ShoppingBag,
  UserRound,
  UsersRound,
} from 'lucide-react';
import ErrorState from '../components/ErrorState.jsx';
import LoadingState from '../components/LoadingState.jsx';
import OperationCard from '../components/OperationCard.jsx';
import PageHeading from '../components/PageHeading.jsx';
import StatCard from '../components/StatCard.jsx';
import { useAuth } from '../context/AuthContext.jsx';
import useApi from '../hooks/useApi.js';
import { getDashboardSummary } from '../api/dashboardApi.js';
import { formatDate, formatMoney } from '../utils/formatters.js';

export default function ManagementOverview() {
  const { session } = useAuth();
  const { data, loading, error, reload } = useApi(getDashboardSummary, []);

  if (loading) return <div className="container section-space"><LoadingState /></div>;
  if (error || !data) return <div className="container section-space"><ErrorState message={error} onRetry={reload} /></div>;

  const adminOperations = [
    { title: 'Properties', description: 'Property type, location, price, status and owner.', to: '/properties', icon: Building2, count: data.totalProperties },
    { title: 'Apartments', description: 'Apartment listings for management and customer browsing.', to: '/apartments', icon: Building, count: data.apartmentCount },
    { title: 'Owners', description: 'Verified property owner records and contact details.', to: '/owners', icon: UsersRound, count: data.totalOwners },
    { title: 'Customers', description: 'Tenant and buyer information in one module.', to: '/customers', icon: UserRound, count: data.totalCustomers },
    { title: 'Rentals', description: 'Agreements, deposits, dates and payment status.', to: '/rentals', icon: KeyRound, count: data.totalRentals },
    { title: 'Sales', description: 'Property sales, paid amount and remaining balance.', to: '/sales', icon: ShoppingBag, count: data.totalSales },
    { title: 'Payments', description: 'Rental and sale payments with balances.', to: '/payments', icon: CreditCard, count: data.totalPayments },
    { title: 'Reports', description: 'Status and payment reports created with LINQ.', to: '/reports', icon: BarChart3 },
  ];

  const userOperations = adminOperations.filter((item) => ['/apartments', '/payments'].includes(item.to));
  const operations = session?.role === 'User' ? userOperations : adminOperations;

  return (
    <section className="section-space management-overview">
      <div className="container">
        <PageHeading
          eyebrow={`${session?.role} workspace`}
          title={`Welcome, ${session?.username}`}
          description="Choose a module below. Every number and record is loaded from the ASP.NET Core API."
        />

        <div className="stats-grid compact-stats">
          <StatCard icon={Building2} label="Available" value={data.availableProperties} helper="Properties ready" />
          <StatCard icon={KeyRound} label="Rented" value={data.rentedProperties} helper="Occupied properties" />
          <StatCard icon={CreditCard} label="Received" value={formatMoney(data.totalPaymentReceived)} helper="Payment total" tone="accent" />
          <StatCard icon={CreditCard} label="Outstanding" value={formatMoney(data.totalOutstandingBalance)} helper="Remaining balance" />
        </div>

        <div className="section-heading management-section-heading">
          <span className="eyebrow">System modules</span>
          <h2>Management operations</h2>
        </div>
        <div className="operation-grid">
          {operations.map((item) => <OperationCard key={item.to} {...item} />)}
        </div>

        {session?.role !== 'User' && (
          <div className="recent-grid">
            <article className="recent-card">
              <h3>Recent Rentals</h3>
              {data.recentRentals?.length ? data.recentRentals.map((rental) => (
                <div className="recent-row" key={rental.rentalID}>
                  <div><strong>{rental.propertyName}</strong><span>{rental.customerName}</span></div>
                  <div><strong>{formatMoney(rental.rentAmount)}</strong><span>{formatDate(rental.startDate)}</span></div>
                </div>
              )) : <p>No rental records available.</p>}
            </article>

            <article className="recent-card">
              <h3>Recent Sales</h3>
              {data.recentSales?.length ? data.recentSales.map((sale) => (
                <div className="recent-row" key={sale.saleID}>
                  <div><strong>{sale.propertyName}</strong><span>{sale.buyerName}</span></div>
                  <div><strong>{formatMoney(sale.salePrice)}</strong><span>{formatDate(sale.saleDate)}</span></div>
                </div>
              )) : <p>No sale records available.</p>}
            </article>
          </div>
        )}
      </div>
    </section>
  );
}
