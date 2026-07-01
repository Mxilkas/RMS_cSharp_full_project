using Microsoft.AspNetCore.Mvc;
using RentalProRMS.Api.Data;
using RentalProRMS.Api.Models;

namespace RentalProRMS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UserData _data;
    private static readonly string[] AllowedRoles = ["Admin", "Manager", "User"];

    public UsersController(UserData data) => _data = data;

    [HttpGet]
    public IActionResult GetAll(string? search = null)
    {
        List<User> users = _data.GetAllUsers();
        if (!string.IsNullOrWhiteSpace(search))
        {
            users = users.Where(u =>
                    u.Username.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    u.Role.Contains(search, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        // LINQ projection prevents passwords from being returned to React.
        List<UserSummary> result = users.Select(u => new UserSummary
        {
            UserID = u.UserID,
            Username = u.Username,
            Role = u.Role
        }).ToList();

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        User? user = _data.GetUserById(id);
        if (user == null) return NotFound(new { message = "User not found." });
        return Ok(new UserSummary { UserID = user.UserID, Username = user.Username, Role = user.Role });
    }

    [HttpPost]
    public IActionResult Create(User user)
    {
        if (!AllowedRoles.Contains(user.Role, StringComparer.OrdinalIgnoreCase))
            return BadRequest(new { message = "Role must be Admin, Manager, or User." });
        if (_data.UsernameExists(user.Username.Trim()))
            return Conflict(new { message = "Username already exists." });

        user.Username = user.Username.Trim();
        user.Role = NormalizeRole(user.Role);
        int id = _data.AddUser(user);
        return CreatedAtAction(nameof(GetById), new { id }, new { userID = id, user.Username, user.Role });
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, User user)
    {
        if (_data.GetUserById(id) == null) return NotFound(new { message = "User not found." });
        if (!AllowedRoles.Contains(user.Role, StringComparer.OrdinalIgnoreCase))
            return BadRequest(new { message = "Role must be Admin, Manager, or User." });
        if (_data.UsernameExists(user.Username.Trim(), id))
            return Conflict(new { message = "Username already exists." });

        user.UserID = id;
        user.Username = user.Username.Trim();
        user.Role = NormalizeRole(user.Role);
        _data.UpdateUser(user);
        return Ok(new { message = "User updated successfully." });
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        return _data.DeleteUser(id)
            ? Ok(new { message = "User deleted successfully." })
            : NotFound(new { message = "User not found." });
    }

    private static string NormalizeRole(string role) =>
        AllowedRoles.First(r => r.Equals(role, StringComparison.OrdinalIgnoreCase));
}
