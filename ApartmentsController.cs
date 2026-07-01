using Microsoft.AspNetCore.Mvc;
using RentalProRMS.Api.Data;
using RentalProRMS.Api.Models;

namespace RentalProRMS.Api.Controllers;

// Apartments use the Properties table; no extra Apartments table is required.
[ApiController]
[Route("api/[controller]")]
public class ApartmentsController : ControllerBase
{
    private readonly PropertyData _propertyData;
    private readonly OwnerData _ownerData;

    public ApartmentsController(PropertyData propertyData, OwnerData ownerData)
    {
        _propertyData = propertyData;
        _ownerData = ownerData;
    }

    [HttpGet]
    public IActionResult GetAll(string? search = null)
    {
        List<Property> apartments = _propertyData.GetAllProperties()
            .Where(p => p.PropertyType.Equals("Apartment", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (!string.IsNullOrWhiteSpace(search))
            apartments = apartments.Where(p => p.PropertyName.Contains(search, StringComparison.OrdinalIgnoreCase)
                || p.Location.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
        return Ok(apartments);
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        Property? apartment = _propertyData.GetPropertyById(id);
        if (apartment == null || !apartment.PropertyType.Equals("Apartment", StringComparison.OrdinalIgnoreCase))
            return NotFound(new { message = "Apartment not found." });
        return Ok(apartment);
    }

    [HttpPost]
    public IActionResult Create(Property apartment)
    {
        if (_ownerData.GetOwnerById(apartment.OwnerID) == null)
            return BadRequest(new { message = "Selected owner does not exist." });
        if (_propertyData.NameExists(apartment.PropertyName.Trim()))
            return Conflict(new { message = "Apartment name already exists." });
        apartment.PropertyType = "Apartment";
        int id = _propertyData.AddProperty(apartment);
        apartment.PropertyID = id;
        return CreatedAtAction(nameof(GetById), new { id }, apartment);
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, Property apartment)
    {
        Property? existing = _propertyData.GetPropertyById(id);
        if (existing == null || !existing.PropertyType.Equals("Apartment", StringComparison.OrdinalIgnoreCase))
            return NotFound(new { message = "Apartment not found." });
        if (_ownerData.GetOwnerById(apartment.OwnerID) == null)
            return BadRequest(new { message = "Selected owner does not exist." });
        if (_propertyData.NameExists(apartment.PropertyName.Trim(), id))
            return Conflict(new { message = "Apartment name already exists." });
        apartment.PropertyID = id;
        apartment.PropertyType = "Apartment";
        _propertyData.UpdateProperty(apartment);
        return Ok(new { message = "Apartment updated successfully." });
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        Property? apartment = _propertyData.GetPropertyById(id);
        if (apartment == null || !apartment.PropertyType.Equals("Apartment", StringComparison.OrdinalIgnoreCase))
            return NotFound(new { message = "Apartment not found." });

        try
        {
            return _propertyData.DeleteProperty(id)
                ? Ok(new { message = "Apartment deleted successfully." })
                : NotFound(new { message = "Apartment not found." });
        }
        catch
        {
            return BadRequest(new { message = "Cannot delete this apartment because related records exist." });
        }
    }
}
