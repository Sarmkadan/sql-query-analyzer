#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SqlQueryAnalyzer.Utilities;

/// <summary>
/// Extension methods for <see cref="DatabaseConnectionValidator"/> providing additional functionality
/// for connection validation, diagnostics, and result processing.
/// </summary>
public static class DatabaseConnectionValidatorExtensions
{
    /// <summary>
    /// Validates a connection string and returns a detailed validation result.
    /// This extension provides a simplified API for common validation scenarios.
    /// </summary>
    /// <param name="validator">The validator instance.</param>
    /// <param name="connectionString">Database connection string to validate.</param>
    /// <param name="databaseType">Type of database (SqlServer, PostgreSQL, MySQL). Defaults to SqlServer.</param>
    /// <returns>Connection validation result with detailed information.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="validator"/> is null.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="connectionString"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="connectionString"/> is empty or whitespace.</exception>
    public static async Task<ConnectionValidationResult> ValidateConnectionAsync(
        this DatabaseConnectionValidator validator,
        string connectionString,
        string databaseType = "SqlServer")
    {
        ArgumentNullException.ThrowIfNull(validator);
        ArgumentNullException.ThrowIfNull(connectionString);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        return await validator.ValidateAsync(connectionString, databaseType, testConnection: true);
    }

    /// <summary>
    /// Validates a connection string format only (no actual connection test).
    /// Quick validation for connection strings without network overhead.
    /// </summary>
    /// <param name="validator">The validator instance.</param>
    /// <param name="connectionString">Database connection string to validate.</param>
    /// <param name="databaseType">Type of database (SqlServer, PostgreSQL, MySQL). Defaults to SqlServer.</param>
    /// <returns>True if format is valid, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="validator"/> is null.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="connectionString"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="connectionString"/> is empty or whitespace.</exception>
    public static async Task<bool> ValidateFormatOnlyAsync(
        this DatabaseConnectionValidator validator,
        string connectionString,
        string databaseType = "SqlServer")
    {
        ArgumentNullException.ThrowIfNull(validator);
        ArgumentNullException.ThrowIfNull(connectionString);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        var result = await validator.ValidateAsync(connectionString, databaseType, testConnection: false);
        return result.IsValid;
    }

    /// <summary>
    /// Gets a summary of validation errors formatted for display.
    /// Returns a clean error message or empty string if validation succeeded.
    /// </summary>
    /// <param name="validator">The validator instance.</param>
    /// <param name="validationResult">The validation result to check.</param>
    /// <returns>Formatted error summary.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="validator"/> is null.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="validationResult"/> is null.</exception>
    public static string GetErrorSummary(
        this DatabaseConnectionValidator validator,
        ConnectionValidationResult validationResult)
    {
        ArgumentNullException.ThrowIfNull(validator);
        ArgumentNullException.ThrowIfNull(validationResult);

        return validationResult.Errors switch
        {
            null => string.Empty,
            { Count: 0 } => string.Empty,
            var errors => "Connection validation failed:\n" +
                string.Join("\n", errors.Select((error, index) => $" {index + 1}. {error}"))
        };
    }

    /// <summary>
    /// Checks if the connection validation was successful.
    /// Combines IsValid and IsConnectionAlive checks for convenience.
    /// </summary>
    /// <param name="validator">The validator instance.</param>
    /// <param name="validationResult">The validation result to check.</param>
    /// <returns>True if connection is valid and alive, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="validator"/> is null.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="validationResult"/> is null.</exception>
    public static bool IsConnectionSuccessful(
        this DatabaseConnectionValidator validator,
        ConnectionValidationResult validationResult)
    {
        ArgumentNullException.ThrowIfNull(validator);
        ArgumentNullException.ThrowIfNull(validationResult);

        return validationResult.IsValid && validationResult.IsConnectionAlive;
    }

    /// <summary>
    /// Gets a formatted database version string for display.
    /// Returns a user-friendly version string or a default message if version is empty.
    /// </summary>
    /// <param name="validator">The validator instance.</param>
    /// <param name="validationResult">The validation result.</param>
    /// <returns>Formatted version string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="validator"/> is null.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="validationResult"/> is null.</exception>
    public static string GetFormattedVersion(
        this DatabaseConnectionValidator validator,
        ConnectionValidationResult validationResult)
    {
        ArgumentNullException.ThrowIfNull(validator);
        ArgumentNullException.ThrowIfNull(validationResult);

        return string.IsNullOrWhiteSpace(validationResult.DatabaseVersion)
            ? "Unknown version"
            : validationResult.DatabaseVersion;
    }

    /// <summary>
    /// Creates a comprehensive diagnostic report from the validation result.
    /// Includes success status, version, message, and any errors.
    /// </summary>
    /// <param name="validator">The validator instance.</param>
    /// <param name="validationResult">The validation result.</param>
    /// <returns>Formatted diagnostic report string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="validator"/> is null.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="validationResult"/> is null.</exception>
    public static string GenerateDiagnosticReport(
        this DatabaseConnectionValidator validator,
        ConnectionValidationResult validationResult)
    {
        ArgumentNullException.ThrowIfNull(validator);
        ArgumentNullException.ThrowIfNull(validationResult);

        var report = new System.Text.StringBuilder();
        report.AppendLine("=== Database Connection Diagnostic Report ===");
        report.AppendLine();

        report.AppendLine($"Valid: {(validationResult.IsValid ? "✓ YES" : "✗ NO")}");
        report.AppendLine($"Connection Alive: {(validationResult.IsConnectionAlive ? "✓ YES" : "✗ NO")}");

        if (validationResult.IsValid)
        {
            report.AppendLine($"Success: ✓ YES");
        }
        else
        {
            report.AppendLine($"Success: ✗ NO");
        }

        if (!string.IsNullOrWhiteSpace(validationResult.DatabaseVersion))
        {
            report.AppendLine($"Database Version: {validator.GetFormattedVersion(validationResult)}");
        }

        if (!string.IsNullOrWhiteSpace(validationResult.Message))
        {
            report.AppendLine($"Message: {validationResult.Message}");
        }

        if (validationResult.Errors is { Count: > 0 })
        {
            report.AppendLine();
            report.AppendLine("Errors:");
            foreach (var error in validationResult.Errors)
            {
                report.AppendLine($" - {error}");
            }
        }

        report.AppendLine();
        report.AppendLine("=== End of Report ===");

        return report.ToString();
    }
}