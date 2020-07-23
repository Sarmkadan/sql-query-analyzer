using System;
using System.Collections.Generic;
using System.Globalization;

namespace SqlQueryAnalyzer.Models;

/// <summary>
/// Provides extension methods for the <see cref="QueryRewriteSuggestion"/> class.
/// </summary>
public static class QueryRewriteSuggestionExtensions
{
    /// <summary>
    /// Determines if the suggestion is considered high-impact (estimated improvement >= 20%).
    /// </summary>
    /// <param name="suggestion">The suggestion to analyze.</param>
    /// <returns><see langword="true"/> if the estimated improvement is high; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="suggestion"/> is null.</exception>
    public static bool IsHighImpact(this QueryRewriteSuggestion suggestion)
    {
        ArgumentNullException.ThrowIfNull(suggestion);
        return suggestion.EstimatedImprovementPercent >= 20.0;
    }

    /// <summary>
    /// Determines if the suggestion can be safely applied without human review.
    /// </summary>
    /// <param name="suggestion">The suggestion to analyze.</param>
    /// <returns><see langword="true"/> if auto-applicable and not a breaking change; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="suggestion"/> is null.</exception>
    public static bool IsSafelyApplicable(this QueryRewriteSuggestion suggestion)
    {
        ArgumentNullException.ThrowIfNull(suggestion);
        return suggestion.IsAutoApplicable && !suggestion.IsBreakingChange;
    }

    /// <summary>
    /// Gets a detailed summary of the suggestion including priority and risk level.
    /// </summary>
    /// <param name="suggestion">The suggestion to describe.</param>
    /// <returns>A detailed string summary.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="suggestion"/> is null.</exception>
    public static string ToDetailedSummary(this QueryRewriteSuggestion suggestion)
    {
        ArgumentNullException.ThrowIfNull(suggestion);
        return string.Format(CultureInfo.InvariantCulture,
            "ID: {0} | Priority: {1} | Risk: {2} | Summary: {3}",
            suggestion.SuggestionId,
            suggestion.Priority,
            suggestion.GetRiskLevel(),
            suggestion.GetSummary());
    }
}
