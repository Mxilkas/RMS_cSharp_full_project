using Microsoft.Data.SqlClient;
using RentalProRMS.Api.Models;

namespace RentalProRMS.Api.Data;

public class UserData
{
    private readonly string _connectionString;

    public UserData(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection is missing in appsettings.json.");
    }

    public List<User> GetAllUsers()
    {
        List<User> users = new();
        using SqlConnection connection = new(_connectionString);
        using SqlCommand command = new("SELECT UserID, Username, Password, Role FROM Users ORDER BY UserID DESC", connection);
        connection.Open();
        using SqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            users.Add(new User
            {
                UserID = Convert.ToInt32(reader["UserID"]),
                Username = reader["Username"].ToString() ?? string.Empty,
                Password = reader["Password"].ToString() ?? string.Empty,
                Role = reader["Role"].ToString() ?? string.Empty
            });
        }
        return users;
    }

    public User? GetUserById(int id)
    {
        using SqlConnection connection = new(_connectionString);
        using SqlCommand command = new("SELECT UserID, Username, Password, Role FROM Users WHERE UserID = @UserID", connection);
        command.Parameters.AddWithValue("@UserID", id);
        connection.Open();
        using SqlDataReader reader = command.ExecuteReader();
        if (!reader.Read()) return null;
        return new User
        {
            UserID = Convert.ToInt32(reader["UserID"]),
            Username = reader["Username"].ToString() ?? string.Empty,
            Password = reader["Password"].ToString() ?? string.Empty,
            Role = reader["Role"].ToString() ?? string.Empty
        };
    }

    public User? Login(string username, string password)
    {
        const string sql = "SELECT UserID, Username, Password, Role FROM Users WHERE Username = @Username AND Password = @Password";
        using SqlConnection connection = new(_connectionString);
        using SqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@Username", username);
        command.Parameters.AddWithValue("@Password", password);
        connection.Open();
        using SqlDataReader reader = command.ExecuteReader();
        if (!reader.Read()) return null;
        return new User
        {
            UserID = Convert.ToInt32(reader["UserID"]),
            Username = reader["Username"].ToString() ?? string.Empty,
            Password = reader["Password"].ToString() ?? string.Empty,
            Role = reader["Role"].ToString() ?? string.Empty
        };
    }

    public bool UsernameExists(string username, int? ignoreUserId = null)
    {
        string sql = "SELECT COUNT(*) FROM Users WHERE Username = @Username";
        if (ignoreUserId.HasValue) sql += " AND UserID <> @UserID";
        using SqlConnection connection = new(_connectionString);
        using SqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@Username", username);
        if (ignoreUserId.HasValue) command.Parameters.AddWithValue("@UserID", ignoreUserId.Value);
        connection.Open();
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    public int AddUser(User user)
    {
        const string sql = @"INSERT INTO Users (Username, Password, Role)
                             VALUES (@Username, @Password, @Role);
                             SELECT CAST(SCOPE_IDENTITY() AS INT);";
        using SqlConnection connection = new(_connectionString);
        using SqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@Username", user.Username);
        command.Parameters.AddWithValue("@Password", user.Password);
        command.Parameters.AddWithValue("@Role", user.Role);
        connection.Open();
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public bool UpdateUser(User user)
    {
        const string sql = @"UPDATE Users SET Username=@Username, Password=@Password, Role=@Role
                             WHERE UserID=@UserID";
        using SqlConnection connection = new(_connectionString);
        using SqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@Username", user.Username);
        command.Parameters.AddWithValue("@Password", user.Password);
        command.Parameters.AddWithValue("@Role", user.Role);
        command.Parameters.AddWithValue("@UserID", user.UserID);
        connection.Open();
        return command.ExecuteNonQuery() > 0;
    }

    public bool DeleteUser(int id)
    {
        using SqlConnection connection = new(_connectionString);
        using SqlCommand command = new("DELETE FROM Users WHERE UserID=@UserID", connection);
        command.Parameters.AddWithValue("@UserID", id);
        connection.Open();
        return command.ExecuteNonQuery() > 0;
    }
}
