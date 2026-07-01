using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using RentalProRMS.Api.Data;
using RentalProRMS.Api.Models;

namespace RentalProRMS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly CustomerData _data;
    private static readonly string[] AllowedTypes = ["Tenant", "Buyer", "Both"];
    public CustomersController(CustomerData data) => _data = data;

    [HttpGet]
    public IActionResult GetAll(string? search = null, string? type = null)
    {
        List<Customer> customers = _data.GetAllCustomers();
        if (!string.IsNullOrWhiteSpace(search))
        {
            customers = customers.Where(c =>
                    c.FullName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    c.Phone.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (c.Email?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();
        }
        if (!string.IsNullOrWhiteSpace(type))
            customers = customers.Where(c => c.CustomerType.Equals(type, StringComparison.OrdinalIgnoreCase)).ToList();
        return Ok(customers);
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        Customer? customer = _data.GetCustomerById(id);
        return customer == null ? NotFound(new { message = "Customer not found." }) : Ok(customer);
    }

    [HttpPost]
    public IActionResult Create(Customer customer)
    {
        if (!AllowedTypes.Contains(customer.CustomerType, StringComparer.OrdinalIgnoreCase))
            return BadRequest(new { message = "CustomerType must be Tenant, Buyer, or Both." });
        if (_data.PhoneExists(customer.Phone.Trim()))
            return Conflict(new { message = "Customer phone already exists." });
        customer.CustomerType = NormalizeType(customer.CustomerType);
        int id = _data.AddCustomer(customer);
        customer.CustomerID = id;
        return CreatedAtAction(nameof(GetById), new { id }, customer);
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, Customer customer)
    {
        if (_data.GetCustomerById(id) == null) return NotFound(new { message = "Customer not found." });
        if (!AllowedTypes.Contains(customer.CustomerType, StringComparer.OrdinalIgnoreCase))
            return BadRequest(new { message = "CustomerType must be Tenant, Buyer, or Both." });
        if (_data.PhoneExists(customer.Phone.Trim(), id))
            return Conflict(new { message = "Customer phone already exists." });
        customer.CustomerID = id;
        customer.CustomerType = NormalizeType(customer.CustomerType);
        _data.UpdateCustomer(customer);
        return Ok(new { message = "Customer updated successfully." });
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        try
        {
            return _data.DeleteCustomer(id)
                ? Ok(new { message = "Customer deleted successfully." })
                : NotFound(new { message = "Customer not found." });
        }
        catch (SqlException)
        {
            return BadRequest(new { message = "Cannot delete this customer because rentals, sales, or payments are linked." });
        }
    }

    private static string NormalizeType(string type) =>
        AllowedTypes.First(t => t.Equals(type, StringComparison.OrdinalIgnoreCase));
}
