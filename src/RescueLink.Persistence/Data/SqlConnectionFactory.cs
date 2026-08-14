using System.Data.Common;
using Microsoft.Data.SqlClient;
using RescueLink.Application.Abstractions.Data;

namespace RescueLink.Persistence.Data;

internal sealed class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        _connectionString = connectionString;
    }

    public DbConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }
}