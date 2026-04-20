#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using SqlQueryAnalyzer.Exceptions;
using SqlQueryAnalyzer.Models;

namespace SqlQueryAnalyzer.Utilities;

/// <summary>
/// Validates SQL queries and related objects
/// </summary>
public static class QueryValidator
{
    // Check if query text is valid SQL
    public static bool IsValidQuery(string queryText)
    {
        if (string.IsNullOrWhiteSpace(queryText))
            return false;

        var trimmed = queryText.Trim();
        var sqlKeywords = new[] { "SELECT", "INSERT", "UPDATE", "DELETE", "CREATE", "DROP", "EXEC", "CALL" };

        return sqlKeywords.Any(kw => trimmed.StartsWith(kw, StringComparison.OrdinalIgnoreCase));
    }

    // Validate database query object
    public static void ValidateDatabaseQuery(DatabaseQuery query)
    {
        // Fix: Use Argument exceptions instead of generic ValidationException
        if (query == null)
            throw new ArgumentNullException(nameof(query), "Database query object cannot be null.");

        if (string.IsNullOrWhiteSpace(query.QueryText))
            throw new ArgumentException("Database query text cannot be empty.", nameof(query.QueryText));

        if (!IsValidQuery(query.QueryText))
            throw new InvalidQueryException($"Invalid SQL query text provided: '{query.QueryText}'", query.QueryText);

        if (query.LineCount < 1)
            throw new ArgumentOutOfRangeException(nameof(query.LineCount), query.LineCount, "Query must have at least one line.");
    }

    // Validate analysis result
    public static void ValidateAnalysisResult(QueryAnalysisResult result)
    {
        // Fix: Add missing null check and improve exception types
        if (result == null)
            throw new ArgumentNullException(nameof(result), "Query analysis result cannot be null.");

        if (string.IsNullOrWhiteSpace(result.Query))
            throw new ArgumentException("Analysis result must have associated query text.", nameof(result.Query));

        if (result.PerformanceScore < 0 || result.PerformanceScore > 100)
            throw new ArgumentOutOfRangeException(nameof(result.PerformanceScore), result.PerformanceScore, "Performance score must be between 0 and 100.");

        if (result.Issues != null)
        {
            foreach (var issue in result.Issues)
            {
                if (!issue.IsValid())
                    throw new ValidationException($"Invalid issue detected in analysis results: {issue.IssueType}", "Issues");
            }
        }
    }

    // Validate index
    public static void ValidateIndex(Index index)
    {
        if (index == null)
            throw new ValidationException("Index cannot be null", "Index");

        if (string.IsNullOrWhiteSpace(index.IndexName))
            throw new ValidationException("Index name is required", "IndexName");

        if (string.IsNullOrWhiteSpace(index.TableName))
            throw new ValidationException("Table name is required", "TableName");

        if (index.Columns.Count == 0)
            throw new ValidationException("Index must have at least one column", "Columns");
    }

    // Validate index suggestion
    public static void ValidateIndexSuggestion(IndexSuggestion suggestion)
    {
        if (suggestion == null)
            throw new ValidationException("Index suggestion cannot be null", "IndexSuggestion");

        if (string.IsNullOrWhiteSpace(suggestion.TableName))
            throw new ValidationException("Table name is required", "TableName");

        if (suggestion.IndexColumns.Count == 0)
            throw new ValidationException("Index must have at least one column", "IndexColumns");

        if (suggestion.EstimatedPerformanceGain < 0 || suggestion.EstimatedPerformanceGain > 100)
            throw new ValidationException(
                "Performance gain must be between 0 and 100", "EstimatedPerformanceGain");
    }

    // Check for SQL injection patterns
    public static List<string> DetectSQLInjectionRisks(string queryText)
    {
        var risks = new List<string>();

        // Check for common SQL injection patterns
        var injectionPatterns = new[]
        {
            @"('\s*OR\s*'1'\s*=\s*'1)", // Classic OR 1=1
            @"(;\s*DROP\s+)", // DROP commands
            @"(;\s*DELETE\s+)", // DELETE commands
            @"(UNION\s+SELECT)", // UNION-based injection
            @"(\bEXEC\s*\()", // Dynamic execution
            @"(sp_executesql)", // Stored procedure execution
        };

        foreach (var pattern in injectionPatterns)
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(queryText, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            {
                risks.Add($"Potential injection pattern detected: {pattern}");
            }
        }

        return risks;
    }

    // Validate connection configuration
    public static void ValidateConnectionString(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ConfigurationException("Connection string cannot be empty", "ConnectionString");

        if (!connectionString.Contains("Server") && !connectionString.Contains("Host") &&
            !connectionString.Contains("Data Source"))
        {
            throw new ConfigurationException("Connection string must contain server information", "ConnectionString");
        }

        if (!connectionString.Contains("Database") && !connectionString.Contains("Catalog"))
        {
            throw new ConfigurationException("Connection string must specify a database", "ConnectionString");
        }
    }

    // Sanitize query for display
    public static string SanitizeQueryForDisplay(string query, int maxLength = 100)
    {
        if (string.IsNullOrEmpty(query))
            return string.Empty;

        var displayText = query.Replace("\r\n", " ").Replace("\n", " ").Replace("\t", " ");

        if (displayText.Length > maxLength)
            displayText = displayText.Substring(0, maxLength) + "...";

        return displayText;
    }
}
