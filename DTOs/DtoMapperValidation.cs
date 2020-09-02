#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

namespace SqlQueryAnalyzer.DTOs;

/// <summary>
/// Provides validation helpers for <see cref="QueryDetailDto"/> class
/// </summary>
public static class DtoMapperValidation
{
    /// <summary>
    /// Validates all public members of a <see cref="QueryDetailDto"/> instance
    /// </summary>
    /// <param name="value">The QueryDetailDto instance to validate</param>
    /// <returns>A list of validation problems (empty if valid)</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    public static IReadOnlyList<string> Validate(this QueryDetailDto value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate QueryId
        if (string.IsNullOrWhiteSpace(value.QueryId))
        {
            problems.Add("QueryId must not be null or whitespace");
        }

        // Validate QueryText
        if (string.IsNullOrWhiteSpace(value.QueryText))
        {
            problems.Add("QueryText must not be null or whitespace");
        }

        // Validate QueryType
        if (string.IsNullOrWhiteSpace(value.QueryType))
        {
            problems.Add("QueryType must not be null or whitespace");
        }
        else if (!IsValidQueryType(value.QueryType))
        {
            problems.Add($"QueryType '{value.QueryType}' is not a valid query type");
        }

        // Validate TableCount
        if (value.TableCount < 0)
        {
            problems.Add("TableCount must be non-negative");
        }

        // Validate Tables
        if (value.Tables is null)
        {
            problems.Add("Tables collection must not be null");
        }
        else if (value.Tables.Count != value.TableCount)
        {
            problems.Add($"Tables collection count ({value.Tables.Count}) does not match TableCount ({value.TableCount})");
        }
        else
        {
            for (int i = 0; i < value.Tables.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(value.Tables[i]))
                {
                    problems.Add($"Tables[{i}] must not be null or whitespace");
                }
            }
        }

        // Validate JoinCount
        if (value.JoinCount < 0)
        {
            problems.Add("JoinCount must be non-negative");
        }

        // Validate ParameterCount
        if (value.ParameterCount < 0)
        {
            problems.Add("ParameterCount must be non-negative");
        }

        // Validate LineCount
        if (value.LineCount < 0)
        {
            problems.Add("LineCount must be non-negative");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if a QueryDetailDto instance is valid
    /// </summary>
    /// <param name="value">The QueryDetailDto instance to check</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValid(this QueryDetailDto value) => Validate(value).Count == 0;

    /// <summary>
    /// Ensures that a QueryDetailDto instance is valid, throwing an exception if not
    /// </summary>
    /// <param name="value">The QueryDetailDto instance to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    /// <exception cref="ArgumentException">Thrown if validation fails, containing all problems</exception>
    public static void EnsureValid(this QueryDetailDto value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"QueryDetailDto validation failed:{Environment.NewLine}  - {string.Join($"{Environment.NewLine}  - ", problems)}");
        }
    }

    /// <summary>
    /// Validates that a string is a valid query type
    /// </summary>
    /// <param name="queryType">The query type to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    private static bool IsValidQueryType(string queryType)
    {
        return queryType.Equals("SELECT", StringComparison.OrdinalIgnoreCase) ||
               queryType.Equals("INSERT", StringComparison.OrdinalIgnoreCase) ||
               queryType.Equals("UPDATE", StringComparison.OrdinalIgnoreCase) ||
               queryType.Equals("DELETE", StringComparison.OrdinalIgnoreCase) ||
               queryType.Equals("MERGE", StringComparison.OrdinalIgnoreCase);
    }
}