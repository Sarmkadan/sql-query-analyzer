#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using SqlQueryAnalyzer.Constants;

namespace SqlQueryAnalyzer.Models;

/// <summary>
/// Provides validation helpers for QueryAnalysisResultExtensions extension methods
/// </summary>
public static class QueryAnalysisResultExtensionsValidation
{
    /// <summary>
    /// Validates the return values from QueryAnalysisResultExtensions extension methods
    /// </summary>
    /// <param name="value">The query analysis result to validate extension method results against</param>
    /// <returns>List of validation problems (empty if all extension method results are valid)</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
    public static IReadOnlyList<string> ValidateQueryAnalysisResultExtensions(this QueryAnalysisResult value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Create a test instance for validation
        var testResult = new QueryAnalysisResult
        {
            QueryId = Guid.NewGuid().ToString(),
            Query = "SELECT * FROM TestTable WHERE Id = 1",
            AnalyzedAt = DateTime.UtcNow,
            Complexity = QueryComplexity.Medium,
            PerformanceScore = 85,
            EstimatedExecutionTime = TimeSpan.FromMilliseconds(150),
            Issues = new List<PerformanceIssue>(),
            IndexSuggestions = new List<IndexSuggestion>(),
            ExecutionPlan = null,
            Statistics = new QueryStatistics
            {
                ExecutionCount = 100,
                TotalCpuTime = TimeSpan.FromMilliseconds(500),
                TotalLogicalReads = 1000,
                TotalLogicalWrites = 50,
                TotalExecutionTime = TimeSpan.FromMilliseconds(200),
                RowsAffected = 10,
                AverageRowsReturned = 10,
                MaxRowsReturned = 100
            },
            Metadata = new Dictionary<string, object>()
        };

        // Test IsHighPerformance
        try
        {
            var result = testResult.IsHighPerformance();
            var expectedHighPerformance = testResult.PerformanceScore >= 90 && !testResult.HasCriticalIssues;
            if (result != expectedHighPerformance)
            {
                problems.Add($"IsHighPerformance returned {result}, but expected {expectedHighPerformance} for PerformanceScore={testResult.PerformanceScore}, HasCriticalIssues={testResult.HasCriticalIssues}");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"IsHighPerformance threw exception: {ex.Message}");
        }

        // Test NeedsOptimization
        try
        {
            var result = testResult.NeedsOptimization();
            var expectedNeedsOptimization = testResult.PerformanceScore < 70 || testResult.HasCriticalIssues;
            if (result != expectedNeedsOptimization)
            {
                problems.Add($"NeedsOptimization returned {result}, but expected {expectedNeedsOptimization} for PerformanceScore={testResult.PerformanceScore}, HasCriticalIssues={testResult.HasCriticalIssues}");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"NeedsOptimization threw exception: {ex.Message}");
        }

        // Test GetSeverityLevel
        try
        {
            var result = testResult.GetSeverityLevel();
            var expectedSeverity = testResult.HasCriticalIssues
                ? "Critical"
                : testResult.PerformanceScore < 60
                    ? "High"
                    : testResult.PerformanceScore < 80
                        ? "Medium"
                        : "Low";

            if (result != expectedSeverity)
            {
                problems.Add($"GetSeverityLevel returned '{result}', but expected '{expectedSeverity}' for PerformanceScore={testResult.PerformanceScore}, HasCriticalIssues={testResult.HasCriticalIssues}");
            }

            var validSeverities = new[] { "Critical", "High", "Medium", "Low" };
            if (!validSeverities.Contains(result))
            {
                problems.Add($"GetSeverityLevel returned invalid severity level '{result}'. Expected one of: {string.Join(", ", validSeverities)}");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"GetSeverityLevel threw exception: {ex.Message}");
        }

        // Test DeepCopy
        try
        {
            var copy = testResult.DeepCopy();

            if (copy is null)
            {
                problems.Add("DeepCopy returned null");
            }
            else if (ReferenceEquals(copy, testResult))
            {
                problems.Add("DeepCopy returned the same reference as the original (shallow copy)");
            }
            else
            {
                // Validate all properties were copied correctly
                if (copy.QueryId != testResult.QueryId)
                {
                    problems.Add("DeepCopy did not copy QueryId correctly");
                }

                if (copy.Query != testResult.Query)
                {
                    problems.Add("DeepCopy did not copy Query correctly");
                }

                if (copy.AnalyzedAt != testResult.AnalyzedAt)
                {
                    problems.Add("DeepCopy did not copy AnalyzedAt correctly");
                }

                if (copy.Complexity != testResult.Complexity)
                {
                    problems.Add("DeepCopy did not copy Complexity correctly");
                }

                if (copy.PerformanceScore != testResult.PerformanceScore)
                {
                    problems.Add("DeepCopy did not copy PerformanceScore correctly");
                }

                if (copy.EstimatedExecutionTime != testResult.EstimatedExecutionTime)
                {
                    problems.Add("DeepCopy did not copy EstimatedExecutionTime correctly");
                }

                if (copy.Issues is null || copy.Issues.Count != testResult.Issues.Count)
                {
                    problems.Add("DeepCopy did not copy Issues correctly");
                }

                if (copy.IndexSuggestions is null || copy.IndexSuggestions.Count != testResult.IndexSuggestions.Count)
                {
                    problems.Add("DeepCopy did not copy IndexSuggestions correctly");
                }

                if (!ReferenceEquals(copy.ExecutionPlan, testResult.ExecutionPlan))
                {
                    problems.Add("DeepCopy did not copy ExecutionPlan correctly");
                }

                if (copy.Statistics is null || ReferenceEquals(copy.Statistics, testResult.Statistics))
                {
                    problems.Add("DeepCopy did not create a proper deep copy of Statistics");
                }
                else
                {
                    if (copy.Statistics.ExecutionCount != testResult.Statistics.ExecutionCount ||
                        copy.Statistics.TotalCpuTime != testResult.Statistics.TotalCpuTime ||
                        copy.Statistics.TotalLogicalReads != testResult.Statistics.TotalLogicalReads)
                    {
                        problems.Add("DeepCopy did not deep copy Statistics properties correctly");
                    }
                }

                if (copy.Metadata is null || copy.Metadata.Count != testResult.Metadata.Count || ReferenceEquals(copy.Metadata, testResult.Metadata))
                {
                    problems.Add("DeepCopy did not create a proper deep copy of Metadata");
                }
            }
        }
        catch (Exception ex)
        {
            problems.Add($"DeepCopy threw exception: {ex.Message}");
        }

        // Test FormatSummary
        try
        {
            var result = testResult.FormatSummary();

            if (result is null)
            {
                problems.Add("FormatSummary returned null");
            }
            else if (string.IsNullOrWhiteSpace(result))
            {
                problems.Add("FormatSummary returned empty or whitespace string");
            }
            else
            {
                if (!result.Contains(testResult.QueryId))
                {
                    problems.Add("FormatSummary does not contain QueryId");
                }

                if (!result.Contains(testResult.Query))
                {
                    problems.Add("FormatSummary does not contain Query");
                }

                if (!result.Contains(testResult.AnalyzedAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)))
                {
                    problems.Add("FormatSummary does not contain AnalyzedAt in expected format");
                }
            }
        }
        catch (Exception ex)
        {
            problems.Add($"FormatSummary threw exception: {ex.Message}");
        }

