using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using RentalProRMS.Api.Data;
using RentalProRMS.Api.Models;

namespace RentalProRMS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RentalsController : ControllerBase
{
    private readonly RentalData _data;
    private readonly PropertyData _propertyData;
    private readonly CustomerData _customerData;

    public RentalsController(RentalData data, PropertyData propertyData, CustomerData customerData)
    {
        _data = data;
        _propertyData = propertyData;
        _customerData = customerData;
    }

    [HttpGet]
    public IActionResult GetAll(string? search = null)
    {
        List<Rental> rentals = _data.GetAllRentals();
        if (!string.IsNullOrWhiteSpace(search))
            rentals = rentals.Where(r =>
                    (r.PropertyName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (r.CustomerName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    r.PaymentStatus.Contains(search, StringComparison.OrdinalIgnoreCase))
                .ToList();
        return Ok(rentals);
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        Rental? rental = _data.GetRentalById(id);
        return rental == null ? NotFound(new { message = "Rental not found." }) : Ok(rental);
    }

    [HttpPost]
    public IActionResult Create(Rental rental)
    {
        Property? property = _propertyData.GetPropertyById(rental.PropertyID);
        if (property == null) return BadRequest(new { message = "Selected property does not exist." });
        if (_customerData.GetCustomerById(rental.CustomerID) == null)
            return BadRequest(new { message = "Selected customer does not exist." });
        if (rental.EndDate.Date <= rental.StartDate.Date)
            return BadRequest(new { message = "EndDate must be after StartDate." });
        if (!property.Status.Equals("Available", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "Only an Available property can be rented." });

        int id = _data.AddRental(rental);
        _propertyData.UpdateStatus(rental.PropertyID, "Rented");
        rental.RentalID = id;
        return CreatedAtAction(nameof(GetById), new { id }, rental);
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, Rental rental)
    {
        Rental? existing = _data.GetRentalById(id);
        if (existing == null) return NotFound(new { message = "Rental not found." });
        Property? selectedProperty = _propertyData.GetPropertyById(rental.PropertyID);
        if (selectedProperty == null)
            return BadRequest(new { message = "Selected property does not exist." });
        if (existing.PropertyID != rental.PropertyID &&
            !selectedProperty.Status.Equals("Available", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "The new property must be Available." });
        if (_customerData.GetCustomerById(rental.CustomerID) == null)
            return BadRequest(new { message = "Selected customer does not exist." });
        if (rental.EndDate.Date <= rental.StartDate.Date)
            return BadRequest(new { message = "EndDate must be after StartDate." });

        rental.RentalID = id;
        _data.UpdateRental(rental);
        if (existing.PropertyID != rental.PropertyID)
            _propertyData.UpdateStatus(existing.PropertyID, "Available");
        _propertyData.UpdateStatus(rental.PropertyID, "Rented");
        return Ok(new { message = "Rental updated successfully." });
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        Rental? rental = _data.GetRentalById(id);
        if (rental == null) return NotFound(new { message = "Rental not found." });
        try
        {
            _data.DeleteRental(id);
            _propertyData.UpdateStatus(rental.PropertyID, "Available");
            return Ok(new { message = "Rental deleted successfully." });
        }
        catch (SqlException)
        {
            return BadRequest(new { message = "Rental could not be deleted." });
        }
    }
}
