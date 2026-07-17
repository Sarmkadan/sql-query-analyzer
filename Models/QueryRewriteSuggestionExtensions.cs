using System;
using System.Collections.Generic;

namespace SqlQueryAnalyzer.Models;

/// <summary>
/// Provides extension methods for the <see cref="QueryRewriteSuggestion"/> class
/// to analyze and describe query rewrite suggestions.
/// </summary>
public static class QueryRewriteSuggestionExtensions
{
    /// <summary>
    /// Determines whether the suggestion is considered high-impact based on the estimated improvement.
    /// </summary>
    /// <param name="suggestion">The suggestion to analyze. Must not be <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the estimated improvement is 20% or greater; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><inheritdoc cref="ArgumentNullException.ThrowIfNull" path="/param[@name='suggestion']"/></exception>
    public static bool IsHighImpact(this QueryRewriteSuggestion suggestion)
    {
        ArgumentNullException.ThrowIfNull(suggestion);
        return suggestion.EstimatedImprovementPercent >= 20.0;
    }

    /// <summary>
    /// Determines whether the suggestion can be safely applied without human review.
    /// A suggestion is safely applicable when it is marked as auto-applicable and is not a breaking change.
    /// </summary>
    /// <param name="suggestion">The suggestion to analyze. Must not be <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the suggestion is auto-applicable and not a breaking change; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><inheritdoc cref="ArgumentNullException.ThrowIfNull" path="/param[@name='suggestion']"/></exception>
    public static bool IsSafelyApplicable(this QueryRewriteSuggestion suggestion)
    {
        ArgumentNullException.ThrowIfNull(suggestion);
        return suggestion.IsAutoApplicable && !suggestion.IsBreakingChange;
    }

    /// <summary>
    /// Gets a detailed summary of the suggestion including identifier, priority, risk level, and summary text.
    /// </summary>
    /// <param name="suggestion">The suggestion to describe. Must not be <see langword="null"/>.</param>
    /// <returns>A detailed string summary containing the suggestion ID, priority, risk level, and summary.</returns>
    /// <exception cref="ArgumentNullException"><inheritdoc cref="ArgumentNullException.ThrowIfNull" path="/param[@name='suggestion']"/></exception>
    public static string ToDetailedSummary(this QueryRewriteSuggestion suggestion)
    {
        ArgumentNullException.ThrowIfNull(suggestion);
        return $"ID: {suggestion.SuggestionId} | Priority: {suggestion.Priority} | Risk: {suggestion.GetRiskLevel()} | Summary: {suggestion.GetSummary()}";
    }
}
