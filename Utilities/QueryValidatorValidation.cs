#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using SqlQueryAnalyzer.Exceptions;
using SqlQueryAnalyzer.Models;
using ModelIndex = SqlQueryAnalyzer.Models.Index;

namespace SqlQueryAnalyzer.Utilities;

/// <summary>
/// Provides validation helpers for <see cref="QueryValidator"/> static methods.
/// Validates all public members and ensures they meet expected constraints.
/// </summary>
public static class QueryValidatorValidation
{
    /// <summary>
    /// Validates the QueryValidator static class by testing its public methods.
    /// Returns a list of human-readable validation problems.
    /// </summary>
    /// <returns>An empty list if valid, otherwise a list of validation errors.</returns>
    public static IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();

        // Validate IsValidQuery behavior
        try
        {
            // Test with null
            if (QueryValidator.IsValidQuery(null!))
            {
                errors.Add("IsValidQuery returned true for null input");
            }

            // Test with empty string
            if (QueryValidator.IsValidQuery(string.Empty))
            {
                errors.Add("IsValidQuery returned true for empty string input");
            }

            // Test with whitespace
            if (QueryValidator.IsValidQuery("   "))
            {
                errors.Add("IsValidQuery returned true for whitespace input");
            }

            // Test with valid SQL
            if (!QueryValidator.IsValidQuery("SELECT * FROM users"))
            {
                errors.Add("IsValidQuery returned false for valid SQL query");
            }

            // Test with invalid SQL
            if (QueryValidator.IsValidQuery("INVALID QUERY"))
            {
                errors.Add("IsValidQuery returned true for invalid SQL query");
            }
        }
        catch (Exception ex)
        {
            errors.Add($"IsValidQuery validation threw exception: {ex.GetType().Name}: {ex.Message}");
        }

        // Validate ValidateDatabaseQuery behavior
        try
        {
            // Test with null
            try
            {
                QueryValidator.ValidateDatabaseQuery(null!);
                errors.Add("ValidateDatabaseQuery did not throw for null input");
            }
            catch (ValidationException) { /* Expected */ }
            catch (Exception ex)
            {
                errors.Add($"ValidateDatabaseQuery threw wrong exception type for null: {ex.GetType().Name}");
            }

            // Test with valid DatabaseQuery
            try
            {
                var validQuery = new DatabaseQuery
                {
                    QueryText = "SELECT * FROM users WHERE id = 1",
                    LineCount = 1
                };
                QueryValidator.ValidateDatabaseQuery(validQuery);
            }
            catch (Exception ex)
            {
                errors.Add($"ValidateDatabaseQuery threw exception for valid DatabaseQuery: {ex.GetType().Name}: {ex.Message}");
            }

            // Test with invalid DatabaseQuery (null query text)
            try
            {
                var invalidQuery = new DatabaseQuery
                {
                    QueryText = null,
                    LineCount = 1
                };
                QueryValidator.ValidateDatabaseQuery(invalidQuery);
                errors.Add("ValidateDatabaseQuery did not throw for null QueryText");
            }
            catch (ArgumentException) { /* Expected */ }
            catch (Exception ex)
            {
                errors.Add($"ValidateDatabaseQuery threw wrong exception type for null QueryText: {ex.GetType().Name}");
            }

            // Test with empty query text
            try
            {
                var invalidQuery = new DatabaseQuery
                {
                    QueryText = "   ",
                    LineCount = 1
                };
                QueryValidator.ValidateDatabaseQuery(invalidQuery);
                errors.Add("ValidateDatabaseQuery did not throw for empty QueryText");
            }
            catch (ArgumentException) { /* Expected */ }
            catch (Exception ex)
            {
                errors.Add($"ValidateDatabaseQuery threw wrong exception type for empty QueryText: {ex.GetType().Name}");
            }

            // Test with invalid query text
            try
            {
                var invalidQuery = new DatabaseQuery
                {
                    QueryText = "INVALID QUERY",
                    LineCount = 1
                };
                QueryValidator.ValidateDatabaseQuery(invalidQuery);
                errors.Add("ValidateDatabaseQuery did not throw for invalid QueryText");
            }
            catch (InvalidQueryException) { /* Expected */ }
            catch (Exception ex)
            {
                errors.Add($"ValidateDatabaseQuery threw wrong exception type for invalid QueryText: {ex.GetType().Name}");
            }

            // Test with zero line count
            try
            {
                var invalidQuery = new DatabaseQuery
                {
                    QueryText = "SELECT 1",
                    LineCount = 0
                };
                QueryValidator.ValidateDatabaseQuery(invalidQuery);
                errors.Add("ValidateDatabaseQuery did not throw for zero LineCount");
            }
            catch (ArgumentOutOfRangeException) { /* Expected */ }
            catch (Exception ex)
            {
                errors.Add($"ValidateDatabaseQuery threw wrong exception type for zero LineCount: {ex.GetType().Name}");
            }
        }
        catch (Exception ex)
        {
            errors.Add($"ValidateDatabaseQuery validation threw exception: {ex.GetType().Name}: {ex.Message}");
        }

