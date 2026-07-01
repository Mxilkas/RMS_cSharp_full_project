using System.ComponentModel.DataAnnotations;

namespace RentalProRMS.Api.Models;

public class Rental
{
    public int RentalID { get; set; }

    [Range(1, int.MaxValue)]
    public int PropertyID { get; set; }

    [Range(1, int.MaxValue)]
    public int CustomerID { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal RentAmount { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Deposit { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [Required, StringLength(20)]
    public string PaymentStatus { get; set; } = string.Empty;

    public string? PropertyName { get; set; }
    public string? CustomerName { get; set; }
}
