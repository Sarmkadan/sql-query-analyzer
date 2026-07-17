#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlQueryAnalyzer.Utilities;

/// <summary>
/// Provides validation helpers for <see cref="SqlInjectionDetectorExtensions"/> method parameters.
/// Validates null values and empty strings for extension method parameters.
/// </summary>
public sealed class SqlInjectionDetectorExtensionsValidation
{
    /// <summary>
    /// Validates parameters for <see cref="SqlInjectionDetectorExtensions.FilterBySeverity(SqlInjectionDetector, List{SqlInjectionIssue}, string)"/> extension method.
    /// </summary>
    /// <param name="detector">The detector instance (can be null).</param>
    /// <param name="issues">List of detected vulnerabilities (must not be null).</param>
    /// <param name="minSeverity">Minimum severity level (must not be null or empty if provided).</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="issues"/> is null.</exception>
    public static IReadOnlyList<string> Validate(
        SqlInjectionDetector? detector,
        List<SqlInjectionIssue>? issues,
        string? minSeverity = null)
    {
        ArgumentNullException.ThrowIfNull(issues);

        var errors = new List<string>();

        if (minSeverity is not null)
        {
            if (string.IsNullOrWhiteSpace(minSeverity))
            {
                errors.Add("minSeverity must not be null or whitespace.");
            }
            else
            {
                var validSeverities = new[] { "Critical", "High", "Medium", "Low" };
                if (!validSeverities.Contains(minSeverity, StringComparer.OrdinalIgnoreCase))
                {
                    errors.Add($"minSeverity '{minSeverity}' is not valid. Expected: Critical, High, Medium, or Low.");
                }
            }
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates parameters for <see cref="SqlInjectionDetectorExtensions.GroupByType(SqlInjectionDetector, List{SqlInjectionIssue})"/>
    /// and <see cref="SqlInjectionDetectorExtensions.GenerateSummaryReport(SqlInjectionDetector, List{SqlInjectionIssue})"/> extension methods.
    /// </summary>
    /// <param name="detector">The detector instance (can be null).</param>
    /// <param name="issues">List of detected vulnerabilities (must not be null).</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="issues"/> is null.</exception>
    public static IReadOnlyList<string> ValidateGroupByTypeAndGenerateSummaryReport(
        SqlInjectionDetector? detector,
        List<SqlInjectionIssue>? issues)
    {
        ArgumentNullException.ThrowIfNull(issues);
        return Array.Empty<string>();
    }

    /// <summary>
    /// Validates parameters for <see cref="SqlInjectionDetectorExtensions.GenerateDetailedReport(SqlInjectionDetector, List{SqlInjectionIssue}, string)"/> extension method.
    /// </summary>
    /// <param name="detector">The detector instance (can be null).</param>
    /// <param name="issues">List of detected vulnerabilities (must not be null).</param>
    /// <param name="query">The original query being analyzed (must not be null or empty).</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="issues"/> or <paramref name="query"/> is null.</exception>
    public static IReadOnlyList<string> ValidateDetailedReport(
        SqlInjectionDetector? detector,
        List<SqlInjectionIssue>? issues,
        string? query)
    {
        ArgumentNullException.ThrowIfNull(issues);
        ArgumentException.ThrowIfNullOrEmpty(query);

        return Array.Empty<string>();
    }

    /// <summary>
    /// Validates parameters for <see cref="SqlInjectionDetectorExtensions.HasCriticalIssues(SqlInjectionDetector, List{SqlInjectionIssue})"/> extension method.
    /// </summary>
    /// <param name="detector">The detector instance (can be null).</param>
    /// <param name="issues">List of detected vulnerabilities (must not be null).</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="issues"/> is null.</exception>
    public static IReadOnlyList<string> ValidateHasCriticalIssues(
        SqlInjectionDetector? detector,
        List<SqlInjectionIssue>? issues)
    {
        ArgumentNullException.ThrowIfNull(issues);
        return Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether the parameters for <see cref="SqlInjectionDetectorExtensions.FilterBySeverity(SqlInjectionDetector, List{SqlInjectionIssue}, string)"/> are valid.
    /// </summary>
    /// <param name="detector">The detector instance.</param>
    /// <param name="issues">List of detected vulnerabilities.</param>
    /// <param name="minSeverity">Minimum severity level.</param>
    /// <returns>True if the parameters are valid; otherwise, false.</returns>
    public static bool IsValid(
        SqlInjectionDetector? detector,
        List<SqlInjectionIssue>? issues,
        string? minSeverity = null) =>
        Validate(detector, issues, minSeverity).Count == 0;

    /// <summary>
    /// Determines whether the parameters for <see cref="SqlInjectionDetectorExtensions.GroupByType(SqlInjectionDetector, List{SqlInjectionIssue})"/>
    /// and <see cref="SqlInjectionDetectorExtensions.GenerateSummaryReport(SqlInjectionDetector, List{SqlInjectionIssue})"/> are valid.
    /// </summary>
    /// <param name="detector">The detector instance.</param>
    /// <param name="issues">List of detected vulnerabilities.</param>
    /// <returns>True if the parameters are valid; otherwise, false.</returns>
    public static bool IsValidGroupByTypeAndGenerateSummaryReport(
        SqlInjectionDetector? detector,
        List<SqlInjectionIssue>? issues) =>
        ValidateGroupByTypeAndGenerateSummaryReport(detector, issues).Count == 0;

    /// <summary>
    /// Determines whether the parameters for <see cref="SqlInjectionDetectorExtensions.GenerateDetailedReport(SqlInjectionDetector, List{SqlInjectionIssue}, string)"/> are valid.
    /// </summary>
    /// <param name="detector">The detector instance.</param>
    /// <param name="issues">List of detected vulnerabilities.</param>
    /// <param name="query">The original query being analyzed.</param>
    /// <returns>True if the parameters are valid; otherwise, false.</returns>
    public static bool IsValidDetailedReport(
        SqlInjectionDetector? detector,
        List<SqlInjectionIssue>? issues,
        string? query) =>
        ValidateDetailedReport(detector, issues, query).Count == 0;

    /// <summary>
    /// Determines whether the parameters for <see cref="SqlInjectionDetectorExtensions.HasCriticalIssues(SqlInjectionDetector, List{SqlInjectionIssue})"/> are valid.
    /// </summary>
    /// <param name="detector">The detector instance.</param>
    /// <param name="issues">List of detected vulnerabilities.</param>
    /// <returns>True if the parameters are valid; otherwise, false.</returns>
    public static bool IsValidHasCriticalIssues(
        SqlInjectionDetector? detector,
        List<SqlInjectionIssue>? issues) =>
        ValidateHasCriticalIssues(detector, issues).Count == 0;

    /// <summary>
    /// Ensures that the parameters for <see cref="SqlInjectionDetectorExtensions.FilterBySeverity(SqlInjectionDetector, List{SqlInjectionIssue}, string)"/> are valid,
    /// throwing an <see cref="ArgumentException"/> if not.
    /// </summary>
    /// <param name="detector">The detector instance.</param>
    /// <param name="issues">List of detected vulnerabilities.</param>
    /// <param name="minSeverity">Minimum severity level.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="issues"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the parameters are not valid, containing a list of validation problems.</exception>
    public static void EnsureValid(
        SqlInjectionDetector? detector,
        List<SqlInjectionIssue>? issues,
        string? minSeverity = null)
    {
        var errors = Validate(detector, issues, minSeverity);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"Parameters for SqlInjectionDetectorExtensions.FilterBySeverity are not valid. Problems:\n{string.Join("\n", errors.Select((e, i) => $" {i + 1}. {e}"))}");
        }
    }

    /// <summary>
    /// Ensures that the parameters for <see cref="SqlInjectionDetectorExtensions.GroupByType(SqlInjectionDetector, List{SqlInjectionIssue})"/>
    /// and <see cref="SqlInjectionDetectorExtensions.GenerateSummaryReport(SqlInjectionDetector, List{SqlInjectionIssue})"/> are valid,
    /// throwing an <see cref="ArgumentException"/> if not.
    /// </summary>
    /// <param name="detector">The detector instance.</param>
    /// <param name="issues">List of detected vulnerabilities.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="issues"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the parameters are not valid, containing a list of validation problems.</exception>
    public static void EnsureValidGroupByTypeAndGenerateSummaryReport(
        SqlInjectionDetector? detector,
        List<SqlInjectionIssue>? issues)
    {
        var errors = ValidateGroupByTypeAndGenerateSummaryReport(detector, issues);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"Parameters for SqlInjectionDetectorExtensions.GroupByType and GenerateSummaryReport are not valid. Problems:\n{string.Join("\n", errors.Select((e, i) => $" {i + 1}. {e}"))}");
        }
    }

    /// <summary>
    /// Ensures that the parameters for <see cref="SqlInjectionDetectorExtensions.GenerateDetailedReport(SqlInjectionDetector, List{SqlInjectionIssue}, string)"/> are valid,
    /// throwing an <see cref="ArgumentException"/> if not.
    /// </summary>
    /// <param name="detector">The detector instance.</param>
    /// <param name="issues">List of detected vulnerabilities.</param>
    /// <param name="query">The original query being analyzed.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="issues"/> or <paramref name="query"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the parameters are not valid, containing a list of validation problems.</exception>
    public static void EnsureValidDetailedReport(
        SqlInjectionDetector? detector,
        List<SqlInjectionIssue>? issues,
        string? query)
    {
        var errors = ValidateDetailedReport(detector, issues, query);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"Parameters for SqlInjectionDetectorExtensions.GenerateDetailedReport are not valid. Problems:\n{string.Join("\n", errors.Select((e, i) => $" {i + 1}. {e}"))}");
        }
    }

    /// <summary>
    /// Ensures that the parameters for <see cref="SqlInjectionDetectorExtensions.HasCriticalIssues(SqlInjectionDetector, List{SqlInjectionIssue})"/> are valid,
    /// throwing an <see cref="ArgumentException"/> if not.
    /// </summary>
    /// <param name="detector">The detector instance.</param>
    /// <param name="issues">List of detected vulnerabilities.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="issues"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the parameters are not valid, containing a list of validation problems.</exception>
    public static void EnsureValidHasCriticalIssues(
        SqlInjectionDetector? detector,
        List<SqlInjectionIssue>? issues)
    {
        var errors = ValidateHasCriticalIssues(detector, issues);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"Parameters for SqlInjectionDetectorExtensions.HasCriticalIssues are not valid. Problems:\n{string.Join("\n", errors.Select((e, i) => $" {i + 1}. {e}"))}");
        }
    }
}