        // Validate ValidateAnalysisResult behavior
        try
        {
            // Test with null
            try
            {
                QueryValidator.ValidateAnalysisResult(null!);
                errors.Add("ValidateAnalysisResult did not throw for null input");
            }
            catch (ArgumentNullException) { /* Expected */ }
            catch (Exception ex)
            {
                errors.Add($"ValidateAnalysisResult threw wrong exception type for null: {ex.GetType().Name}");
            }

            // Test with valid QueryAnalysisResult
            try
            {
                var validResult = new QueryAnalysisResult
                {
                    Query = "SELECT * FROM users",
                    PerformanceScore = 85.5,
                    Issues = new List<PerformanceIssue>()
                };
                QueryValidator.ValidateAnalysisResult(validResult);
            }
            catch (Exception ex)
            {
                errors.Add($"ValidateAnalysisResult threw exception for valid QueryAnalysisResult: {ex.GetType().Name}: {ex.Message}");
            }

            // Test with null Query
            try
            {
                var invalidResult = new QueryAnalysisResult
                {
                    Query = null,
                    PerformanceScore = 85.5
                };
                QueryValidator.ValidateAnalysisResult(invalidResult);
                errors.Add("ValidateAnalysisResult did not throw for null Query");
            }
            catch (ArgumentException) { /* Expected */ }
            catch (Exception ex)
            {
                errors.Add($"ValidateAnalysisResult threw wrong exception type for null Query: {ex.GetType().Name}");
            }

            // Test with empty Query
            try
            {
                var invalidResult = new QueryAnalysisResult
                {
                    Query = "   ",
                    PerformanceScore = 85.5
                };
                QueryValidator.ValidateAnalysisResult(invalidResult);
                errors.Add("ValidateAnalysisResult did not throw for empty Query");
            }
            catch (ArgumentException) { /* Expected */ }
            catch (Exception ex)
            {
                errors.Add($"ValidateAnalysisResult threw wrong exception type for empty Query: {ex.GetType().Name}");
            }

            // Test with out of range PerformanceScore
            try
            {
                var invalidResult = new QueryAnalysisResult
                {
                    Query = "SELECT * FROM users",
                    PerformanceScore = 150
                };
                QueryValidator.ValidateAnalysisResult(invalidResult);
                errors.Add("ValidateAnalysisResult did not throw for out of range PerformanceScore");
            }
            catch (ArgumentOutOfRangeException) { /* Expected */ }
            catch (Exception ex)
            {
                errors.Add($"ValidateAnalysisResult threw wrong exception type for out of range PerformanceScore: {ex.GetType().Name}");
            }
        }
        catch (Exception ex)
        {
            errors.Add($"ValidateAnalysisResult validation threw exception: {ex.GetType().Name}: {ex.Message}");
        }

