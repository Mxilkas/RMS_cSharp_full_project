using Microsoft.AspNetCore.Mvc;
using RentalProRMS.Api.Data;
using RentalProRMS.Api.Models;

namespace RentalProRMS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly PropertyData _properties;
    private readonly PaymentData _payments;
    private readonly OwnerData _owners;
    private readonly CustomerData _customers;
    private readonly RentalData _rentals;
    private readonly SaleData _sales;

    public ReportsController(PropertyData properties, PaymentData payments, OwnerData owners,
        CustomerData customers, RentalData rentals, SaleData sales)
    {
        _properties = properties;
        _payments = payments;
        _owners = owners;
        _customers = customers;
        _rentals = rentals;
        _sales = sales;
    }

    [HttpGet("summary")]
    public IActionResult Summary()
    {
        return Ok(new
        {
            properties = _properties.GetAllProperties().Count,
            owners = _owners.GetAllOwners().Count,
            customers = _customers.GetAllCustomers().Count,
            rentals = _rentals.GetAllRentals().Count,
            sales = _sales.GetAllSales().Count,
            payments = _payments.GetAllPayments().Count,
            apartments = _properties.GetAllProperties().Count(p =>
                p.PropertyType.Equals("Apartment", StringComparison.OrdinalIgnoreCase))
        });
    }

    [HttpGet("properties-by-status")]
    public IActionResult PropertiesByStatus()
    {
        // LINQ query syntax with Group By.
        List<StatusReport> report = (from property in _properties.GetAllProperties()
                                     group property by property.Status into statusGroup
                                     orderby statusGroup.Key
                                     select new StatusReport
                                     {
                                         Status = statusGroup.Key,
                                         Count = statusGroup.Count()
                                     }).ToList();
        return Ok(report);
    }

    [HttpGet("payments-by-type")]
    public IActionResult PaymentsByType()
    {
        List<PaymentTypeReport> report = _payments.GetAllPayments()
            .GroupBy(p => p.PaymentType)
            .Select(group => new PaymentTypeReport
            {
                PaymentType = group.Key,
                PaymentCount = group.Count(),
                TotalPaid = group.Sum(p => p.PaidAmount),
                TotalRemaining = group.Sum(p => p.RemainingBalance)
            })
            .OrderBy(r => r.PaymentType)
            .ToList();
        return Ok(report);
    }
}
