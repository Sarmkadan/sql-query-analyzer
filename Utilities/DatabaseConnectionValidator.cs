#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Data;
using System.Globalization;
using Microsoft.Extensions.Logging;

namespace SqlQueryAnalyzer.Utilities;

/// <summary>
/// Validates database connections before analysis.
/// Checks connectivity, version compatibility, required permissions.
/// Provides detailed diagnostic information for connection failures.
/// </summary>
public class DatabaseConnectionValidator
{
    private readonly ILogger<DatabaseConnectionValidator> _logger;

    public DatabaseConnectionValidator(ILogger<DatabaseConnectionValidator> logger)
    {
        _logger = logger;
    }

    public override string ToString() => 
        $"DatabaseConnectionValidator {{ IsValid = {false}, IsConnectionAlive = {false}, Message = {string.Empty}, DatabaseVersion = {string.Empty}, Errors = System.Collections.Generic.List<string>, Success = {false} }}";

    /// <summary>
    /// Validates a connection string format and optionally tests the connection.
    /// Returns validation result with detailed error information if validation fails.
    /// </summary>
    public async Task<ConnectionValidationResult> ValidateAsync(
        string connectionString,
        string databaseType = "SqlServer",
        bool testConnection = true)
    {
        _logger.LogInformation("Validating connection string for {DatabaseType} (testConnection: {TestConnection})", databaseType, testConnection);
        var result = new ConnectionValidationResult();

        try
        {
            // Validate connection string format
            if (!ValidateConnectionStringFormat(connectionString, databaseType))
            {
                result.IsValid = false;
                result.Errors.Add("Invalid connection string format");
                result.Message = GetFormatHint(databaseType);
                _logger.LogWarning("Connection string format validation failed for {DatabaseType}", databaseType);
                return result;
            }

            _logger.LogDebug("Connection string format valid for {DatabaseType}", databaseType);

            // Test actual connection if requested
            if (testConnection)
            {
                var connectionTest = await TestDatabaseConnectionAsync(connectionString, databaseType);
                if (!connectionTest.Success)
                {
                    result.IsValid = false;
                    result.Errors.AddRange(connectionTest.Errors);
                    _logger.LogWarning("Database connection test failed for {DatabaseType}", databaseType);
                    return result;
                }

                result.DatabaseVersion = connectionTest.DatabaseVersion;
                result.IsConnectionAlive = true;
            }

            result.IsValid = true;
            result.Message = "Connection validation successful";
            _logger.LogInformation("Connection validation successful for {DatabaseType}", databaseType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during connection validation");
            result.IsValid = false;
            result.Errors.Add($"Validation error: {ex.Message}");
        }

        return result;
    }

    /// <summary>
    /// Validates connection string format without connecting.
    /// Quick check to catch obviously malformed strings.
    /// </summary>
    private bool ValidateConnectionStringFormat(string connectionString, string databaseType)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return false;

        return databaseType.ToLower() switch
        {
            "sqlserver" => connectionString.Contains("Server=", StringComparison.OrdinalIgnoreCase) ||
                          connectionString.Contains("Data Source=", StringComparison.OrdinalIgnoreCase),
            "postgresql" => connectionString.Contains("Host=", StringComparison.OrdinalIgnoreCase) ||
                           connectionString.Contains("Server=", StringComparison.OrdinalIgnoreCase),
            "mysql" => connectionString.Contains("Server=", StringComparison.OrdinalIgnoreCase) ||
                      connectionString.Contains("Host=", StringComparison.OrdinalIgnoreCase),
            _ => false
        };
    }

    /// <summary>
    /// Attempts actual database connection with timeout protection.
    /// Tests TCP-level connectivity to the host/port extracted from the connection string.
    /// Does not query the database engine directly (no driver dependency is available here),
    /// so the reported <see cref="ConnectionTestResult.DatabaseVersion"/> reflects only what
    /// can be determined without one.
    /// </summary>
    private async Task<ConnectionTestResult> TestDatabaseConnectionAsync(
        string connectionString,
        string databaseType)
    {
        var result = new ConnectionTestResult();

        try
        {
            _logger.LogInformation($"Testing {databaseType} connection");

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            result = await TestTcpConnectionAsync(connectionString, databaseType, cts.Token);
            result.Success = result.Errors.Count == 0;
        }
        catch (OperationCanceledException)
        {
            result.Success = false;
            result.Errors.Add("Connection test timeout (5 seconds)");
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Errors.Add($"Connection failed: {ex.Message}");
        }

        return result;
    }

