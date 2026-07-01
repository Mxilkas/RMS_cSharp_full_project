using Microsoft.AspNetCore.Mvc;
using RentalProRMS.Api.Data;
using RentalProRMS.Api.Models;

namespace RentalProRMS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserData _userData;

    public AuthController(UserData userData)
    {
        _userData = userData;
    }

    [HttpPost("login")]
    public IActionResult Login(LoginRequest request)
    {
        User? user = _userData.Login(request.Username.Trim(), request.Password);
        if (user == null)
            return Unauthorized(new { message = "Invalid username or password." });

        return Ok(new
        {
            message = "Login successful.",
            userId = user.UserID,
            username = user.Username,
            role = user.Role
        });
    }
}
