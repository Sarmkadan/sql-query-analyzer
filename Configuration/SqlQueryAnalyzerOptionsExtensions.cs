using System;
using System.Collections.Generic;

namespace SqlQueryAnalyzer.Configuration;

/// <summary>
/// Provides extension methods for <see cref="SqlQueryAnalyzerOptions"/> configuration validation and retrieval.
/// </summary>
public static class SqlQueryAnalyzerOptionsExtensions
{
    /// <summary>
    /// Validates that all required options are properly configured.
    /// </summary>
    /// <param name="options">The SQL query analyzer options to validate.</param>
    /// <returns>True if all required options are valid; otherwise false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    public static bool IsValid(this SqlQueryAnalyzerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return options.Database is not null
            && !string.IsNullOrWhiteSpace(options.Database.Provider)
            && !string.IsNullOrWhiteSpace(options.Database.ConnectionString)
            && options.Analysis is not null
            && options.Cache is not null
            && options.Performance is not null
            && options.Logging is not null;
    }

    /// <summary>
    /// Gets whether the analyzer is enabled for execution.
    /// Returns true if the cache is enabled AND the analysis options indicate analysis should run.
    /// </summary>
    /// <param name="options">The SQL query analyzer options.</param>
    /// <returns>True if analyzer is enabled; otherwise false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    public static bool IsAnalyzerEnabled(this SqlQueryAnalyzerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return options.Cache.Enabled && options.Analysis is not null;
    }

    /// <summary>
    /// Gets the effective provider name from the database options.
    /// Normalizes the provider name to lowercase for consistent comparison.
    /// </summary>
    /// <param name="options">The SQL query analyzer options.</param>
    /// <returns>The normalized provider name.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    public static string GetNormalizedProvider(this SqlQueryAnalyzerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return string.IsNullOrWhiteSpace(options.Database?.Provider)
            ? "sqlserver" // Default provider
            : options.Database.Provider.Trim().ToLowerInvariant();
    }

    /// <summary>
    /// Determines if any of the critical analysis features are enabled.
    /// </summary>
    /// <param name="options">The SQL query analyzer options.</param>
    /// <returns>True if any critical analysis feature is enabled; otherwise false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    public static bool HasCriticalAnalysisEnabled(this SqlQueryAnalyzerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return options.Analysis.DetectNPlusOne
            || options.Analysis.DetectMissingIndexes
            || options.Analysis.DetectJoinIssues
            || options.Analysis.AnalyzeExecutionPlans;
    }

    /// <summary>
    /// Gets the effective connection timeout in milliseconds.
    /// </summary>
    /// <param name="options">The SQL query analyzer options.</param>
    /// <returns>The connection timeout in milliseconds.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    public static int GetConnectionTimeoutMs(this SqlQueryAnalyzerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return options.Database.ConnectionTimeoutSeconds * 1000;
    }

    /// <summary>
    /// Gets the effective maximum concurrent analysis threads.
    /// Ensures at least 1 thread and no more than 100.
    /// </summary>
    /// <param name="options">The SQL query analyzer options.</param>
    /// <returns>The maximum concurrent threads.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    public static int GetMaxConcurrentThreads(this SqlQueryAnalyzerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return Math.Clamp(options.Analysis.MaxThreads, 1, 100);
    }

    /// <summary>
    /// Gets whether detailed logging is enabled based on configuration.
    /// Detailed logging is enabled if connection logging is enabled.
    /// </summary>
    /// <param name="options">The SQL query analyzer options.</param>
    /// <returns>True if detailed logging should be enabled; otherwise false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    public static bool ShouldEnableDetailedLogging(this SqlQueryAnalyzerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return options.Database.EnableConnectionLogging;
    }

    /// <summary>
    /// Gets the list of patterns to ignore, ensuring it's never null.
    /// </summary>
    /// <param name="options">The SQL query analyzer options.</param>
    /// <returns>A non-null list of ignore patterns.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> GetIgnorePatterns(this SqlQueryAnalyzerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return options.Analysis.IgnorePatterns?.AsReadOnly() ?? [];
    }

    /// <summary>
    /// Gets whether execution plan analysis is enabled and should be performed.
    /// </summary>
    /// <param name="options">The SQL query analyzer options.</param>
    /// <returns>True if execution plan analysis is enabled; otherwise false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    public static bool ShouldAnalyzeExecutionPlans(this SqlQueryAnalyzerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return options.Analysis.AnalyzeExecutionPlans;
    }

    /// <summary>
    /// Gets the effective maximum query length limit.
    /// </summary>
    /// <param name="options">The SQL query analyzer options.</param>
    /// <returns>The maximum query length in characters.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    public static int GetMaxQueryLength(this SqlQueryAnalyzerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return Math.Max(1024, options.Performance.MaxQueryLength);
    }
}