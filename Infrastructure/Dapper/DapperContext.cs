using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Infrastructure.Dapper;

public class DapperContext
{
    private readonly string _connectionString;

    public DapperContext(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("SecurePayDb")
            ?? throw new InvalidOperationException("Connection string 'SecurePayDb' is not configured.");
    }

    // Every repository calls this to get a fresh open connection
    public IDbConnection CreateConnection()
        => new SqlConnection(_connectionString);
}
