using Microsoft.Data.SqlClient;
using RentalProRMS.Api.Models;

namespace RentalProRMS.Api.Data;

public class PropertyData
{
    private readonly string _connectionString;

    public PropertyData(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection is missing in appsettings.json.");
    }

    private const string SelectSql = @"SELECT p.PropertyID, p.PropertyName, p.PropertyType, p.Location,
        p.Price, p.Status, p.OwnerID, p.Description, o.FullName AS OwnerName
        FROM Properties p INNER JOIN Owners o ON p.OwnerID=o.OwnerID";

    public List<Property> GetAllProperties()
    {
        List<Property> properties = new();
        using SqlConnection connection = new(_connectionString);
        using SqlCommand command = new(SelectSql + " ORDER BY p.PropertyID DESC", connection);
        connection.Open();
        using SqlDataReader reader = command.ExecuteReader();
        while (reader.Read()) properties.Add(MapProperty(reader));
        return properties;
    }

    public Property? GetPropertyById(int id)
    {
        using SqlConnection connection = new(_connectionString);
        using SqlCommand command = new(SelectSql + " WHERE p.PropertyID=@PropertyID", connection);
        command.Parameters.AddWithValue("@PropertyID", id);
        connection.Open();
        using SqlDataReader reader = command.ExecuteReader();
        return reader.Read() ? MapProperty(reader) : null;
    }

    public bool NameExists(string name, int? ignorePropertyId = null)
    {
        string sql = "SELECT COUNT(*) FROM Properties WHERE PropertyName=@PropertyName";
        if (ignorePropertyId.HasValue) sql += " AND PropertyID<>@PropertyID";
        using SqlConnection connection = new(_connectionString);
        using SqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@PropertyName", name);
        if (ignorePropertyId.HasValue) command.Parameters.AddWithValue("@PropertyID", ignorePropertyId.Value);
        connection.Open();
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    public int AddProperty(Property property)
    {
        const string sql = @"INSERT INTO Properties
          (PropertyName, PropertyType, Location, Price, Status, OwnerID, Description)
          VALUES (@PropertyName, @PropertyType, @Location, @Price, @Status, @OwnerID, @Description);
          SELECT CAST(SCOPE_IDENTITY() AS INT);";
        using SqlConnection connection = new(_connectionString);
        using SqlCommand command = new(sql, connection);
        AddParameters(command, property);
        connection.Open();
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public bool UpdateProperty(Property property)
    {
        const string sql = @"UPDATE Properties SET PropertyName=@PropertyName, PropertyType=@PropertyType,
          Location=@Location, Price=@Price, Status=@Status, OwnerID=@OwnerID, Description=@Description
          WHERE PropertyID=@PropertyID";
        using SqlConnection connection = new(_connectionString);
        using SqlCommand command = new(sql, connection);
        AddParameters(command, property);
        command.Parameters.AddWithValue("@PropertyID", property.PropertyID);
        connection.Open();
        return command.ExecuteNonQuery() > 0;
    }

    public bool UpdateStatus(int propertyId, string status)
    {
        using SqlConnection connection = new(_connectionString);
        using SqlCommand command = new("UPDATE Properties SET Status=@Status WHERE PropertyID=@PropertyID", connection);
        command.Parameters.AddWithValue("@Status", status);
        command.Parameters.AddWithValue("@PropertyID", propertyId);
        connection.Open();
        return command.ExecuteNonQuery() > 0;
    }

    public bool DeleteProperty(int id)
    {
        using SqlConnection connection = new(_connectionString);
        using SqlCommand command = new("DELETE FROM Properties WHERE PropertyID=@PropertyID", connection);
        command.Parameters.AddWithValue("@PropertyID", id);
        connection.Open();
        return command.ExecuteNonQuery() > 0;
    }

    private static Property MapProperty(SqlDataReader reader) => new()
    {
        PropertyID = Convert.ToInt32(reader["PropertyID"]),
        PropertyName = reader["PropertyName"].ToString() ?? string.Empty,
        PropertyType = reader["PropertyType"].ToString() ?? string.Empty,
        Location = reader["Location"].ToString() ?? string.Empty,
        Price = Convert.ToDecimal(reader["Price"]),
        Status = reader["Status"].ToString() ?? string.Empty,
        OwnerID = Convert.ToInt32(reader["OwnerID"]),
        Description = reader["Description"] == DBNull.Value ? null : reader["Description"].ToString(),
        OwnerName = reader["OwnerName"].ToString()
    };

    private static void AddParameters(SqlCommand command, Property property)
    {
        command.Parameters.AddWithValue("@PropertyName", property.PropertyName);
        command.Parameters.AddWithValue("@PropertyType", property.PropertyType);
        command.Parameters.AddWithValue("@Location", property.Location);
        command.Parameters.AddWithValue("@Price", property.Price);
        command.Parameters.AddWithValue("@Status", property.Status);
        command.Parameters.AddWithValue("@OwnerID", property.OwnerID);
        command.Parameters.AddWithValue("@Description", (object?)property.Description ?? DBNull.Value);
    }
}
