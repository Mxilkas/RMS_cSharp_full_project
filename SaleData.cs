using Microsoft.Data.SqlClient;
using RentalProRMS.Api.Models;

namespace RentalProRMS.Api.Data;

public class SaleData
{
    private readonly string _connectionString;

    public SaleData(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection is missing in appsettings.json.");
    }

    private const string SelectSql = @"SELECT s.SaleID, s.PropertyID, p.PropertyName,
        s.BuyerID, c.FullName AS BuyerName, s.SalePrice, s.PaidAmount, s.Balance,
        s.SaleDate, s.PaymentStatus
        FROM Sales s
        INNER JOIN Properties p ON s.PropertyID=p.PropertyID
        INNER JOIN Customers c ON s.BuyerID=c.CustomerID";

    public List<Sale> GetAllSales()
    {
        List<Sale> sales = new();
        using SqlConnection connection = new(_connectionString);
        using SqlCommand command = new(SelectSql + " ORDER BY s.SaleID DESC", connection);
        connection.Open();
        using SqlDataReader reader = command.ExecuteReader();
        while (reader.Read()) sales.Add(MapSale(reader));
        return sales;
    }

    public Sale? GetSaleById(int id)
    {
        using SqlConnection connection = new(_connectionString);
        using SqlCommand command = new(SelectSql + " WHERE s.SaleID=@SaleID", connection);
        command.Parameters.AddWithValue("@SaleID", id);
        connection.Open();
        using SqlDataReader reader = command.ExecuteReader();
        return reader.Read() ? MapSale(reader) : null;
    }

    public int AddSale(Sale sale)
    {
        const string sql = @"INSERT INTO Sales
          (PropertyID, BuyerID, SalePrice, PaidAmount, SaleDate, PaymentStatus)
          VALUES (@PropertyID, @BuyerID, @SalePrice, @PaidAmount, @SaleDate, @PaymentStatus);
          SELECT CAST(SCOPE_IDENTITY() AS INT);";
        using SqlConnection connection = new(_connectionString);
        using SqlCommand command = new(sql, connection);
        AddParameters(command, sale);
        connection.Open();
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public bool UpdateSale(Sale sale)
    {
        const string sql = @"UPDATE Sales SET PropertyID=@PropertyID, BuyerID=@BuyerID,
          SalePrice=@SalePrice, PaidAmount=@PaidAmount, SaleDate=@SaleDate,
          PaymentStatus=@PaymentStatus WHERE SaleID=@SaleID";
        using SqlConnection connection = new(_connectionString);
        using SqlCommand command = new(sql, connection);
        AddParameters(command, sale);
        command.Parameters.AddWithValue("@SaleID", sale.SaleID);
        connection.Open();
        return command.ExecuteNonQuery() > 0;
    }

    public bool DeleteSale(int id)
    {
        using SqlConnection connection = new(_connectionString);
        using SqlCommand command = new("DELETE FROM Sales WHERE SaleID=@SaleID", connection);
        command.Parameters.AddWithValue("@SaleID", id);
        connection.Open();
        return command.ExecuteNonQuery() > 0;
    }

    private static Sale MapSale(SqlDataReader reader) => new()
    {
        SaleID = Convert.ToInt32(reader["SaleID"]),
        PropertyID = Convert.ToInt32(reader["PropertyID"]),
        PropertyName = reader["PropertyName"].ToString(),
        BuyerID = Convert.ToInt32(reader["BuyerID"]),
        BuyerName = reader["BuyerName"].ToString(),
        SalePrice = Convert.ToDecimal(reader["SalePrice"]),
        PaidAmount = Convert.ToDecimal(reader["PaidAmount"]),
        Balance = Convert.ToDecimal(reader["Balance"]),
        SaleDate = Convert.ToDateTime(reader["SaleDate"]),
        PaymentStatus = reader["PaymentStatus"].ToString() ?? string.Empty
    };

    private static void AddParameters(SqlCommand command, Sale sale)
    {
        command.Parameters.AddWithValue("@PropertyID", sale.PropertyID);
        command.Parameters.AddWithValue("@BuyerID", sale.BuyerID);
        command.Parameters.AddWithValue("@SalePrice", sale.SalePrice);
        command.Parameters.AddWithValue("@PaidAmount", sale.PaidAmount);
        command.Parameters.AddWithValue("@SaleDate", sale.SaleDate.Date);
        command.Parameters.AddWithValue("@PaymentStatus", sale.PaymentStatus);
    }
}
