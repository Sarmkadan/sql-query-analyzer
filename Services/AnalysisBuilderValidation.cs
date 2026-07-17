#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using SqlQueryAnalyzer.DTOs;

namespace SqlQueryAnalyzer.Services;

/// <summary>
/// Provides validation extension methods for <see cref="AnalysisBuilder"/>
/// </summary>
public static class AnalysisBuilderValidation
{
    /// <summary>
    /// Validates the <see cref="AnalysisBuilder"/> instance and returns a list of validation problems.
    /// </summary>
    /// <param name="value">The builder instance to validate</param>
    /// <returns>List of human-readable validation problems; empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static IReadOnlyList<string> Validate(this AnalysisBuilder value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate query text
        if (string.IsNullOrWhiteSpace(value.Build().QueryText))
        {
            errors.Add("Query text is required");
        }

        // Validate application name
        if (string.IsNullOrWhiteSpace(value.Build().ApplicationName))
        {
            errors.Add("Application name is required");
        }

        // Validate procedure name
        if (string.IsNullOrWhiteSpace(value.Build().ProcedureName))
        {
            errors.Add("Procedure name is required");
        }

        // Validate module name
        if (string.IsNullOrWhiteSpace(value.Build().ModuleName))
        {
            errors.Add("Module name is required");
        }

        // Validate execution plan XML
        if (string.IsNullOrWhiteSpace(value.Build().ExecutionPlanXml))
        {
            errors.Add("Execution plan XML is required");
        }

        return errors;
    }

    /// <summary>
    /// Determines whether the <see cref="AnalysisBuilder"/> instance is valid.
    /// </summary>
    /// <param name="value">The builder instance to check</param>
    /// <returns>True if the builder is valid; otherwise false</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static bool IsValid(this AnalysisBuilder value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return !string.IsNullOrWhiteSpace(value.Build().QueryText);
    }

    /// <summary>
    /// Validates the <see cref="AnalysisBuilder"/> instance and throws an <see cref="ArgumentException"/>
    /// if it is not valid.
    /// </summary>
    /// <param name="value">The builder instance to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown if the builder is not valid, containing all validation errors</exception>
    public static void EnsureValid(this AnalysisBuilder value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate query text
        if (string.IsNullOrWhiteSpace(value.Build().QueryText))
        {
            errors.Add("Query text is required");
        }

        // Validate application name
        if (string.IsNullOrWhiteSpace(value.Build().ApplicationName))
        {
            errors.Add("Application name is required");
        }

        // Validate procedure name
        if (string.IsNullOrWhiteSpace(value.Build().ProcedureName))
        {
            errors.Add("Procedure name is required");
        }

        // Validate module name
        if (string.IsNullOrWhiteSpace(value.Build().ModuleName))
        {
            errors.Add("Module name is required");
        }

        // Validate execution plan XML
        if (string.IsNullOrWhiteSpace(value.Build().ExecutionPlanXml))
        {
            errors.Add("Execution plan XML is required");
        }

        if (errors.Count > 0)
        {
            throw new ArgumentException($"AnalysisBuilder validation failed: {string.Join("; ", errors)}");
        }
    }
}

/// <summary>
/// Provides validation extension methods for <see cref="BatchAnalysisBuilder"/>
/// </summary>
public static class BatchAnalysisBuilderValidation
{
    /// <summary>
    /// Validates the <see cref="BatchAnalysisBuilder"/> instance and returns a list of validation problems.
    /// </summary>
    /// <param name="value">The builder instance to validate</param>
    /// <returns>List of human-readable validation problems; empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static IReadOnlyList<string> Validate(this BatchAnalysisBuilder value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate that queries exist
        if (value.Build().Queries.Count == 0)
        {
            errors.Add("At least one query is required");
        }
        else if (value.Build().Queries.Count > 100)
        {
            errors.Add("Maximum 100 queries per batch");
        }

        // Validate application name
        if (string.IsNullOrWhiteSpace(value.Build().ApplicationName))
        {
            errors.Add("Application name is required");
        }

        // Validate timeout
        if (value.Build().TimeoutSeconds < 1 || value.Build().TimeoutSeconds > 3600)
        {
            errors.Add("Timeout must be between 1 and 3600 seconds");
        }

        return errors;
    }

    /// <summary>
    /// Determines whether the <see cref="BatchAnalysisBuilder"/> instance is valid.
    /// </summary>
    /// <param name="value">The builder instance to check</param>
    /// <returns>True if the builder is valid; otherwise false</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static bool IsValid(this BatchAnalysisBuilder value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Build().Queries.Count > 0;
    }

    /// <summary>
    /// Validates the <see cref="BatchAnalysisBuilder"/> instance and throws an <see cref="ArgumentException"/>
    /// if it is not valid.
    /// </summary>
    /// <param name="value">The builder instance to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown if the builder is not valid, containing all validation errors</exception>
    public static void EnsureValid(this BatchAnalysisBuilder value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate queries count
        if (value.Build().Queries.Count == 0)
        {
            errors.Add("At least one query is required");
        }
        else if (value.Build().Queries.Count > 100)
        {
            errors.Add("Maximum 100 queries per batch");
        }

        // Validate application name
        if (string.IsNullOrWhiteSpace(value.Build().ApplicationName))
        {
            errors.Add("Application name is required");
        }

        // Validate timeout
        if (value.Build().TimeoutSeconds < 1 || value.Build().TimeoutSeconds > 3600)
        {
            errors.Add("Timeout must be between 1 and 3600 seconds");
        }

        if (errors.Count > 0)
        {
            throw new ArgumentException($"BatchAnalysisBuilder validation failed: {string.Join("; ", errors)}");
        }
    }
}

/// <summary>
/// Provides validation extension methods for <see cref="IndexAnalysisBuilder"/>
/// </summary>
public static class IndexAnalysisBuilderValidation
{
    /// <summary>
    /// Validates the <see cref="IndexAnalysisBuilder"/> instance and returns a list of validation problems.
    /// </summary>
    /// <param name="value">The builder instance to validate</param>
    /// <returns>List of human-readable validation problems; empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static IReadOnlyList<string> Validate(this IndexAnalysisBuilder value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate table name
        if (string.IsNullOrWhiteSpace(value.Build().TableName))
        {
            errors.Add("Table name is required");
        }

        return errors;
    }

    /// <summary>
    /// Determines whether the <see cref="IndexAnalysisBuilder"/> instance is valid.
    /// </summary>
    /// <param name="value">The builder instance to check</param>
    /// <returns>True if the builder is valid; otherwise false</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static bool IsValid(this IndexAnalysisBuilder value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return !string.IsNullOrWhiteSpace(value.Build().TableName);
    }

    /// <summary>
    /// Validates the <see cref="IndexAnalysisBuilder"/> instance and throws an <see cref="ArgumentException"/>
    /// if it is not valid.
    /// </summary>
    /// <param name="value">The builder instance to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown if the builder is not valid, containing all validation errors</exception>
    public static void EnsureValid(this IndexAnalysisBuilder value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate table name
        if (string.IsNullOrWhiteSpace(value.Build().TableName))
        {
            errors.Add("Table name is required");
        }

        if (errors.Count > 0)
        {
            throw new ArgumentException($"IndexAnalysisBuilder validation failed: {string.Join("; ", errors)}");
        }
    }
}