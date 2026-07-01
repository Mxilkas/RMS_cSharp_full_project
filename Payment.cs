using System.ComponentModel.DataAnnotations;

namespace RentalProRMS.Api.Models;

public class Payment
{
    public int PaymentID { get; set; }

    [Range(1, int.MaxValue)]
    public int CustomerID { get; set; }

    [Range(1, int.MaxValue)]
    public int PropertyID { get; set; }

    [Required, StringLength(20)]
    public string PaymentType { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue)]
    public decimal TotalAmount { get; set; }

    [Range(0, double.MaxValue)]
    public decimal PaidAmount { get; set; }

    public decimal RemainingBalance { get; set; }

    [Required]
    public DateTime PaymentDate { get; set; }

    [StringLength(300)]
    public string? Note { get; set; }

    public string? CustomerName { get; set; }
    public string? PropertyName { get; set; }
}
