using System.ComponentModel.DataAnnotations;

namespace RentalProRMS.Api.Models;

public class Property
{
    public int PropertyID { get; set; }

    [Required, StringLength(100)]
    public string PropertyName { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string PropertyType { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Location { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }

    [Required, StringLength(20)]
    public string Status { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int OwnerID { get; set; }

    [StringLength(300)]
    public string? Description { get; set; }

    // Returned by JOIN queries. It is not a column in Properties.
    public string? OwnerName { get; set; }
}
