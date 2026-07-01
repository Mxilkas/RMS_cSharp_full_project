using Microsoft.AspNetCore.Mvc;
using RentalProRMS.Api.Data;
using RentalProRMS.Api.Models;

namespace RentalProRMS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SalesController : ControllerBase
{
    private readonly SaleData _data;
    private readonly PropertyData _propertyData;
    private readonly CustomerData _customerData;

    public SalesController(SaleData data, PropertyData propertyData, CustomerData customerData)
    {
        _data = data;
        _propertyData = propertyData;
        _customerData = customerData;
    }

    [HttpGet]
    public IActionResult GetAll(string? search = null)
    {
        List<Sale> sales = _data.GetAllSales();
        if (!string.IsNullOrWhiteSpace(search))
            sales = sales.Where(s =>
                    (s.PropertyName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (s.BuyerName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    s.PaymentStatus.Contains(search, StringComparison.OrdinalIgnoreCase))
                .ToList();
        return Ok(sales);
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        Sale? sale = _data.GetSaleById(id);
        return sale == null ? NotFound(new { message = "Sale not found." }) : Ok(sale);
    }

    [HttpPost]
    public IActionResult Create(Sale sale)
    {
        Property? property = _propertyData.GetPropertyById(sale.PropertyID);
        if (property == null) return BadRequest(new { message = "Selected property does not exist." });
        if (_customerData.GetCustomerById(sale.BuyerID) == null)
            return BadRequest(new { message = "Selected buyer does not exist." });
        if (sale.PaidAmount > sale.SalePrice)
            return BadRequest(new { message = "PaidAmount cannot be greater than SalePrice." });
        if (!property.Status.Equals("Available", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "Only an Available property can be sold." });

        sale.PaymentStatus = CalculateStatus(sale.SalePrice, sale.PaidAmount);
        int id = _data.AddSale(sale);
        _propertyData.UpdateStatus(sale.PropertyID, "Sold");
        sale.SaleID = id;
        sale.Balance = sale.SalePrice - sale.PaidAmount;
        return CreatedAtAction(nameof(GetById), new { id }, sale);
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, Sale sale)
    {
        Sale? existing = _data.GetSaleById(id);
        if (existing == null) return NotFound(new { message = "Sale not found." });
        Property? selectedProperty = _propertyData.GetPropertyById(sale.PropertyID);
        if (selectedProperty == null)
            return BadRequest(new { message = "Selected property does not exist." });
        if (existing.PropertyID != sale.PropertyID &&
            !selectedProperty.Status.Equals("Available", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "The new property must be Available." });
        if (_customerData.GetCustomerById(sale.BuyerID) == null)
            return BadRequest(new { message = "Selected buyer does not exist." });
        if (sale.PaidAmount > sale.SalePrice)
            return BadRequest(new { message = "PaidAmount cannot be greater than SalePrice." });

        sale.SaleID = id;
        sale.PaymentStatus = CalculateStatus(sale.SalePrice, sale.PaidAmount);
        _data.UpdateSale(sale);
        if (existing.PropertyID != sale.PropertyID)
            _propertyData.UpdateStatus(existing.PropertyID, "Available");
        _propertyData.UpdateStatus(sale.PropertyID, "Sold");
        return Ok(new { message = "Sale updated successfully." });
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        Sale? sale = _data.GetSaleById(id);
        if (sale == null) return NotFound(new { message = "Sale not found." });
        _data.DeleteSale(id);
        _propertyData.UpdateStatus(sale.PropertyID, "Available");
        return Ok(new { message = "Sale deleted successfully." });
    }

    private static string CalculateStatus(decimal total, decimal paid)
    {
        if (paid <= 0) return "Unpaid";
        if (paid >= total) return "Paid";
        return "Partial";
    }
}
