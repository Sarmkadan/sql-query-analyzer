using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlQueryAnalyzer.Configuration;

public static class SqlQueryAnalyzerOptionsExtensions
{
    /// <summary>
    /// Validates that all required options are properly configured.
    /// </summary>
    /// <param name="options">The SQL query analyzer options to validate</param>
    /// <returns>True if all required options are valid; otherwise false</returns>
    public static bool IsValid(this SqlQueryAnalyzerOptions options)
    {
        if (options == null)
        {
            return false;
        }

        return options.Database != null
               && !string.IsNullOrWhiteSpace(options.Database.Provider)
               && !string.IsNullOrWhiteSpace(options.Database.ConnectionString)
               && options.Analysis != null
               && options.Cache != null
               && options.Performance != null
               && options.Logging != null;
    }

    /// <summary>
    /// Gets whether the analyzer is enabled for execution.
    /// Returns true if the cache is enabled AND the analysis options indicate analysis should run.
    /// </summary>
    /// <param name="options">The SQL query analyzer options</param>
    /// <returns>True if analyzer is enabled; otherwise false</returns>
    public static bool IsAnalyzerEnabled(this SqlQueryAnalyzerOptions options)
    {
        if (options?.Cache == null)
        {
            return false;
        }

        return options.Cache.Enabled && options.Analysis != null;
    }

    /// <summary>
    /// Gets the effective provider name from the database options.
    /// Normalizes the provider name to lowercase for consistent comparison.
    /// </summary>
    /// <param name="options">The SQL query analyzer options</param>
    /// <returns>The normalized provider name</returns>
    public static string GetNormalizedProvider(this SqlQueryAnalyzerOptions options)
    {
        if (options?.Database == null || string.IsNullOrWhiteSpace(options.Database.Provider))
        {
            return "sqlserver"; // Default provider
        }

        return options.Database.Provider.Trim().ToLowerInvariant();
    }

    /// <summary>
    /// Determines if any of the critical analysis features are enabled.
    /// </summary>
    /// <param name="options">The SQL query analyzer options</param>
    /// <returns>True if any critical analysis feature is enabled; otherwise false</returns>
    public static bool HasCriticalAnalysisEnabled(this SqlQueryAnalyzerOptions options)
    {
        if (options?.Analysis == null)
        {
            return false;
        }

        return options.Analysis.DetectNPlusOne
               || options.Analysis.DetectMissingIndexes
               || options.Analysis.DetectJoinIssues
               || options.Analysis.AnalyzeExecutionPlans;
    }

    /// <summary>
    /// Gets the effective connection timeout in milliseconds.
    /// </summary>
    /// <param name="options">The SQL query analyzer options</param>
    /// <returns>The connection timeout in milliseconds</returns>
    public static int GetConnectionTimeoutMs(this SqlQueryAnalyzerOptions options)
    {
        if (options?.Database == null)
        {
            return 5000; // Default 5 seconds
        }

        return options.Database.ConnectionTimeoutSeconds * 1000;
    }

    /// <summary>
    /// Gets the effective maximum concurrent analysis threads.
    /// Ensures at least 1 thread and no more than 100.
    /// </summary>
    /// <param name="options">The SQL query analyzer options</param>
    /// <returns>The maximum concurrent threads</returns>
    public static int GetMaxConcurrentThreads(this SqlQueryAnalyzerOptions options)
    {
        if (options?.Analysis == null)
        {
            return Math.Min(Environment.ProcessorCount, 10);
        }

        return Math.Clamp(options.Analysis.MaxThreads, 1, 100);
    }

    /// <summary>
    /// Gets whether detailed logging is enabled based on configuration.
    /// Detailed logging is enabled if connection logging is enabled.
    /// </summary>
    /// <param name="options">The SQL query analyzer options</param>
    /// <returns>True if detailed logging should be enabled; otherwise false</returns>
    public static bool ShouldEnableDetailedLogging(this SqlQueryAnalyzerOptions options)
    {
        return options?.Database?.EnableConnectionLogging ?? false;
    }

    /// <summary>
    /// Gets the list of patterns to ignore, ensuring it's never null.
    /// </summary>
    /// <param name="options">The SQL query analyzer options</param>
    /// <returns>A non-null list of ignore patterns</returns>
    public static IReadOnlyList<string> GetIgnorePatterns(this SqlQueryAnalyzerOptions options)
    {
        if (options?.Analysis == null || options.Analysis.IgnorePatterns == null)
        {
            return Array.Empty<string>();
        }

        return options.Analysis.IgnorePatterns.AsReadOnly();
    }

    /// <summary>
    /// Gets whether execution plan analysis is enabled and should be performed.
    /// </summary>
    /// <param name="options">The SQL query analyzer options</param>
    /// <returns>True if execution plan analysis is enabled; otherwise false</returns>
    public static bool ShouldAnalyzeExecutionPlans(this SqlQueryAnalyzerOptions options)
    {
        return options?.Analysis?.AnalyzeExecutionPlans ?? false;
    }

    /// <summary>
    /// Gets the effective maximum query length limit.
    /// </summary>
    /// <param name="options">The SQL query analyzer options</param>
    /// <returns>The maximum query length in characters</returns>
    public static int GetMaxQueryLength(this SqlQueryAnalyzerOptions options)
    {
        if (options?.Performance == null)
        {
            return 1024 * 1024; // Default 1MB
        }

        return Math.Max(1024, options.Performance.MaxQueryLength);
    }
}