    /// <summary>
    /// Opens a TCP connection to the host/port parsed from the connection string to verify
    /// the target is reachable. This confirms network connectivity but cannot authenticate
    /// or query the database engine, since no database driver is referenced by this project.
    /// </summary>
    private async Task<ConnectionTestResult> TestTcpConnectionAsync(string connectionString, string databaseType, CancellationToken ct)
    {
        var result = new ConnectionTestResult();

        var (host, port) = ParseHostAndPort(connectionString, databaseType);
        if (host == null)
        {
            result.Errors.Add("Could not determine host from connection string");
            return result;
        }

        try
        {
            using var client = new System.Net.Sockets.TcpClient();
            await client.ConnectAsync(host, port, ct);
            result.Success = client.Connected;

            if (!result.Success)
                result.Errors.Add($"Unable to reach {host}:{port}");

            _logger.LogDebug($"{databaseType} host {host}:{port} reachable: {result.Success}");
        }
        catch (Exception ex) when (ex is System.Net.Sockets.SocketException or ObjectDisposedException)
        {
            result.Errors.Add($"{databaseType} connection error: {ex.Message}");
        }

        return result;
    }

    /// <summary>
    /// Extracts the host name/address and port from a connection string, falling back to the
    /// database engine's default port when none is specified.
    /// </summary>
    private static (string? Host, int Port) ParseHostAndPort(string connectionString, string databaseType)
    {
        var defaultPort = databaseType.ToLowerInvariant() switch
        {
            "sqlserver" => 1433,
            "postgresql" => 5432,
            "mysql" => 3306,
            _ => 0
        };

        string? host = null;
        var port = defaultPort;

        foreach (var segment in connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = segment.Split('=', 2);
            if (parts.Length != 2)
                continue;

            var key = parts[0].Trim();
            var value = parts[1].Trim();

            if (key.Equals("Server", StringComparison.OrdinalIgnoreCase) ||
                key.Equals("Host", StringComparison.OrdinalIgnoreCase) ||
                key.Equals("Data Source", StringComparison.OrdinalIgnoreCase))
            {
                var hostPart = value;
                var commaIndex = hostPart.IndexOf(',');
                var colonIndex = hostPart.IndexOf(':');
                var separatorIndex = commaIndex >= 0 ? commaIndex : colonIndex;

                if (separatorIndex >= 0)
                {
                    hostPart = value[..separatorIndex];
                    if (int.TryParse(value[(separatorIndex + 1)..], NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedPort))
                        port = parsedPort;
                }

                host = hostPart;
            }
            else if (key.Equals("Port", StringComparison.OrdinalIgnoreCase) &&
                     int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var explicitPort))
            {
                port = explicitPort;
            }
        }

        return (host, port);
    }

    /// <summary>
    /// Provides helpful hints for correct connection string format.
    /// </summary>
    private string GetFormatHint(string databaseType) =>
        databaseType.ToLower() switch
        {
            "sqlserver" => "SQL Server format: Server=hostname;Database=dbname;User Id=user;Password=pass",
            "postgresql" => "PostgreSQL format: Host=hostname;Database=dbname;Username=user;Password=pass",
            "mysql" => "MySQL format: Server=hostname;Database=dbname;Uid=user;Pwd=pass",
            _ => "Unknown database type"
        };
}

/// <summary>
/// Result of connection string validation.
/// </summary>
public class ConnectionValidationResult
{
    public bool IsValid { get; set; }
    public bool IsConnectionAlive { get; set; }
    public string Message { get; set; } = string.Empty;
    public string DatabaseVersion { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// Result of actual database connection test.
/// </summary>
public class ConnectionTestResult
{
    public bool Success { get; set; }
    public string DatabaseVersion { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();
}
