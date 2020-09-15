#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using SqlQueryAnalyzer.Models;

namespace SqlQueryAnalyzer.Utilities;

/// <summary>
/// Provides validation helpers for <see cref="ReportGenerator"/> method parameters.
/// </summary>
public static class ReportGeneratorValidation
{
    /// <summary>
    /// Validates parameters for <see cref="ReportGenerator.GenerateTextReport(QueryAnalysisResult)"/>,
    /// <see cref="ReportGenerator.GenerateCsvReport(List{QueryAnalysisResult})"/>,
    /// <see cref="ReportGenerator.GenerateJsonReport(QueryAnalysisResult)"/>,
    /// <see cref="ReportGenerator.GenerateHtmlReport(QueryAnalysisResult)"/>, and
    /// <see cref="ReportGenerator.GenerateSummary(QueryAnalysisResult)"/>.
    /// </summary>
    /// <param name="analysis">The query analysis result to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="analysis"/> is null.</exception>
    public static IReadOnlyList<string> Validate(QueryAnalysisResult analysis)
    {
        ArgumentNullException.ThrowIfNull(analysis);

        var errors = new List<string>();

        // Validate QueryId
        if (string.IsNullOrWhiteSpace(analysis.QueryId))
        {
            errors.Add("QueryId must not be null or whitespace.");
        }

        // Validate Query/QueryText
        if (string.IsNullOrWhiteSpace(analysis.Query))
        {
            errors.Add("Query must not be null or whitespace.");
        }

        // Validate AnalyzedAt (should not be default DateTime)
        if (analysis.AnalyzedAt == default)
        {
            errors.Add("AnalyzedAt must be set to a valid date/time.");
        }
        else if (analysis.AnalyzedAt > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add("AnalyzedAt cannot be in the future.");
        }

        // Validate Complexity
        if (analysis.Complexity < Constants.QueryComplexity.Simple ||
            analysis.Complexity > Constants.QueryComplexity.Extreme)
        {
            errors.Add("Complexity must be a valid QueryComplexity value.");
        }

        // Validate PerformanceScore (0-100 range)
        if (analysis.PerformanceScore < 0 || analysis.PerformanceScore > 100)
        {
            errors.Add("PerformanceScore must be between 0 and 100.");
        }

        // Validate EstimatedExecutionTime (should be positive)
        if (analysis.EstimatedExecutionTime < TimeSpan.Zero)
        {
            errors.Add("EstimatedExecutionTime must not be negative.");
        }

        // Validate Issues list (can be empty but not null)
        if (analysis.Issues is null)
        {
            errors.Add("Issues list must not be null.");
        }

        // Validate IndexSuggestions list (can be empty but not null)
        if (analysis.IndexSuggestions is null)
        {
            errors.Add("IndexSuggestions list must not be null.");
        }

        // Validate Statistics (should not be null)
        if (analysis.Statistics is null)
        {
            errors.Add("Statistics must not be null.");
        }

        // Validate Metadata (can be empty but not null)
        if (analysis.Metadata is null)
        {
            errors.Add("Metadata must not be null.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates parameters for <see cref="ReportGenerator.GenerateCsvReport(List{QueryAnalysisResult})"/>.
    /// </summary>
    /// <param name="analyses">The list of query analysis results to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="analyses"/> is null.</exception>
    public static IReadOnlyList<string> Validate(List<QueryAnalysisResult> analyses)
    {
        ArgumentNullException.ThrowIfNull(analyses);

        var errors = new List<string>();

        if (analyses.Count == 0)
        {
            errors.Add("Analyses list must not be empty.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the parameters for <see cref="ReportGenerator.GenerateTextReport(QueryAnalysisResult)"/>,
    /// <see cref="ReportGenerator.GenerateCsvReport(List{QueryAnalysisResult})"/>,
    /// <see cref="ReportGenerator.GenerateJsonReport(QueryAnalysisResult)"/>,
    /// <see cref="ReportGenerator.GenerateHtmlReport(QueryAnalysisResult)"/>, and
    /// <see cref="ReportGenerator.GenerateSummary(QueryAnalysisResult)"/> are valid.
    /// </summary>
    /// <param name="analysis">The query analysis result to check.</param>
    /// <returns>True if the parameters are valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="analysis"/> is null.</exception>
    public static bool IsValid(QueryAnalysisResult analysis)
    {
        return Validate(analysis).Count == 0;
    }

    /// <summary>
    /// Determines whether the parameters for <see cref="ReportGenerator.GenerateCsvReport(List{QueryAnalysisResult})"/> are valid.
    /// </summary>
    /// <param name="analyses">The list of query analysis results to check.</param>
    /// <returns>True if the parameters are valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="analyses"/> is null.</exception>
    public static bool IsValid(List<QueryAnalysisResult> analyses)
    {
        return Validate(analyses).Count == 0;
    }

    /// <summary>
    /// Ensures that the parameters for <see cref="ReportGenerator.GenerateTextReport(QueryAnalysisResult)"/>,
    /// <see cref="ReportGenerator.GenerateCsvReport(List{QueryAnalysisResult})"/>,
    /// <see cref="ReportGenerator.GenerateJsonReport(QueryAnalysisResult)"/>,
    /// <see cref="ReportGenerator.GenerateHtmlReport(QueryAnalysisResult)"/>, and
    /// <see cref="ReportGenerator.GenerateSummary(QueryAnalysisResult)"/> are valid, throwing an <see cref="ArgumentException"/> if not.
    /// </summary>
    /// <param name="analysis">The query analysis result to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="analysis"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the parameters are not valid, containing a list of validation problems.</exception>
    public static void EnsureValid(QueryAnalysisResult analysis)
    {
        var errors = Validate(analysis);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"Parameters for ReportGenerator methods are not valid. Problems:\n{string.Join("\n", errors)}");
        }
    }

    /// <summary>
    /// Ensures that the parameters for <see cref="ReportGenerator.GenerateCsvReport(List{QueryAnalysisResult})"/> are valid,
    /// throwing an <see cref="ArgumentException"/> if not.
    /// </summary>
    /// <param name="analyses">The list of query analysis results to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="analyses"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the parameters are not valid, containing a list of validation problems.</exception>
    public static void EnsureValid(List<QueryAnalysisResult> analyses)
    {
        var errors = Validate(analyses);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"Parameters for GenerateCsvReport are not valid. Problems:\n{string.Join("\n", errors)}");
        }
    }
}
