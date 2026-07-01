using Microsoft.AspNetCore.Mvc;
using RentalProRMS.Api.Data;
using RentalProRMS.Api.Models;

namespace RentalProRMS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WebsiteController : ControllerBase
{
    private readonly PropertyData _properties;
    private readonly OwnerData _owners;
    private readonly CustomerData _customers;
    private readonly RentalData _rentals;

    public WebsiteController(
        PropertyData properties,
        OwnerData owners,
        CustomerData customers,
        RentalData rentals)
    {
        _properties = properties;
        _owners = owners;
        _customers = customers;
        _rentals = rentals;
    }

    [HttpGet("home")]
    public IActionResult Home()
    {
        return Ok(new
        {
            brandName = "RentalPro RMS",
            tagline = "Property, rental and payment management in one clear workspace.",
            heroTitle = "Manage every property relationship with confidence.",
            heroDescription = "RentalPro RMS helps owners, managers and customers organize properties, rentals, sales and payments through one responsive platform.",
            primaryAction = "Explore Apartments",
            secondaryAction = "Open Management",
            highlights = new[]
            {
                new { title = "Centralized Records", description = "Keep owners, customers, properties and contracts connected." },
                new { title = "Live Financial Tracking", description = "Monitor payments, balances, rentals and property sales." },
                new { title = "Responsive Access", description = "Use the system comfortably on desktop, tablet or mobile." }
            }
        });
    }

    [HttpGet("about")]
    public IActionResult About()
    {
        List<Property> properties = _properties.GetAllProperties();

        return Ok(new
        {
            title = "A simpler way to manage rental operations",
            introduction = "RentalPro RMS is designed for property teams that need practical tools without unnecessary complexity.",
            mission = "To make property management accurate, transparent and easy to use for every team member.",
            vision = "To become a trusted digital workspace for rental businesses and property owners in Mogadishu and beyond.",
            values = new[]
            {
                new { title = "Clarity", description = "Information is organized so users can understand it quickly." },
                new { title = "Reliability", description = "Records remain connected through consistent database relationships." },
                new { title = "Service", description = "The website supports owners, managers, tenants and buyers." }
            },
            stats = new
            {
                properties = properties.Count,
                availableProperties = properties.Count(p => p.Status.Equals("Available", StringComparison.OrdinalIgnoreCase)),
                owners = _owners.GetAllOwners().Count,
                customers = _customers.GetAllCustomers().Count,
                rentals = _rentals.GetAllRentals().Count
            }
        });
    }

    [HttpGet("contact")]
    public IActionResult Contact()
    {
        return Ok(new
        {
            companyName = "RentalPro RMS",
            address = "Hodan District, Mogadishu, Somalia",
            phone = "+252 61 0000000",
            email = "support@rentalprorms.com",
            workingHours = "Saturday - Thursday, 8:00 AM - 5:00 PM",
            responseTime = "We normally respond within one business day."
        });
    }

    [HttpPost("contact")]
    public IActionResult SendContactMessage(ContactMessageRequest request)
    {
        // This beginner-friendly version validates and accepts the message.
        // A future version can store messages in a ContactMessages table or send email.
        return Ok(new
        {
            message = "Thank you. Your message was received successfully.",
            receivedAt = DateTime.Now,
            sender = request.FullName
        });
    }
}
