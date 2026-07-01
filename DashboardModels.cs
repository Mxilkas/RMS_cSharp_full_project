namespace RentalProRMS.Api.Models;

public class DashboardSummary
{
    public int TotalUsers { get; set; }
    public int TotalOwners { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalProperties { get; set; }
    public int AvailableProperties { get; set; }
    public int RentedProperties { get; set; }
    public int ApartmentCount { get; set; }
    public int TotalRentals { get; set; }
    public int TotalSales { get; set; }
    public int TotalPayments { get; set; }
    public decimal TotalPaymentReceived { get; set; }
    public decimal TotalOutstandingBalance { get; set; }
    public List<Rental> RecentRentals { get; set; } = new();
    public List<Sale> RecentSales { get; set; } = new();
}

public class StatusReport
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class PaymentTypeReport
{
    public string PaymentType { get; set; } = string.Empty;
    public int PaymentCount { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal TotalRemaining { get; set; }
}
