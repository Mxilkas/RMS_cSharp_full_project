using System.ComponentModel.DataAnnotations;

namespace RentalProRMS.Api.Models;

public class Owner
{
    public int OwnerID { get; set; }

    [Required, StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string Phone { get; set; } = string.Empty;

    [EmailAddress, StringLength(100)]
    public string? Email { get; set; }

    [StringLength(200)]
    public string? Address { get; set; }
}
