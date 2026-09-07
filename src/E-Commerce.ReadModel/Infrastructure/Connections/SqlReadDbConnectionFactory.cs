using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace E_Commerce.ReadModel.Infrastructure.Connections;

public sealed class SqlReadDbConnectionFactory : IReadDbConnectionFactory
{
    private readonly string _connectionString;

    public SqlReadDbConnectionFactory(IConfiguration configuration)
    {
        _connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "ConnectionStrings:DefaultConnection is required.");
    }

    public async Task<SqlConnection> CreateOpenConnectionAsync(
        CancellationToken cancellationToken = default)
    {
        var connection = new SqlConnection(_connectionString);

        await connection.OpenAsync(cancellationToken);

        return connection;
    }
}