#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace SqlQueryAnalyzer.Models;

/// <summary>
/// Provides validation helpers for <see cref="PerformanceIssue"/> instances.
/// </summary>
public static class PerformanceIssueValidation
{
    /// <summary>
    /// Validates a <see cref="PerformanceIssue"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The performance issue to validate.</param>
    /// <returns>A read-only list of validation problems; empty if the issue is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this PerformanceIssue value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate IssueId
        if (string.IsNullOrWhiteSpace(value.IssueId))
        {
            problems.Add("IssueId cannot be null, empty, or whitespace.");
        }

        // Validate IssueType
        if (!Enum.IsDefined(value.IssueType))
        {
            problems.Add("IssueType must be a valid enum value.");
        }

        // Validate Severity
        if (!Enum.IsDefined(value.Severity))
        {
            problems.Add("Severity must be a valid IssueSeverity value.");
        }

        // Validate Description
        if (string.IsNullOrWhiteSpace(value.Description))
        {
            problems.Add("Description cannot be null, empty, or whitespace.");
        }
        else if (value.Description.Length > 2000)
        {
            problems.Add("Description exceeds maximum length of 2000 characters.");
        }

        // Validate AffectedClause
        if (value.AffectedClause.Length > 100)
        {
            problems.Add("AffectedClause exceeds maximum length of 100 characters.");
        }

        // Validate LineNumber
        if (value.LineNumber < 1)
        {
            problems.Add("LineNumber must be a positive integer (1 or greater).");
        }
        else if (value.LineNumber > 1_000_000)
        {
            problems.Add("LineNumber exceeds reasonable maximum value.");
        }

        // Validate ColumnNumber
        if (value.ColumnNumber < 1)
        {
            problems.Add("ColumnNumber must be a positive integer (1 or greater).");
        }
        else if (value.ColumnNumber > 10_000)
        {
            problems.Add("ColumnNumber exceeds reasonable maximum value.");
        }

        // Validate EstimatedPerformanceImpact
        if (double.IsNaN(value.EstimatedPerformanceImpact))
        {
            problems.Add("EstimatedPerformanceImpact cannot be NaN.");
        }
        else if (double.IsInfinity(value.EstimatedPerformanceImpact))
        {
            problems.Add("EstimatedPerformanceImpact cannot be infinite.");
        }
        else if (value.EstimatedPerformanceImpact < 0 || value.EstimatedPerformanceImpact > 100)
        {
            problems.Add("EstimatedPerformanceImpact must be between 0 and 100 inclusive.");
        }

        // Validate AffectedRowCount (if not null)
        if (value.AffectedRowCount.HasValue)
        {
            if (value.AffectedRowCount < 0)
            {
                problems.Add("AffectedRowCount cannot be negative.");
            }
            else if (value.AffectedRowCount > 100_000_000)
            {
                problems.Add("AffectedRowCount exceeds reasonable maximum value.");
            }
        }

        // Validate EstimatedTimeIncrease (if not null)
        if (value.EstimatedTimeIncrease.HasValue)
        {
            if (value.EstimatedTimeIncrease.Value < TimeSpan.Zero)
            {
                problems.Add("EstimatedTimeIncrease cannot be negative.");
            }
            else if (value.EstimatedTimeIncrease.Value.TotalHours > 24)
            {
                problems.Add("EstimatedTimeIncrease exceeds reasonable maximum of 24 hours.");
            }
        }

        // Validate RecommendedFix
        if (value.RecommendedFix.Length > 2000)
        {
            problems.Add("RecommendedFix exceeds maximum length of 2000 characters.");
        }

        // Validate ExampleFix
        if (value.ExampleFix.Length > 2000)
        {
            problems.Add("ExampleFix exceeds maximum length of 2000 characters.");
        }

        // Validate Priority
        if (value.Priority < 1 || value.Priority > 5)
        {
            problems.Add("Priority must be between 1 and 5 inclusive.");
        }

        // Validate Metadata
        if (value.Metadata == null)
        {
            problems.Add("Metadata dictionary cannot be null.");
        }
        else
        {
            foreach (var kvp in value.Metadata)
            {
                if (string.IsNullOrWhiteSpace(kvp.Key))
                {
                    problems.Add("Metadata contains a null or empty key.");
                    break;
                }

                if (kvp.Value != null && kvp.Value.Length > 1000)
                {
                    problems.Add($"Metadata value for key '{kvp.Key}' exceeds maximum length of 1000 characters.");
                    break;
                }
            }
        }

        // Validate DetectedAt
        if (value.DetectedAt == default)
        {
            problems.Add("DetectedAt cannot be the default DateTime value.");
        }
        else if (value.DetectedAt > DateTime.UtcNow.AddMinutes(5))
        {
            problems.Add("DetectedAt cannot be in the future.");
        }
        else if (value.DetectedAt.Year < 2000 || value.DetectedAt.Year > DateTime.UtcNow.Year + 1)
        {
            problems.Add("DetectedAt has an invalid year value.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="PerformanceIssue"/> is valid.
    /// </summary>
    /// <param name="value">The performance issue to check.</param>
    /// <returns>True if the issue is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this PerformanceIssue value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="PerformanceIssue"/> is valid, throwing an <see cref="ArgumentException"/> if it is not.
    /// </summary>
    /// <param name="value">The performance issue to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the issue is invalid, containing details of all validation problems.</exception>
    public static void EnsureValid(this PerformanceIssue value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();

        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"PerformanceIssue validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
    }
}