        // Test ToJsonString
        try
        {
            var result = testResult.ToJsonString();

            if (result is null)
            {
                problems.Add("ToJsonString returned null");
            }
            else if (string.IsNullOrWhiteSpace(result))
            {
                problems.Add("ToJsonString returned empty or whitespace string");
            }
            else
            {
                try
                {
                    JsonDocument.Parse(result);
                }
                catch (Exception jsonEx)
                {
                    problems.Add($"ToJsonString returned invalid JSON: {jsonEx.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            problems.Add($"ToJsonString threw exception: {ex.Message}");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the QueryAnalysisResultExtensions extension method results are valid
    /// </summary>
    /// <param name="value">The query analysis result to check extension method results against</param>
    /// <returns>True if all extension method results are valid; otherwise, false</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
    public static bool AreQueryAnalysisResultExtensionsValid(this QueryAnalysisResult value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.ValidateQueryAnalysisResultExtensions().Count == 0;
    }

    /// <summary>
    /// Ensures that the QueryAnalysisResultExtensions extension method results are valid, throwing an exception if they are not
    /// </summary>
    /// <param name="value">The query analysis result to validate extension method results against</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown when extension method results contain validation problems</exception>
    public static void EnsureQueryAnalysisResultExtensionsAreValid(this QueryAnalysisResult value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var problems = value.ValidateQueryAnalysisResultExtensions();

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"QueryAnalysisResultExtensions validation failed. Problems:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }
}
