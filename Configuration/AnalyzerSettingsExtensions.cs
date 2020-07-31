using System;
using System.Collections.Generic;
using System.Globalization;
using SqlQueryAnalyzer.Configuration;

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
    public static bool IsConnectionValid(this AnalyzerSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        return !string.IsNullOrWhiteSpace(settings.Database.ConnectionString);
    }

    /// <summary>
    /// Provides a summary string of the key settings in the current configuration.
    /// </summary>
    /// <param name="settings">The settings instance.</param>
    /// <returns>A formatted summary string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="settings"/> is null.</exception>
    public static string GetSummary(this AnalyzerSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        return string.Format(CultureInfo.InvariantCulture,
            "Provider: {0}, MaxThreads: {1}, DetectNPlusOne: {2}",
            settings.Database.Provider,
            settings.Analysis.MaxThreads,
            settings.Analysis.DetectNPlusOne);
    }
}
