#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace SqlQueryAnalyzer.Utilities;

/// <summary>
/// Provides validation helpers for <see cref="DatabaseConnectionValidatorExtensions"/> extension methods.
/// Validates parameters passed to extension methods to ensure they meet requirements.
/// </summary>
public static class DatabaseConnectionValidatorExtensionsValidation
{
    /// <summary>
    /// Validates the parameters passed to <see cref="DatabaseConnectionValidatorExtensions.ValidateConnectionAsync"/> and
    /// <see cref="DatabaseConnectionValidatorExtensions.ValidateFormatOnlyAsync"/> extension methods.
    /// </summary>
    /// <param name="validator">The database connection validator instance.</param>
    /// <param name="connectionString">The connection string to validate.</param>
    /// <param name="databaseType">The database type (SqlServer, PostgreSQL, MySQL). Defaults to SqlServer.</param>
    /// <returns>List of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when validator is null.</exception>
    /// <exception cref="ArgumentException">Thrown when connectionString is null or empty.</exception>
    public static IReadOnlyList<string> Validate(
        this DatabaseConnectionValidator? validator,
        string? connectionString,
        string? databaseType = null)
    {
        ArgumentNullException.ThrowIfNull(validator);
        ArgumentException.ThrowIfNullOrEmpty(connectionString);

        var problems = new List<string>();

        // Validate connection string is not whitespace
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            problems.Add("Connection string cannot be null, empty, or whitespace.");
        }

        // Validate database type if provided
        if (!string.IsNullOrWhiteSpace(databaseType))
        {
            var normalizedType = databaseType.Trim().ToLowerInvariant();
            if (normalizedType is not ("sqlserver" or "postgresql" or "mysql"))
            {
                problems.Add("Database type must be SqlServer, PostgreSQL, or MySQL.");
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates the parameters passed to <see cref="DatabaseConnectionValidatorExtensions.GetErrorSummary"/>,
    /// <see cref="DatabaseConnectionValidatorExtensions.IsConnectionSuccessful"/>,
    /// <see cref="DatabaseConnectionValidatorExtensions.GetFormattedVersion"/>,
    /// and <see cref="DatabaseConnectionValidatorExtensions.GenerateDiagnosticReport"/> extension methods.
    /// </summary>
    /// <param name="validator">The database connection validator instance.</param>
    /// <param name="validationResult">The connection validation result to check.</param>
    /// <returns>List of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when validator is null.</exception>
    /// <exception cref="ArgumentNullException">Thrown when validationResult is null.</exception>
    public static IReadOnlyList<string> Validate(
        this DatabaseConnectionValidator? validator,
        ConnectionValidationResult? validationResult)
    {
        ArgumentNullException.ThrowIfNull(validator);
        ArgumentNullException.ThrowIfNull(validationResult);

        var problems = new List<string>();

        // ConnectionValidationResult validation
        if (validationResult.Errors is null)
        {
            problems.Add("ConnectionValidationResult.Errors cannot be null.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if the parameters passed to extension methods are valid.
    /// </summary>
    /// <param name="validator">The database connection validator instance.</param>
    /// <param name="connectionString">The connection string to validate.</param>
    /// <param name="databaseType">The database type (SqlServer, PostgreSQL, MySQL).</param>
    /// <returns>True if valid; false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when validator is null.</exception>
    /// <exception cref="ArgumentException">Thrown when connectionString is null or empty.</exception>
    public static bool IsValid(
        this DatabaseConnectionValidator? validator,
        string? connectionString,
        string? databaseType = null)
    {
        return validator.Validate(connectionString, databaseType).Count == 0;
    }

    /// <summary>
    /// Checks if the parameters passed to extension methods are valid.
    /// </summary>
    /// <param name="validator">The database connection validator instance.</param>
    /// <param name="validationResult">The connection validation result to check.</param>
    /// <returns>True if valid; false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when validator is null.</exception>
    /// <exception cref="ArgumentNullException">Thrown when validationResult is null.</exception>
    public static bool IsValid(
        this DatabaseConnectionValidator? validator,
        ConnectionValidationResult? validationResult)
    {
        return validator.Validate(validationResult).Count == 0;
    }

    /// <summary>
    /// Ensures the parameters passed to extension methods are valid.
    /// Throws <see cref="ArgumentException"/> if validation fails.
    /// </summary>
    /// <param name="validator">The database connection validator instance.</param>
    /// <param name="connectionString">The connection string to validate.</param>
    /// <param name="databaseType">The database type (SqlServer, PostgreSQL, MySQL).</param>
    /// <exception cref="ArgumentNullException">Thrown when validator is null.</exception>
    /// <exception cref="ArgumentException">Thrown when connectionString is null or empty, or when validation fails.</exception>
    public static void EnsureValid(
        this DatabaseConnectionValidator? validator,
        string? connectionString,
        string? databaseType = null)
    {
        var problems = validator.Validate(connectionString, databaseType);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Validation failed for DatabaseConnectionValidatorExtensions extension method parameters:\n- {
                    string.Join("\n- ", problems)
                }");
        }
    }

    /// <summary>
    /// Ensures the parameters passed to extension methods are valid.
    /// Throws <see cref="ArgumentException"/> if validation fails.
    /// </summary>
    /// <param name="validator">The database connection validator instance.</param>
    /// <param name="validationResult">The connection validation result to check.</param>
    /// <exception cref="ArgumentNullException">Thrown when validator is null.</exception>
    /// <exception cref="ArgumentNullException">Thrown when validationResult is null.</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
    public static void EnsureValid(
        this DatabaseConnectionValidator? validator,
        ConnectionValidationResult? validationResult)
    {
        var problems = validator.Validate(validationResult);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Validation failed for DatabaseConnectionValidatorExtensions extension method parameters:\n- {
                    string.Join("\n- ", problems)
                }");
        }
    }
}
