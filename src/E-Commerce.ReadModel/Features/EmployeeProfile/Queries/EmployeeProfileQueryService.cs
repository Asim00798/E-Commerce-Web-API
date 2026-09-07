using E_Commerce.ReadModel.Features.EmployeeProfile.Models;
using E_Commerce.ReadModel.Infrastructure.Connections;
using Microsoft.Data.SqlClient;
using System.Data;

namespace E_Commerce.ReadModel.Features.EmployeeProfile.Queries;

public sealed class EmployeeProfileQueryService : IEmployeeProfileQueryService
{
    private readonly IReadDbConnectionFactory _connectionFactory;

    public EmployeeProfileQueryService(IReadDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<EmployeeProfileReadModel?> GetEmployeeProfileAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new SqlCommand("dbo.GetEmployeeProfile", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.Add(new SqlParameter("@EmployeeId", employeeId));

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return new EmployeeProfileReadModel
        {
            EmployeeId = reader.GetGuid(reader.GetOrdinal("EmployeeId")),
            FullName = reader.GetString(reader.GetOrdinal("FullName")),
            Department = reader.GetString(reader.GetOrdinal("Department")),
            ActiveShipments = reader.GetInt32(reader.GetOrdinal("ActiveShipments")),
            CompletedShipments = reader.GetInt32(reader.GetOrdinal("CompletedShipments")),
            AverageRating = reader.GetDouble(reader.GetOrdinal("AverageRating")),
            LastActiveAt = reader.IsDBNull(reader.GetOrdinal("LastActiveAt"))
                ? null
                : reader.GetDateTime(reader.GetOrdinal("LastActiveAt"))
        };
    }
}