        // Validate ValidateIndex behavior
        try
        {
            // Test with null
            try
            {
                QueryValidator.ValidateIndex(null!);
                errors.Add("ValidateIndex did not throw for null input");
            }
            catch (ValidationException) { /* Expected */ }
            catch (Exception ex)
            {
                errors.Add($"ValidateIndex threw wrong exception type for null: {ex.GetType().Name}");
            }

            // Test with valid Index
            try
            {
                var validIndex = new ModelIndex
                {
                    IndexName = "IX_Users_Email",
                    TableName = "Users",
                    Columns = new List<IndexColumn> { new IndexColumn { ColumnName = "Email" } }
                };
                QueryValidator.ValidateIndex(validIndex);
            }
            catch (Exception ex)
            {
                errors.Add($"ValidateIndex threw exception for valid Index: {ex.GetType().Name}: {ex.Message}");
            }

            // Test with null IndexName
            try
            {
                var invalidIndex = new ModelIndex
                {
                    IndexName = null,
                    TableName = "Users",
                    Columns = new List<IndexColumn> { new IndexColumn { ColumnName = "Email" } }
                };
                QueryValidator.ValidateIndex(invalidIndex);
                errors.Add("ValidateIndex did not throw for null IndexName");
            }
            catch (ValidationException) { /* Expected */ }
            catch (Exception ex)
            {
                errors.Add($"ValidateIndex threw wrong exception type for null IndexName: {ex.GetType().Name}");
            }

            // Test with empty IndexName
            try
            {
                var invalidIndex = new ModelIndex
                {
                    IndexName = "   ",
                    TableName = "Users",
                    Columns = new List<IndexColumn> { new IndexColumn { ColumnName = "Email" } }
                };
                QueryValidator.ValidateIndex(invalidIndex);
                errors.Add("ValidateIndex did not throw for empty IndexName");
            }
            catch (ValidationException) { /* Expected */ }
            catch (Exception ex)
            {
                errors.Add($"ValidateIndex threw wrong exception type for empty IndexName: {ex.GetType().Name}");
            }

            // Test with null TableName
            try
            {
                var invalidIndex = new ModelIndex
                {
                    IndexName = "IX_Users_Email",
                    TableName = null,
                    Columns = new List<IndexColumn> { new IndexColumn { ColumnName = "Email" } }
                };
                QueryValidator.ValidateIndex(invalidIndex);
                errors.Add("ValidateIndex did not throw for null TableName");
            }
            catch (ValidationException) { /* Expected */ }
            catch (Exception ex)
            {
                errors.Add($"ValidateIndex threw wrong exception type for null TableName: {ex.GetType().Name}");
            }

            // Test with empty TableName
            try
            {
                var invalidIndex = new ModelIndex
                {
                    IndexName = "IX_Users_Email",
                    TableName = "   ",
                    Columns = new List<IndexColumn> { new IndexColumn { ColumnName = "Email" } }
                };
                QueryValidator.ValidateIndex(invalidIndex);
                errors.Add("ValidateIndex did not throw for empty TableName");
            }
            catch (ValidationException) { /* Expected */ }
            catch (Exception ex)
            {
                errors.Add($"ValidateIndex threw wrong exception type for empty TableName: {ex.GetType().Name}");
            }

            // Test with empty Columns list
            try
            {
                var invalidIndex = new ModelIndex
                {
                    IndexName = "IX_Users_Email",
                    TableName = "Users",
                    Columns = new List<IndexColumn>()
                };
                QueryValidator.ValidateIndex(invalidIndex);
                errors.Add("ValidateIndex did not throw for empty Columns list");
            }
            catch (ValidationException) { /* Expected */ }
            catch (Exception ex)
            {
                errors.Add($"ValidateIndex threw wrong exception type for empty Columns list: {ex.GetType().Name}");
            }
        }
        catch (Exception ex)
        {
            errors.Add($"ValidateIndex validation threw exception: {ex.GetType().Name}: {ex.Message}");
        }

