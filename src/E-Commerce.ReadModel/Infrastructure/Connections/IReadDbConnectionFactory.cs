using Microsoft.Data.SqlClient;

namespace E_Commerce.ReadModel.Infrastructure.Connections;

/// <summary>
/// Creates open SQL Server connections for ReadModel queries.
/// </summary>
public interface IReadDbConnectionFactory
{
    Task<SqlConnection> CreateOpenConnectionAsync(
        CancellationToken cancellationToken = default);
}