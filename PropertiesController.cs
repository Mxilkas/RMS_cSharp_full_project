using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using RentalProRMS.Api.Data;
using RentalProRMS.Api.Models;

namespace RentalProRMS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PropertiesController : ControllerBase
{
    private readonly PropertyData _data;
    private readonly OwnerData _ownerData;
    public PropertiesController(PropertyData data, OwnerData ownerData)
    {
        _data = data;
        _ownerData = ownerData;
    }

    [HttpGet]
    public IActionResult GetAll(string? search = null, string? status = null, string? type = null)
    {
        List<Property> properties = _data.GetAllProperties();

        // LINQ query syntax example requested for the learning project.
        if (!string.IsNullOrWhiteSpace(search))
        {
            properties = (from property in properties
                          where property.PropertyName.Contains(search, StringComparison.OrdinalIgnoreCase)
                             || property.Location.Contains(search, StringComparison.OrdinalIgnoreCase)
                             || property.PropertyType.Contains(search, StringComparison.OrdinalIgnoreCase)
                             || (property.OwnerName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
                          orderby property.PropertyID descending
                          select property).ToList();
        }
        if (!string.IsNullOrWhiteSpace(status))
            properties = properties.Where(p => p.Status.Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();
        if (!string.IsNullOrWhiteSpace(type))
            properties = properties.Where(p => p.PropertyType.Equals(type, StringComparison.OrdinalIgnoreCase)).ToList();
        return Ok(properties);
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        Property? property = _data.GetPropertyById(id);
        return property == null ? NotFound(new { message = "Property not found." }) : Ok(property);
    }

    [HttpPost]
    public IActionResult Create(Property property)
    {
        if (_ownerData.GetOwnerById(property.OwnerID) == null)
            return BadRequest(new { message = "Selected owner does not exist." });
        if (_data.NameExists(property.PropertyName.Trim()))
            return Conflict(new { message = "Property name already exists." });
        int id = _data.AddProperty(property);
        property.PropertyID = id;
        return CreatedAtAction(nameof(GetById), new { id }, property);
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, Property property)
    {
        if (_data.GetPropertyById(id) == null) return NotFound(new { message = "Property not found." });
        if (_ownerData.GetOwnerById(property.OwnerID) == null)
            return BadRequest(new { message = "Selected owner does not exist." });
        if (_data.NameExists(property.PropertyName.Trim(), id))
            return Conflict(new { message = "Property name already exists." });
        property.PropertyID = id;
        _data.UpdateProperty(property);
        return Ok(new { message = "Property updated successfully." });
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        try
        {
            return _data.DeleteProperty(id)
                ? Ok(new { message = "Property deleted successfully." })
                : NotFound(new { message = "Property not found." });
        }
        catch (SqlException)
        {
            return BadRequest(new { message = "Cannot delete this property because rentals, sales, or payments are linked." });
        }
    }
}