        // Validate ValidateIndexSuggestion behavior
        try
        {
            // Test with null
            try
            {
                QueryValidator.ValidateIndexSuggestion(null!);
                errors.Add("ValidateIndexSuggestion did not throw for null input");
            }
            catch (ValidationException) { /* Expected */ }
            catch (Exception ex)
            {
                errors.Add($"ValidateIndexSuggestion threw wrong exception type for null: {ex.GetType().Name}");
            }

            // Test with valid IndexSuggestion
            try
            {
                var validSuggestion = new IndexSuggestion
                {
                    TableName = "Users",
                    IndexColumns = new List<string> { "Email" },
                    EstimatedPerformanceGain = 45.2
                };
                QueryValidator.ValidateIndexSuggestion(validSuggestion);
            }
            catch (Exception ex)
            {
                errors.Add($"ValidateIndexSuggestion threw exception for valid IndexSuggestion: {ex.GetType().Name}: {ex.Message}");
            }

            // Test with null TableName
            try
            {
                var invalidSuggestion = new IndexSuggestion
                {
                    TableName = null,
                    IndexColumns = new List<string> { "Email" },
                    EstimatedPerformanceGain = 45.2
                };
                QueryValidator.ValidateIndexSuggestion(invalidSuggestion);
                errors.Add("ValidateIndexSuggestion did not throw for null TableName");
            }
            catch (ValidationException) { /* Expected */ }
            catch (Exception ex)
            {
                errors.Add($"ValidateIndexSuggestion threw wrong exception type for null TableName: {ex.GetType().Name}");
            }

            // Test with empty TableName
            try
            {
                var invalidSuggestion = new IndexSuggestion
                {
                    TableName = "   ",
                    IndexColumns = new List<string> { "Email" },
                    EstimatedPerformanceGain = 45.2
                };
                QueryValidator.ValidateIndexSuggestion(invalidSuggestion);
                errors.Add("ValidateIndexSuggestion did not throw for empty TableName");
            }
            catch (ValidationException) { /* Expected */ }
            catch (Exception ex)
            {
                errors.Add($"ValidateIndexSuggestion threw wrong exception type for empty TableName: {ex.GetType().Name}");
            }

            // Test with empty IndexColumns list
            try
            {
                var invalidSuggestion = new IndexSuggestion
                {
                    TableName = "Users",
                    IndexColumns = new List<string>(),
                    EstimatedPerformanceGain = 45.2
                };
                QueryValidator.ValidateIndexSuggestion(invalidSuggestion);
                errors.Add("ValidateIndexSuggestion did not throw for empty IndexColumns list");
            }
            catch (ValidationException) { /* Expected */ }
            catch (Exception ex)
            {
                errors.Add($"ValidateIndexSuggestion threw wrong exception type for empty IndexColumns list: {ex.GetType().Name}");
            }

            // Test with out of range EstimatedPerformanceGain
            try
            {
                var invalidSuggestion = new IndexSuggestion
                {
                    TableName = "Users",
                    IndexColumns = new List<string> { "Email" },
                    EstimatedPerformanceGain = 150
                };
                QueryValidator.ValidateIndexSuggestion(invalidSuggestion);
                errors.Add("ValidateIndexSuggestion did not throw for out of range EstimatedPerformanceGain");
            }
            catch (ValidationException) { /* Expected */ }
            catch (Exception ex)
            {
                errors.Add($"ValidateIndexSuggestion threw wrong exception type for out of range EstimatedPerformanceGain: {ex.GetType().Name}");
            }
        }
        catch (Exception ex)
        {
            errors.Add($"ValidateIndexSuggestion validation threw exception: {ex.GetType().Name}: {ex.Message}");
        }

        // Validate DetectSQLInjectionRisks behavior
        try
        {
            // Test with null
            var nullRisks = QueryValidator.DetectSQLInjectionRisks(null!);
            if (nullRisks == null)
            {
                errors.Add("DetectSQLInjectionRisks returned null for null input");
            }

            // Test with clean query
            var cleanRisks = QueryValidator.DetectSQLInjectionRisks("SELECT * FROM users WHERE id = 1");
            if (cleanRisks.Count != 0)
            {
                errors.Add("DetectSQLInjectionRisks returned risks for clean query");
            }

            // Test with injection query
            var injectionQuery = "SELECT * FROM users WHERE 1=1 OR '1'='1'";
            var injectionRisks = QueryValidator.DetectSQLInjectionRisks(injectionQuery);
            if (injectionRisks.Count == 0)
            {
                errors.Add("DetectSQLInjectionRisks did not detect injection pattern");
            }
        }
        catch (Exception ex)
        {
            errors.Add($"DetectSQLInjectionRisks validation threw exception: {ex.GetType().Name}: {ex.Message}");
        }

