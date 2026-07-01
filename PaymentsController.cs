using Microsoft.AspNetCore.Mvc;
using RentalProRMS.Api.Data;
using RentalProRMS.Api.Models;

namespace RentalProRMS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly PaymentData _data;
    private readonly PropertyData _propertyData;
    private readonly CustomerData _customerData;
    private static readonly string[] AllowedTypes = ["Rent", "Sale"];

    public PaymentsController(PaymentData data, PropertyData propertyData, CustomerData customerData)
    {
        _data = data;
        _propertyData = propertyData;
        _customerData = customerData;
    }

    [HttpGet]
    public IActionResult GetAll(string? search = null, string? type = null)
    {
        List<Payment> payments = _data.GetAllPayments();
        if (!string.IsNullOrWhiteSpace(search))
            payments = payments.Where(p =>
                    (p.CustomerName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (p.PropertyName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    p.PaymentType.Contains(search, StringComparison.OrdinalIgnoreCase))
                .ToList();
        if (!string.IsNullOrWhiteSpace(type))
            payments = payments.Where(p => p.PaymentType.Equals(type, StringComparison.OrdinalIgnoreCase)).ToList();
        return Ok(payments);
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        Payment? payment = _data.GetPaymentById(id);
        return payment == null ? NotFound(new { message = "Payment not found." }) : Ok(payment);
    }

    [HttpPost]
    public IActionResult Create(Payment payment)
    {
        IActionResult? validation = ValidatePayment(payment);
        if (validation != null) return validation;
        payment.PaymentType = NormalizeType(payment.PaymentType);
        int id = _data.AddPayment(payment);
        payment.PaymentID = id;
        payment.RemainingBalance = payment.TotalAmount - payment.PaidAmount;
        return CreatedAtAction(nameof(GetById), new { id }, payment);
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, Payment payment)
    {
        if (_data.GetPaymentById(id) == null) return NotFound(new { message = "Payment not found." });
        IActionResult? validation = ValidatePayment(payment);
        if (validation != null) return validation;
        payment.PaymentID = id;
        payment.PaymentType = NormalizeType(payment.PaymentType);
        _data.UpdatePayment(payment);
        return Ok(new { message = "Payment updated successfully." });
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        return _data.DeletePayment(id)
            ? Ok(new { message = "Payment deleted successfully." })
            : NotFound(new { message = "Payment not found." });
    }

    private IActionResult? ValidatePayment(Payment payment)
    {
        if (!AllowedTypes.Contains(payment.PaymentType, StringComparer.OrdinalIgnoreCase))
            return BadRequest(new { message = "PaymentType must be Rent or Sale." });
        if (_propertyData.GetPropertyById(payment.PropertyID) == null)
            return BadRequest(new { message = "Selected property does not exist." });
        if (_customerData.GetCustomerById(payment.CustomerID) == null)
            return BadRequest(new { message = "Selected customer does not exist." });
        if (payment.PaidAmount > payment.TotalAmount)
            return BadRequest(new { message = "PaidAmount cannot be greater than TotalAmount." });
        return null;
    }

    private static string NormalizeType(string type) =>
        AllowedTypes.First(t => t.Equals(type, StringComparison.OrdinalIgnoreCase));
}
