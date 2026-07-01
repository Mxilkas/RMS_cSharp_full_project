using Microsoft.Data.SqlClient;
using RentalProRMS.Api.Models;

namespace RentalProRMS.Api.Data;

public class PaymentData
{
    private readonly string _connectionString;

    public PaymentData(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection is missing in appsettings.json.");
    }

    private const string SelectSql = @"SELECT pay.PaymentID, pay.CustomerID, c.FullName AS CustomerName,
        pay.PropertyID, p.PropertyName, pay.PaymentType, pay.TotalAmount, pay.PaidAmount,
        pay.RemainingBalance, pay.PaymentDate, pay.Note
        FROM Payments pay
        INNER JOIN Customers c ON pay.CustomerID=c.CustomerID
        INNER JOIN Properties p ON pay.PropertyID=p.PropertyID";

    public List<Payment> GetAllPayments()
    {
        List<Payment> payments = new();
        using SqlConnection connection = new(_connectionString);
        using SqlCommand command = new(SelectSql + " ORDER BY pay.PaymentID DESC", connection);
        connection.Open();
        using SqlDataReader reader = command.ExecuteReader();
        while (reader.Read()) payments.Add(MapPayment(reader));
        return payments;
    }

    public Payment? GetPaymentById(int id)
    {
        using SqlConnection connection = new(_connectionString);
        using SqlCommand command = new(SelectSql + " WHERE pay.PaymentID=@PaymentID", connection);
        command.Parameters.AddWithValue("@PaymentID", id);
        connection.Open();
        using SqlDataReader reader = command.ExecuteReader();
        return reader.Read() ? MapPayment(reader) : null;
    }

    public int AddPayment(Payment payment)
    {
        const string sql = @"INSERT INTO Payments
          (CustomerID, PropertyID, PaymentType, TotalAmount, PaidAmount, PaymentDate, Note)
          VALUES (@CustomerID, @PropertyID, @PaymentType, @TotalAmount, @PaidAmount, @PaymentDate, @Note);
          SELECT CAST(SCOPE_IDENTITY() AS INT);";
        using SqlConnection connection = new(_connectionString);
        using SqlCommand command = new(sql, connection);
        AddParameters(command, payment);
        connection.Open();
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public bool UpdatePayment(Payment payment)
    {
        const string sql = @"UPDATE Payments SET CustomerID=@CustomerID, PropertyID=@PropertyID,
          PaymentType=@PaymentType, TotalAmount=@TotalAmount, PaidAmount=@PaidAmount,
          PaymentDate=@PaymentDate, Note=@Note WHERE PaymentID=@PaymentID";
        using SqlConnection connection = new(_connectionString);
        using SqlCommand command = new(sql, connection);
        AddParameters(command, payment);
        command.Parameters.AddWithValue("@PaymentID", payment.PaymentID);
        connection.Open();
        return command.ExecuteNonQuery() > 0;
    }

    public bool DeletePayment(int id)
    {
        using SqlConnection connection = new(_connectionString);
        using SqlCommand command = new("DELETE FROM Payments WHERE PaymentID=@PaymentID", connection);
        command.Parameters.AddWithValue("@PaymentID", id);
        connection.Open();
        return command.ExecuteNonQuery() > 0;
    }

    private static Payment MapPayment(SqlDataReader reader) => new()
    {
        PaymentID = Convert.ToInt32(reader["PaymentID"]),
        CustomerID = Convert.ToInt32(reader["CustomerID"]),
        CustomerName = reader["CustomerName"].ToString(),
        PropertyID = Convert.ToInt32(reader["PropertyID"]),
        PropertyName = reader["PropertyName"].ToString(),
        PaymentType = reader["PaymentType"].ToString() ?? string.Empty,
        TotalAmount = Convert.ToDecimal(reader["TotalAmount"]),
        PaidAmount = Convert.ToDecimal(reader["PaidAmount"]),
        RemainingBalance = Convert.ToDecimal(reader["RemainingBalance"]),
        PaymentDate = Convert.ToDateTime(reader["PaymentDate"]),
        Note = reader["Note"] == DBNull.Value ? null : reader["Note"].ToString()
    };

    private static void AddParameters(SqlCommand command, Payment payment)
    {
        command.Parameters.AddWithValue("@CustomerID", payment.CustomerID);
        command.Parameters.AddWithValue("@PropertyID", payment.PropertyID);
        command.Parameters.AddWithValue("@PaymentType", payment.PaymentType);
        command.Parameters.AddWithValue("@TotalAmount", payment.TotalAmount);
        command.Parameters.AddWithValue("@PaidAmount", payment.PaidAmount);
        command.Parameters.AddWithValue("@PaymentDate", payment.PaymentDate.Date);
        command.Parameters.AddWithValue("@Note", (object?)payment.Note ?? DBNull.Value);
    }
}
