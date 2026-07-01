using Microsoft.Data.SqlClient;
using RentalProRMS.Api.Models;

namespace RentalProRMS.Api.Data;

public class OwnerData
{
    private readonly string _connectionString;

    public OwnerData(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection is missing in appsettings.json.");
    }

    public List<Owner> GetAllOwners()
    {
        List<Owner> owners = new();
        using SqlConnection connection = new(_connectionString);
        using SqlCommand command = new("SELECT OwnerID, FullName, Phone, Email, Address FROM Owners ORDER BY OwnerID DESC", connection);
        connection.Open();
        using SqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            owners.Add(MapOwner(reader));
        }
        return owners;
    }

    public Owner? GetOwnerById(int id)
    {
        using SqlConnection connection = new(_connectionString);
        using SqlCommand command = new("SELECT OwnerID, FullName, Phone, Email, Address FROM Owners WHERE OwnerID=@OwnerID", connection);
        command.Parameters.AddWithValue("@OwnerID", id);
        connection.Open();
        using SqlDataReader reader = command.ExecuteReader();
        return reader.Read() ? MapOwner(reader) : null;
    }

    public bool PhoneExists(string phone, int? ignoreOwnerId = null)
    {
        string sql = "SELECT COUNT(*) FROM Owners WHERE Phone=@Phone";
        if (ignoreOwnerId.HasValue) sql += " AND OwnerID<>@OwnerID";
        using SqlConnection connection = new(_connectionString);
        using SqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@Phone", phone);
        if (ignoreOwnerId.HasValue) command.Parameters.AddWithValue("@OwnerID", ignoreOwnerId.Value);
        connection.Open();
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    public int AddOwner(Owner owner)
    {
        const string sql = @"INSERT INTO Owners (FullName, Phone, Email, Address)
                             VALUES (@FullName, @Phone, @Email, @Address);
                             SELECT CAST(SCOPE_IDENTITY() AS INT);";
        using SqlConnection connection = new(_connectionString);
        using SqlCommand command = new(sql, connection);
        AddParameters(command, owner);
        connection.Open();
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public bool UpdateOwner(Owner owner)
    {
        const string sql = @"UPDATE Owners SET FullName=@FullName, Phone=@Phone, Email=@Email, Address=@Address
                             WHERE OwnerID=@OwnerID";
        using SqlConnection connection = new(_connectionString);
        using SqlCommand command = new(sql, connection);
        AddParameters(command, owner);
        command.Parameters.AddWithValue("@OwnerID", owner.OwnerID);
        connection.Open();
        return command.ExecuteNonQuery() > 0;
    }

    public bool DeleteOwner(int id)
    {
        using SqlConnection connection = new(_connectionString);
        using SqlCommand command = new("DELETE FROM Owners WHERE OwnerID=@OwnerID", connection);
        command.Parameters.AddWithValue("@OwnerID", id);
        connection.Open();
        return command.ExecuteNonQuery() > 0;
    }

    private static Owner MapOwner(SqlDataReader reader) => new()
    {
        OwnerID = Convert.ToInt32(reader["OwnerID"]),
        FullName = reader["FullName"].ToString() ?? string.Empty,
        Phone = reader["Phone"].ToString() ?? string.Empty,
        Email = reader["Email"] == DBNull.Value ? null : reader["Email"].ToString(),
        Address = reader["Address"] == DBNull.Value ? null : reader["Address"].ToString()
    };

    private static void AddParameters(SqlCommand command, Owner owner)
    {
        command.Parameters.AddWithValue("@FullName", owner.FullName);
        command.Parameters.AddWithValue("@Phone", owner.Phone);
        command.Parameters.AddWithValue("@Email", (object?)owner.Email ?? DBNull.Value);
        command.Parameters.AddWithValue("@Address", (object?)owner.Address ?? DBNull.Value);
    }
}
