using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Uploader.Infrastructure.Data;

public class DapperContext(IConfiguration configuration)
{
    private readonly string _connectionString = 
        configuration.GetConnectionString("Sqlite")
        ?? throw new ArgumentException("No connection string found");

    public IDbConnection CreateConnection()
        => new SqliteConnection(_connectionString);
}