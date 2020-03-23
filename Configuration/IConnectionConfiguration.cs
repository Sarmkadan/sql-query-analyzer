#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Threading.Tasks;

namespace SqlQueryAnalyzer.Configuration;

/// <summary>
/// Interface for database connection configuration
/// </summary>
public interface IConnectionConfiguration
{
    string ConnectionString { get; }
    string DatabaseName { get; }
    string ServerName { get; }
    string DatabaseType { get; }
    int CommandTimeout { get; }

    Task<bool> TestConnectionAsync();
    Task<bool> InitializeDatabaseAsync();
}

/// <summary>
/// SQL Server connection configuration
/// </summary>
public class SqlServerConfiguration : IConnectionConfiguration
{
    private const string DefaultServer = "localhost";
    private const string DefaultDatabase = "QueryAnalyzer";
    private const int DefaultTimeout = 30;

    public string ConnectionString { get; private set; }
    public string DatabaseName { get; private set; }
    public string ServerName { get; private set; }
    public string DatabaseType => "SQL Server";
    public int CommandTimeout { get; private set; }

    public SqlServerConfiguration()
    {
        var serverName = Environment.GetEnvironmentVariable("DB_SERVER") ?? DefaultServer;
        var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? DefaultDatabase;
        var userId = Environment.GetEnvironmentVariable("DB_USER") ?? "sa";
        var password = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "YourPassword123!";

        ServerName = serverName;
        DatabaseName = dbName;
        CommandTimeout = int.TryParse(Environment.GetEnvironmentVariable("DB_TIMEOUT"), out var timeout)
            ? timeout
            : DefaultTimeout;

        // Build connection string
        ConnectionString = $"Server={serverName};Database={dbName};User Id={userId};Password={password};Encrypt=false;";
    }

    public SqlServerConfiguration(string server, string database, string userId, string password)
    {
        ServerName = server;
        DatabaseName = database;
        CommandTimeout = DefaultTimeout;
        ConnectionString = $"Server={server};Database={database};User Id={userId};Password={password};Encrypt=false;";
    }

    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            using (var connection = new System.Data.SqlClient.SqlConnection(ConnectionString))
            {
                await connection.OpenAsync().ConfigureAwait(false);
                return connection.State == System.Data.ConnectionState.Open;
            }
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> InitializeDatabaseAsync()
    {
        try
        {
            using (var connection = new System.Data.SqlClient.SqlConnection(ConnectionString))
            {
                await connection.OpenAsync().ConfigureAwait(false);

                var initSql = @"
                    IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'QueryAnalyses')
                    BEGIN
                        CREATE TABLE QueryAnalyses (
                            QueryId NVARCHAR(50) PRIMARY KEY,
                            QueryText NVARCHAR(MAX) NOT NULL,
                            AnalyzedAt DATETIME2 DEFAULT GETUTCDATE(),
                            PerformanceScore DECIMAL(5,2),
                            ComplexityLevel INT
                        );
                    END
                ";

                using (var cmd = new System.Data.SqlClient.SqlCommand(initSql, connection))
                {
                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }

                return true;
            }
        }
        catch
        {
            return false;
        }
    }
}

/// <summary>
/// PostgreSQL connection configuration
/// </summary>
public class PostgresConfiguration : IConnectionConfiguration
{
    private const string DefaultServer = "localhost";
    private const string DefaultDatabase = "query_analyzer";
    private const int DefaultTimeout = 30;

    public string ConnectionString { get; private set; }
    public string DatabaseName { get; private set; }
    public string ServerName { get; private set; }
    public string DatabaseType => "PostgreSQL";
    public int CommandTimeout { get; private set; }

    public PostgresConfiguration()
    {
        var serverName = Environment.GetEnvironmentVariable("DB_SERVER") ?? DefaultServer;
        var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? DefaultDatabase;
        var userId = Environment.GetEnvironmentVariable("DB_USER") ?? "postgres";
        var password = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "postgres";
        var port = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";

        ServerName = serverName;
        DatabaseName = dbName;
        CommandTimeout = int.TryParse(Environment.GetEnvironmentVariable("DB_TIMEOUT"), out var timeout)
            ? timeout
            : DefaultTimeout;

        ConnectionString = $"Host={serverName};Port={port};Database={dbName};Username={userId};Password={password};";
    }

    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            using (var connection = new Npgsql.NpgsqlConnection(ConnectionString))
            {
                await connection.OpenAsync().ConfigureAwait(false);
                return connection.State == System.Data.ConnectionState.Open;
            }
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> InitializeDatabaseAsync()
    {
        try
        {
            using (var connection = new Npgsql.NpgsqlConnection(ConnectionString))
            {
                await connection.OpenAsync().ConfigureAwait(false);

                var initSql = @"
                    CREATE TABLE IF NOT EXISTS query_analyses (
                        query_id VARCHAR(50) PRIMARY KEY,
                        query_text TEXT NOT NULL,
                        analyzed_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                        performance_score DECIMAL(5,2),
                        complexity_level INT
                    );
                ";

                using (var cmd = new Npgsql.NpgsqlCommand(initSql, connection))
                {
                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }

                return true;
            }
        }
        catch
        {
            return false;
        }
    }
}
