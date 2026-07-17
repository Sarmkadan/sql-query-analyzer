#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlQueryAnalyzer.DTOs;

/// <summary>
/// Provides extension methods for <see cref="AnalysisRequestDto"/> to enhance query analysis functionality.
/// </summary>
/// <remarks>
/// All extension methods follow idiomatic C# patterns with proper null safety, XML documentation,
/// and use modern C# features like expression-bodied members and pattern matching where appropriate.
/// </remarks>
public static class AnalysisRequestDtoExtensions
{
    /// <summary>
    /// Creates a normalized identifier for the analysis context based on the request properties.
    /// This identifier can be used for caching, logging, or correlation purposes.
    /// </summary>
    /// <param name="request">The analysis request.</param>
    /// <returns>A normalized identifier string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is null.</exception>
    public static string GetContextIdentifier(this AnalysisRequestDto request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return string.Join("_", GetContextParts(request).Append($"{request.QueryText?.Length ?? 0}"));
    }

    /// <summary>
    /// Determines whether this analysis request should include plan analysis.
    /// Returns true if either AnalyzePlan is explicitly true, or if ExecutionPlanXml is provided.
    /// </summary>
    /// <param name="request">The analysis request.</param>
    /// <returns>True if plan analysis should be performed; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is null.</exception>
    public static bool ShouldAnalyzePlan(this AnalysisRequestDto request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return request.AnalyzePlan || !string.IsNullOrWhiteSpace(request.ExecutionPlanXml);
    }

    /// <summary>
    /// Determines whether index suggestions should be generated for this analysis.
    /// Returns true if IncludeIndexSuggestions is true AND the query text is not empty.
    /// </summary>
    /// <param name="request">The analysis request.</param>
    /// <returns>True if index suggestions should be generated; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is null.</exception>
    public static bool ShouldIncludeIndexSuggestions(this AnalysisRequestDto request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return request.IncludeIndexSuggestions && !string.IsNullOrWhiteSpace(request.QueryText);
    }

    /// <summary>
    /// Determines whether fragmentation analysis should be performed for this request.
    /// Returns true if AnalyzeFragmentation is true.
    /// </summary>
    /// <param name="request">The analysis request.</param>
    /// <returns>True if fragmentation analysis should be performed; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is null.</exception>
    public static bool ShouldAnalyzeFragmentation(this AnalysisRequestDto request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return request.AnalyzeFragmentation;
    }

    /// <summary>
    /// Gets a display-friendly name for the analysis context.
    /// Combines application, module, and procedure information into a readable format.
    /// </summary>
    /// <param name="request">The analysis request.</param>
    /// <returns>A formatted context name.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is null.</exception>
    public static string GetContextDisplayName(this AnalysisRequestDto request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var parts = GetContextParts(request).ToList();
        return parts.Count == 0 ? "Ad-hoc Query" : string.Join(" - ", parts);
    }

    /// <summary>
    /// Gets a summary of the analysis configuration flags.
    /// Returns a human-readable string describing what analysis features are enabled.
    /// </summary>
    /// <param name="request">The analysis request.</param>
    /// <returns>A configuration summary string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is null.</exception>
    public static string GetConfigurationSummary(this AnalysisRequestDto request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var enabledFeatures = new List<string>();

        if (request.AnalyzePlan)
            enabledFeatures.Add("Plan Analysis");

        if (request.AnalyzeFragmentation)
            enabledFeatures.Add("Fragmentation Analysis");

        if (request.IncludeIndexSuggestions)
            enabledFeatures.Add("Index Suggestions");

        return enabledFeatures.Count == 0
            ? "Quick Analysis (no detailed checks)"
            : $"Analysis: {string.Join(", ", enabledFeatures)}";
    }

    /// <summary>
    /// Creates a deep copy of the analysis request.
    /// </summary>
    /// <param name="request">The analysis request to copy.</param>
    /// <returns>A new instance with the same property values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is null.</exception>
    public static AnalysisRequestDto Clone(this AnalysisRequestDto request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return new AnalysisRequestDto
        {
            QueryText = request.QueryText,
            ApplicationName = request.ApplicationName,
            ProcedureName = request.ProcedureName,
            ModuleName = request.ModuleName,
            IncludeIndexSuggestions = request.IncludeIndexSuggestions,
            AnalyzeFragmentation = request.AnalyzeFragmentation,
            AnalyzePlan = request.AnalyzePlan,
            ExecutionPlanXml = request.ExecutionPlanXml
        };
    }

    /// <summary>
    /// Extracts non-empty context parts from the request for display purposes.
    /// </summary>
    private static IEnumerable<string> GetContextParts(this AnalysisRequestDto request)
    {
        if (!string.IsNullOrWhiteSpace(request.ApplicationName))
            yield return request.ApplicationName;

        if (!string.IsNullOrWhiteSpace(request.ModuleName))
            yield return request.ModuleName;

        if (!string.IsNullOrWhiteSpace(request.ProcedureName))
            yield return request.ProcedureName;
    }
}