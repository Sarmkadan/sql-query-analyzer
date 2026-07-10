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
/// Extension methods for DatabaseConnectionValidator providing additional functionality
/// for connection validation, diagnostics, and result processing.
/// </summary>
public static class DatabaseConnectionValidatorExtensions
{
    /// <summary>
    /// Validates a connection string and returns a detailed validation result.
    /// This extension provides a simplified API for common validation scenarios.
    /// </summary>
    /// <param name="validator">The validator instance</param>
    /// <param name="connectionString">Database connection string</param>
    /// <param name="databaseType">Type of database (SqlServer, PostgreSQL, MySQL)</param>
    /// <returns>Connection validation result with detailed information</returns>
    public static async Task<ConnectionValidationResult> ValidateConnectionAsync(
        this DatabaseConnectionValidator validator,
        string connectionString,
        string databaseType = "SqlServer")
    {
        if (validator == null)
            throw new ArgumentNullException(nameof(validator));

        return await validator.ValidateAsync(connectionString, databaseType, testConnection: true);
    }

    /// <summary>
    /// Validates a connection string format only (no actual connection test).
    /// Quick validation for connection strings without network overhead.
    /// </summary>
    /// <param name="validator">The validator instance</param>
    /// <param name="connectionString">Database connection string</param>
    /// <param name="databaseType">Type of database (SqlServer, PostgreSQL, MySQL)</param>
    /// <returns>True if format is valid, false otherwise</returns>
    public static async Task<bool> ValidateFormatOnlyAsync(
        this DatabaseConnectionValidator validator,
        string connectionString,
        string databaseType = "SqlServer")
    {
        if (validator == null)
            throw new ArgumentNullException(nameof(validator));

        var result = await validator.ValidateAsync(connectionString, databaseType, testConnection: false);
        return result.IsValid;
    }

    /// <summary>
    /// Gets a summary of validation errors formatted for display.
    /// Returns a clean error message or empty string if validation succeeded.
    /// </summary>
    /// <param name="validator">The validator instance</param>
    /// <param name="validationResult">The validation result to check</param>
    /// <returns>Formatted error summary</returns>
    public static string GetErrorSummary(
        this DatabaseConnectionValidator validator,
        ConnectionValidationResult validationResult)
    {
        if (validator == null)
            throw new ArgumentNullException(nameof(validator));

        if (validationResult == null)
            throw new ArgumentNullException(nameof(validationResult));

        if (validationResult.Errors == null || validationResult.Errors.Count == 0)
            return string.Empty;

        return "Connection validation failed:\n" +
               string.Join("\n", validationResult.Errors.Select((error, index) =>
                   $"  {index + 1}. {error}"));
    }

    /// <summary>
    /// Checks if the connection validation was successful.
    /// Combines IsValid and IsConnectionAlive checks for convenience.
    /// </summary>
    /// <param name="validator">The validator instance</param>
    /// <param name="validationResult">The validation result to check</param>
    /// <returns>True if connection is valid and alive, false otherwise</returns>
    public static bool IsConnectionSuccessful(
        this DatabaseConnectionValidator validator,
        ConnectionValidationResult validationResult)
    {
        if (validator == null)
            throw new ArgumentNullException(nameof(validator));

        if (validationResult == null)
            throw new ArgumentNullException(nameof(validationResult));

        return validationResult.IsValid && validationResult.IsConnectionAlive;
    }

    /// <summary>
    /// Gets a formatted database version string for display.
    /// Returns a user-friendly version string or a default message if version is empty.
    /// </summary>
    /// <param name="validator">The validator instance</param>
    /// <param name="validationResult">The validation result</param>
    /// <returns>Formatted version string</returns>
    public static string GetFormattedVersion(
        this DatabaseConnectionValidator validator,
        ConnectionValidationResult validationResult)
    {
        if (validator == null)
            throw new ArgumentNullException(nameof(validator));

        if (validationResult == null)
            throw new ArgumentNullException(nameof(validationResult));

        if (string.IsNullOrWhiteSpace(validationResult.DatabaseVersion))
            return "Unknown version";

        return validationResult.DatabaseVersion;
    }

    /// <summary>
    /// Creates a comprehensive diagnostic report from the validation result.
    /// Includes success status, version, message, and any errors.
    /// </summary>
    /// <param name="validator">The validator instance</param>
    /// <param name="validationResult">The validation result</param>
    /// <returns>Formatted diagnostic report string</returns>
    public static string GenerateDiagnosticReport(
        this DatabaseConnectionValidator validator,
        ConnectionValidationResult validationResult)
    {
        if (validator == null)
            throw new ArgumentNullException(nameof(validator));

        if (validationResult == null)
            throw new ArgumentNullException(nameof(validationResult));

        var report = new System.Text.StringBuilder();
        report.AppendLine("=== Database Connection Diagnostic Report ===");
        report.AppendLine();

        report.AppendLine($"Valid: {(validationResult.IsValid ? "✓ YES" : "✗ NO")}");
        report.AppendLine($"Connection Alive: {(validationResult.IsConnectionAlive ? "✓ YES" : "✗ NO")}");
        report.AppendLine($"Success: {(validationResult.IsValid ? "✓ YES" : "✗ NO")}");

        if (!string.IsNullOrWhiteSpace(validationResult.DatabaseVersion))
        {
            report.AppendLine($"Database Version: {validator.GetFormattedVersion(validationResult)}");
        }

        if (!string.IsNullOrWhiteSpace(validationResult.Message))
        {
            report.AppendLine($"Message: {validationResult.Message}");
        }

        if (validationResult.Errors != null && validationResult.Errors.Count > 0)
        {
            report.AppendLine();
            report.AppendLine("Errors:");
            foreach (var error in validationResult.Errors)
            {
                report.AppendLine($"  - {error}");
            }
        }

        report.AppendLine();
        report.AppendLine("=== End of Report ===");

        return report.ToString();
    }
}