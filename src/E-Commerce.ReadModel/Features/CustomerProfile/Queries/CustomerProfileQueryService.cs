using System.Data;
using E_Commerce.ReadModel.Features.CustomerProfile.Models;
using E_Commerce.ReadModel.Infrastructure.Connections;
using Microsoft.Data.SqlClient;

namespace E_Commerce.ReadModel.Features.CustomerProfile.Queries;

public sealed class CustomerProfileQueryService : ICustomerProfileQueryService
{
    private readonly IReadDbConnectionFactory _connectionFactory;

    public CustomerProfileQueryService(IReadDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<CustomerProfileReadModel?> GetCustomerProfileAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new SqlCommand("dbo.GetCustomerProfile", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.Add(new SqlParameter("@CustomerId", customerId));

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return new CustomerProfileReadModel
        {
            CustomerId = reader.GetGuid(reader.GetOrdinal("CustomerId")),
            FullName = reader.GetString(reader.GetOrdinal("FullName")),
            Email = reader.GetString(reader.GetOrdinal("Email")),
            PhoneNumber = reader.GetString(reader.GetOrdinal("PhoneNumber")),
            TotalOrders = reader.GetInt32(reader.GetOrdinal("TotalOrders")),
            TotalSpent = reader.GetDecimal(reader.GetOrdinal("TotalSpent")),
            AverageRating = reader.GetDouble(reader.GetOrdinal("AverageRating")),
            WishlistItemCount = reader.GetInt32(reader.GetOrdinal("WishlistItemCount")),
            LastOrderDate = reader.IsDBNull(reader.GetOrdinal("LastOrderDate"))
                ? null
                : reader.GetDateTime(reader.GetOrdinal("LastOrderDate"))
        };
    }
}