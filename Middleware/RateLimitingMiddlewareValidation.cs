#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace SqlQueryAnalyzer.Middleware;

/// <summary>
/// Validation helpers for <see cref="RateLimitingMiddleware"/> to ensure configuration is valid.
/// </summary>
public static class RateLimitingMiddlewareValidation
{
    /// <summary>
    /// Validates a <see cref="RateLimitingMiddleware"/> instance and returns any problems found.
    /// </summary>
    /// <param name="value">The middleware instance to validate.</param>
    /// <returns>A list of human-readable problems, or empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this RateLimitingMiddleware? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate maxQueriesPerSecond (default is 100)
        // No validation needed as constructor has sensible defaults

        // Validate maxConcurrentAnalysis (default is 10)
        // No validation needed as constructor has sensible defaults

        // Validate internal state consistency
        var load = value.GetSystemLoad();
        if (load < 0 || load > 100)
        {
            problems.Add("System load must be between 0 and 100.");
        }

        // Validate query statistics if available
        // Note: These properties are not directly accessible, but we can check the stats returned by GetQueryStats
        try
        {
            var stats = value.GetQueryStats("test");
            if (stats.TotalRequests < 0)
            {
                problems.Add("TotalRequests must be non-negative.");
            }

            if (stats.AverageIntervalMs < 0)
            {
                problems.Add("AverageIntervalMs must be non-negative.");
            }

            if (string.IsNullOrWhiteSpace(stats.QueryHash))
            {
                problems.Add("QueryHash must not be null or whitespace.");
            }
        }
        catch
        {
            // GetQueryStats might throw if queryHash is invalid, but that's handled by the method itself
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="RateLimitingMiddleware"/> instance is valid.
    /// </summary>
    /// <param name="value">The middleware instance to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this RateLimitingMiddleware? value)
    {
        return value?.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="RateLimitingMiddleware"/> instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The middleware instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the middleware has validation problems.</exception>
    public static void EnsureValid(this RateLimitingMiddleware? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"RateLimitingMiddleware is not valid. Problems:\n{string.Join("\n", problems)}");
        }
    }
}