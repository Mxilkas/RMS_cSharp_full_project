using Microsoft.Data.SqlClient;
using RentalProRMS.Api.Models;

namespace RentalProRMS.Api.Data;

public class CustomerData
{
    private readonly string _connectionString;

    public CustomerData(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection is missing in appsettings.json.");
    }

    public List<Customer> GetAllCustomers()
    {
        List<Customer> customers = new();
        using SqlConnection connection = new(_connectionString);
        using SqlCommand command = new("SELECT CustomerID, FullName, Phone, Email, Address, CustomerType FROM Customers ORDER BY CustomerID DESC", connection);
        connection.Open();
        using SqlDataReader reader = command.ExecuteReader();
        while (reader.Read()) customers.Add(MapCustomer(reader));
        return customers;
    }

    public Customer? GetCustomerById(int id)
    {
        using SqlConnection connection = new(_connectionString);
        using SqlCommand command = new("SELECT CustomerID, FullName, Phone, Email, Address, CustomerType FROM Customers WHERE CustomerID=@CustomerID", connection);
        command.Parameters.AddWithValue("@CustomerID", id);
        connection.Open();
        using SqlDataReader reader = command.ExecuteReader();
        return reader.Read() ? MapCustomer(reader) : null;
    }

    public bool PhoneExists(string phone, int? ignoreCustomerId = null)
    {
        string sql = "SELECT COUNT(*) FROM Customers WHERE Phone=@Phone";
        if (ignoreCustomerId.HasValue) sql += " AND CustomerID<>@CustomerID";
        using SqlConnection connection = new(_connectionString);
        using SqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@Phone", phone);
        if (ignoreCustomerId.HasValue) command.Parameters.AddWithValue("@CustomerID", ignoreCustomerId.Value);
        connection.Open();
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    public int AddCustomer(Customer customer)
    {
        const string sql = @"INSERT INTO Customers (FullName, Phone, Email, Address, CustomerType)
                             VALUES (@FullName, @Phone, @Email, @Address, @CustomerType);
                             SELECT CAST(SCOPE_IDENTITY() AS INT);";
        using SqlConnection connection = new(_connectionString);
        using SqlCommand command = new(sql, connection);
        AddParameters(command, customer);
        connection.Open();
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public bool UpdateCustomer(Customer customer)
    {
        const string sql = @"UPDATE Customers SET FullName=@FullName, Phone=@Phone, Email=@Email,
                             Address=@Address, CustomerType=@CustomerType WHERE CustomerID=@CustomerID";
        using SqlConnection connection = new(_connectionString);
        using SqlCommand command = new(sql, connection);
        AddParameters(command, customer);
        command.Parameters.AddWithValue("@CustomerID", customer.CustomerID);
        connection.Open();
        return command.ExecuteNonQuery() > 0;
    }

    public bool DeleteCustomer(int id)
    {
        using SqlConnection connection = new(_connectionString);
        using SqlCommand command = new("DELETE FROM Customers WHERE CustomerID=@CustomerID", connection);
        command.Parameters.AddWithValue("@CustomerID", id);
        connection.Open();
        return command.ExecuteNonQuery() > 0;
    }

    private static Customer MapCustomer(SqlDataReader reader) => new()
    {
        CustomerID = Convert.ToInt32(reader["CustomerID"]),
        FullName = reader["FullName"].ToString() ?? string.Empty,
        Phone = reader["Phone"].ToString() ?? string.Empty,
        Email = reader["Email"] == DBNull.Value ? null : reader["Email"].ToString(),
        Address = reader["Address"] == DBNull.Value ? null : reader["Address"].ToString(),
        CustomerType = reader["CustomerType"].ToString() ?? string.Empty
    };

    private static void AddParameters(SqlCommand command, Customer customer)
    {
        command.Parameters.AddWithValue("@FullName", customer.FullName);
        command.Parameters.AddWithValue("@Phone", customer.Phone);
        command.Parameters.AddWithValue("@Email", (object?)customer.Email ?? DBNull.Value);
        command.Parameters.AddWithValue("@Address", (object?)customer.Address ?? DBNull.Value);
        command.Parameters.AddWithValue("@CustomerType", customer.CustomerType);
    }
}
