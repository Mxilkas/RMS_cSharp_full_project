using Microsoft.AspNetCore.Mvc;
using RentalProRMS.Api.Data;
using RentalProRMS.Api.Models;

namespace RentalProRMS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly UserData _users;
    private readonly OwnerData _owners;
    private readonly CustomerData _customers;
    private readonly PropertyData _properties;
    private readonly RentalData _rentals;
    private readonly SaleData _sales;
    private readonly PaymentData _payments;

    public DashboardController(UserData users, OwnerData owners, CustomerData customers,
        PropertyData properties, RentalData rentals, SaleData sales, PaymentData payments)
    {
        _users = users;
        _owners = owners;
        _customers = customers;
        _properties = properties;
        _rentals = rentals;
        _sales = sales;
        _payments = payments;
    }

    [HttpGet]
    public IActionResult GetSummary()
    {
        List<Property> properties = _properties.GetAllProperties();
        List<Rental> rentals = _rentals.GetAllRentals();
        List<Sale> sales = _sales.GetAllSales();
        List<Payment> payments = _payments.GetAllPayments();

        DashboardSummary result = new()
        {
            TotalUsers = _users.GetAllUsers().Count,
            TotalOwners = _owners.GetAllOwners().Count,
            TotalCustomers = _customers.GetAllCustomers().Count,
            TotalProperties = properties.Count,
            AvailableProperties = properties.Count(p => p.Status.Equals("Available", StringComparison.OrdinalIgnoreCase)),
            RentedProperties = properties.Count(p => p.Status.Equals("Rented", StringComparison.OrdinalIgnoreCase)),
            ApartmentCount = properties.Count(p => p.PropertyType.Equals("Apartment", StringComparison.OrdinalIgnoreCase)),
            TotalRentals = rentals.Count,
            TotalSales = sales.Count,
            TotalPayments = payments.Count,
            TotalPaymentReceived = payments.Sum(p => p.PaidAmount),
            TotalOutstandingBalance = payments.Sum(p => p.RemainingBalance),
            RecentRentals = rentals.OrderByDescending(r => r.StartDate).Take(5).ToList(),
            RecentSales = sales.OrderByDescending(s => s.SaleDate).Take(5).ToList()
        };
        return Ok(result);
    }
}
