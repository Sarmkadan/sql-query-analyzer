using System;
using System.Collections.Generic;
using System.Globalization;

namespace SqlQueryAnalyzer.Configuration;

/// <summary>
/// Provides extension methods for <see cref="AnalyzerSettings"/> to simplify common configuration tasks.
/// </summary>
public static class AnalyzerSettingsExtensions
{
    /// <summary>
    /// Gets a list of enabled analysis features.
    /// </summary>
    /// <param name="settings">The settings instance.</param>
    /// <returns>A read-only list of enabled feature names.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="settings"/> is null.</exception>
    public static IReadOnlyList<string> GetActiveDetectionFeatures(this AnalyzerSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var features = new List<string>();
        if (settings.Analysis.DetectNPlusOne) features.Add("DetectNPlusOne");
        if (settings.Analysis.DetectMissingIndexes) features.Add("DetectMissingIndexes");
        if (settings.Analysis.DetectJoinIssues) features.Add("DetectJoinIssues");
        if (settings.Analysis.AnalyzeExecutionPlans) features.Add("AnalyzeExecutionPlans");

        return features.AsReadOnly();
    }

    /// <summary>
    /// Validates if the database connection string is properly configured.
    /// </summary>
    /// <param name="settings">The settings instance.</param>
    /// <returns>True if the connection string is not null or whitespace; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="settings"/> is null.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="settings.Database"/> is null.</exception>
    public static bool IsConnectionValid(this AnalyzerSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(settings.Database);

        return !string.IsNullOrWhiteSpace(settings.Database.ConnectionString);
    }

    /// <summary>
    /// Provides a comprehensive summary string of the key settings in the current configuration.
    /// Includes database provider, analysis settings, and cache configuration.
    /// </summary>
    /// <param name="settings">The settings instance.</param>
    /// <returns>A formatted summary string with invariant culture formatting.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="settings"/> is null.</exception>
    public static string GetSummary(this AnalyzerSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        return string.Format(
            CultureInfo.InvariantCulture,
            "Database: {0}, ConnectionPool: {1}, MaxThreads: {2}, Cache: {3} ({4}), Analysis: NPlusOne={5}, MissingIndexes={6}, JoinIssues={7}, ExecutionPlans={8}",
            settings.Database.Provider,
            settings.Database.ConnectionPoolSize,
            settings.Analysis.MaxThreads,
            settings.Cache.Enabled ? "Enabled" : "Disabled",
            settings.Cache.Provider,
            settings.Analysis.DetectNPlusOne,
            settings.Analysis.DetectMissingIndexes,
            settings.Analysis.DetectJoinIssues,
            settings.Analysis.AnalyzeExecutionPlans);
    }
}
