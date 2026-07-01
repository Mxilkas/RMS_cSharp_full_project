using System.ComponentModel.DataAnnotations;

namespace RentalProRMS.Api.Models;

public class ContactMessageRequest
{
    [Required, StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(120)]
    public string Email { get; set; } = string.Empty;

    [StringLength(30)]
    public string? Phone { get; set; }

    [Required, StringLength(150)]
    public string Subject { get; set; } = string.Empty;

    [Required, StringLength(1000, MinimumLength = 10)]
    public string Message { get; set; } = string.Empty;
}
