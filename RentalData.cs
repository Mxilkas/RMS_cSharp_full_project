using Microsoft.Data.SqlClient;
using RentalProRMS.Api.Models;

namespace RentalProRMS.Api.Data;

public class RentalData
{
    private readonly string _connectionString;

    public RentalData(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection is missing in appsettings.json.");
    }

    private const string SelectSql = @"SELECT r.RentalID, r.PropertyID, p.PropertyName,
        r.CustomerID, c.FullName AS CustomerName, r.RentAmount, r.Deposit,
        r.StartDate, r.EndDate, r.PaymentStatus
        FROM Rentals r
        INNER JOIN Properties p ON r.PropertyID=p.PropertyID
        INNER JOIN Customers c ON r.CustomerID=c.CustomerID";

    public List<Rental> GetAllRentals()
    {
        List<Rental> rentals = new();
        using SqlConnection connection = new(_connectionString);
        using SqlCommand command = new(SelectSql + " ORDER BY r.RentalID DESC", connection);
        connection.Open();
        using SqlDataReader reader = command.ExecuteReader();
        while (reader.Read()) rentals.Add(MapRental(reader));
        return rentals;
    }

    public Rental? GetRentalById(int id)
    {
        using SqlConnection connection = new(_connectionString);
        using SqlCommand command = new(SelectSql + " WHERE r.RentalID=@RentalID", connection);
        command.Parameters.AddWithValue("@RentalID", id);
        connection.Open();
        using SqlDataReader reader = command.ExecuteReader();
        return reader.Read() ? MapRental(reader) : null;
    }

    public int AddRental(Rental rental)
    {
        const string sql = @"INSERT INTO Rentals
          (PropertyID, CustomerID, RentAmount, Deposit, StartDate, EndDate, PaymentStatus)
          VALUES (@PropertyID, @CustomerID, @RentAmount, @Deposit, @StartDate, @EndDate, @PaymentStatus);
          SELECT CAST(SCOPE_IDENTITY() AS INT);";
        using SqlConnection connection = new(_connectionString);
        using SqlCommand command = new(sql, connection);
        AddParameters(command, rental);
        connection.Open();
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public bool UpdateRental(Rental rental)
    {
        const string sql = @"UPDATE Rentals SET PropertyID=@PropertyID, CustomerID=@CustomerID,
          RentAmount=@RentAmount, Deposit=@Deposit, StartDate=@StartDate, EndDate=@EndDate,
          PaymentStatus=@PaymentStatus WHERE RentalID=@RentalID";
        using SqlConnection connection = new(_connectionString);
        using SqlCommand command = new(sql, connection);
        AddParameters(command, rental);
        command.Parameters.AddWithValue("@RentalID", rental.RentalID);
        connection.Open();
        return command.ExecuteNonQuery() > 0;
    }

    public bool DeleteRental(int id)
    {
        using SqlConnection connection = new(_connectionString);
        using SqlCommand command = new("DELETE FROM Rentals WHERE RentalID=@RentalID", connection);
        command.Parameters.AddWithValue("@RentalID", id);
        connection.Open();
        return command.ExecuteNonQuery() > 0;
    }

    private static Rental MapRental(SqlDataReader reader) => new()
    {
        RentalID = Convert.ToInt32(reader["RentalID"]),
        PropertyID = Convert.ToInt32(reader["PropertyID"]),
        PropertyName = reader["PropertyName"].ToString(),
        CustomerID = Convert.ToInt32(reader["CustomerID"]),
        CustomerName = reader["CustomerName"].ToString(),
        RentAmount = Convert.ToDecimal(reader["RentAmount"]),
        Deposit = Convert.ToDecimal(reader["Deposit"]),
        StartDate = Convert.ToDateTime(reader["StartDate"]),
        EndDate = Convert.ToDateTime(reader["EndDate"]),
        PaymentStatus = reader["PaymentStatus"].ToString() ?? string.Empty
    };

    private static void AddParameters(SqlCommand command, Rental rental)
    {
        command.Parameters.AddWithValue("@PropertyID", rental.PropertyID);
        command.Parameters.AddWithValue("@CustomerID", rental.CustomerID);
        command.Parameters.AddWithValue("@RentAmount", rental.RentAmount);
        command.Parameters.AddWithValue("@Deposit", rental.Deposit);
        command.Parameters.AddWithValue("@StartDate", rental.StartDate.Date);
        command.Parameters.AddWithValue("@EndDate", rental.EndDate.Date);
        command.Parameters.AddWithValue("@PaymentStatus", rental.PaymentStatus);
    }
}