        // Validate ValidateConnectionString behavior
        try
        {
            // Test with null
            try
            {
                QueryValidator.ValidateConnectionString(null!);
                errors.Add("ValidateConnectionString did not throw for null input");
            }
            catch (ConfigurationException) { /* Expected */ }
            catch (Exception ex)
            {
                errors.Add($"ValidateConnectionString threw wrong exception type for null: {ex.GetType().Name}");
            }

            // Test with empty string
            try
            {
                QueryValidator.ValidateConnectionString(string.Empty);
                errors.Add("ValidateConnectionString did not throw for empty string");
            }
            catch (ConfigurationException) { /* Expected */ }
            catch (Exception ex)
            {
                errors.Add($"ValidateConnectionString threw wrong exception type for empty string: {ex.GetType().Name}");
            }

            // Test with whitespace
            try
            {
                QueryValidator.ValidateConnectionString("   ");
                errors.Add("ValidateConnectionString did not throw for whitespace");
            }
            catch (ConfigurationException) { /* Expected */ }
            catch (Exception ex)
            {
                errors.Add($"ValidateConnectionString threw wrong exception type for whitespace: {ex.GetType().Name}");
            }

            // Test with missing server information
            try
            {
                QueryValidator.ValidateConnectionString("Database=testdb;User=test;Password=test");
                errors.Add("ValidateConnectionString did not throw for missing server information");
            }
            catch (ConfigurationException) { /* Expected */ }
            catch (Exception ex)
            {
                errors.Add($"ValidateConnectionString threw wrong exception type for missing server: {ex.GetType().Name}");
            }

            // Test with missing database specification
            try
            {
                QueryValidator.ValidateConnectionString("Server=localhost;User=test;Password=test");
                errors.Add("ValidateConnectionString did not throw for missing database specification");
            }
            catch (ConfigurationException) { /* Expected */ }
            catch (Exception ex)
            {
                errors.Add($"ValidateConnectionString threw wrong exception type for missing database: {ex.GetType().Name}");
            }

            // Test with valid connection string
            try
            {
                QueryValidator.ValidateConnectionString("Server=localhost;Database=testdb;User=test;Password=test");
            }
            catch (Exception ex)
            {
                errors.Add($"ValidateConnectionString threw exception for valid connection string: {ex.GetType().Name}: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            errors.Add($"ValidateConnectionString validation threw exception: {ex.GetType().Name}: {ex.Message}");
        }

        // Validate SanitizeQueryForDisplay behavior
        try
        {
            // Test with null
            var nullResult = QueryValidator.SanitizeQueryForDisplay(null!);
            if (nullResult != string.Empty)
            {
                errors.Add("SanitizeQueryForDisplay did not return empty string for null input");
            }

            // Test with empty string
            var emptyResult = QueryValidator.SanitizeQueryForDisplay(string.Empty);
            if (emptyResult != string.Empty)
            {
                errors.Add("SanitizeQueryForDisplay did not return empty string for empty input");
            }

            // Test with whitespace
            var whitespaceResult = QueryValidator.SanitizeQueryForDisplay("   ");
            if (whitespaceResult != "   ")
            {
                errors.Add("SanitizeQueryForDisplay modified whitespace incorrectly");
            }

            // Test with newlines and tabs
            var formattedQuery = "SELECT *\nFROM users\tWHERE id = 1";
            var sanitized = QueryValidator.SanitizeQueryForDisplay(formattedQuery);
            if (sanitized.Contains("\n") || sanitized.Contains("\t"))
            {
                errors.Add("SanitizeQueryForDisplay did not remove newlines and tabs");
            }

            // Test with maxLength parameter
            var longQuery = new string('A', 150);
            var truncated = QueryValidator.SanitizeQueryForDisplay(longQuery, 50);
            if (truncated.Length != 53) // 50 chars + "..."
            {
                errors.Add("SanitizeQueryForDisplay did not truncate correctly");
            }
        }
        catch (Exception ex)
        {
            errors.Add($"SanitizeQueryForDisplay validation threw exception: {ex.GetType().Name}: {ex.Message}");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the QueryValidator static class is valid.
    /// </summary>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid()
    {
        return Validate().Count == 0;
    }

    /// <summary>
    /// Ensures the QueryValidator static class is valid.
    /// Throws an <see cref="ArgumentException"/> with a detailed message listing all validation problems.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if validation fails, containing all validation errors.</exception>
    public static void EnsureValid()
    {
        var errors = Validate();
        if (errors.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"QueryValidator is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
    }
}
