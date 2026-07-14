#nullable enable
using System;

namespace SqlQueryAnalyzer.Services;

/// <summary>
/// Extension methods for <see cref="AnalysisBuilder"/>.
/// </summary>
public static class AnalysisBuilderExtensions
{
    /// <summary>
    /// Configures the analysis request with both application and module contexts.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="applicationName">The application name.</param>
    /// <param name="moduleName">The module name.</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="applicationName"/> or <paramref name="moduleName"/> is null or empty.</exception>
    public static AnalysisBuilder WithContext(this AnalysisBuilder builder, string applicationName, string moduleName)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrEmpty(applicationName, nameof(applicationName));
        ArgumentException.ThrowIfNullOrEmpty(moduleName, nameof(moduleName));
        return builder.WithApplication(applicationName).WithModule(moduleName);
    }

    /// <summary>
    /// Configures the builder for a full diagnostic analysis including index suggestions, fragmentation analysis, and plan analysis.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is null.</exception>
    public static AnalysisBuilder WithFullDiagnosticAnalysis(this AnalysisBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.Full().IncludeIndexSuggestions(true).AnalyzeFragmentation(true).AnalyzePlan(true);
    }

    /// <summary>
    /// Configures the builder for a quick analysis with the provided query text.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="queryText">The query text to analyze.</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="queryText"/> is null or empty.</exception>
    public static AnalysisBuilder WithQuickQuery(this AnalysisBuilder builder, string queryText)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrEmpty(queryText, nameof(queryText));
        return builder.Quick().WithQuery(queryText);
    }
}
