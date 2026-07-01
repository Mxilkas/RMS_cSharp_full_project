using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using RentalProRMS.Api.Data;
using RentalProRMS.Api.Models;

namespace RentalProRMS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OwnersController : ControllerBase
{
    private readonly OwnerData _data;
    public OwnersController(OwnerData data) => _data = data;

    [HttpGet]
    public IActionResult GetAll(string? search = null)
    {
        List<Owner> owners = _data.GetAllOwners();
        if (!string.IsNullOrWhiteSpace(search))
        {
            owners = owners.Where(o =>
                    o.FullName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    o.Phone.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (o.Email?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();
        }
        return Ok(owners);
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        Owner? owner = _data.GetOwnerById(id);
        return owner == null ? NotFound(new { message = "Owner not found." }) : Ok(owner);
    }

    [HttpPost]
    public IActionResult Create(Owner owner)
    {
        if (_data.PhoneExists(owner.Phone.Trim()))
            return Conflict(new { message = "Owner phone already exists." });
        owner.FullName = owner.FullName.Trim();
        owner.Phone = owner.Phone.Trim();
        int id = _data.AddOwner(owner);
        owner.OwnerID = id;
        return CreatedAtAction(nameof(GetById), new { id }, owner);
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, Owner owner)
    {
        if (_data.GetOwnerById(id) == null) return NotFound(new { message = "Owner not found." });
        if (_data.PhoneExists(owner.Phone.Trim(), id))
            return Conflict(new { message = "Owner phone already exists." });
        owner.OwnerID = id;
        _data.UpdateOwner(owner);
        return Ok(new { message = "Owner updated successfully." });
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        try
        {
            return _data.DeleteOwner(id)
                ? Ok(new { message = "Owner deleted successfully." })
                : NotFound(new { message = "Owner not found." });
        }
        catch (SqlException)
        {
            return BadRequest(new { message = "Cannot delete this owner because properties are linked to the owner." });
        }
    }
}
