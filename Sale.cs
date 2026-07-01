using System.ComponentModel.DataAnnotations;

namespace RentalProRMS.Api.Models;

public class Sale
{
    public int SaleID { get; set; }

    [Range(1, int.MaxValue)]
    public int PropertyID { get; set; }

    [Range(1, int.MaxValue)]
    public int BuyerID { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal SalePrice { get; set; }

    [Range(0, double.MaxValue)]
    public decimal PaidAmount { get; set; }

    public decimal Balance { get; set; }

    [Required]
    public DateTime SaleDate { get; set; }

    [Required, StringLength(20)]
    public string PaymentStatus { get; set; } = string.Empty;

    public string? PropertyName { get; set; }
    public string? BuyerName { get; set; }
}
