// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using SqlQueryAnalyzer.Models;
using SqlQueryAnalyzer.Services;

namespace SqlQueryAnalyzer.Extensions;

/// <summary>
/// Dependency-injection registration and LINQ-style convenience extensions
/// for the query rewrite feature.
/// </summary>
public static class QueryRewriteExtensions
{
    /// <summary>
    /// Registers <see cref="IQueryRewriteService"/> with the DI container as a singleton.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The same <paramref name="services"/> instance for chaining.</returns>
    public static IServiceCollection AddQueryRewriteService(this IServiceCollection services)
    {
        services.AddSingleton<IQueryRewriteService, QueryRewriteService>();
        return services;
    }

    /// <summary>
    /// Filters suggestions to those that are safe to apply programmatically without manual review.
    /// </summary>
    public static List<QueryRewriteSuggestion> GetAutoApplicable(
        this IEnumerable<QueryRewriteSuggestion> suggestions)
    {
        return suggestions.Where(s => s.IsAutoApplicable).ToList();
    }

    /// <summary>
    /// Filters suggestions that do not alter the observable result set.
    /// </summary>
    public static List<QueryRewriteSuggestion> GetNonBreaking(
        this IEnumerable<QueryRewriteSuggestion> suggestions)
    {
        return suggestions.Where(s => !s.IsBreakingChange).ToList();
    }

    /// <summary>
    /// Returns suggestions of a specific rewrite type.
    /// </summary>
    public static List<QueryRewriteSuggestion> OfType(
        this IEnumerable<QueryRewriteSuggestion> suggestions,
        RewriteType rewriteType)
    {
        return suggestions.Where(s => s.RewriteType == rewriteType).ToList();
    }

    /// <summary>
    /// Returns suggestions that target a specific SQL clause (e.g. "WHERE", "SELECT").
    /// </summary>
    public static List<QueryRewriteSuggestion> ForClause(
        this IEnumerable<QueryRewriteSuggestion> suggestions,
        string clause)
    {
        return suggestions
            .Where(s => s.AffectedClause.Equals(clause, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>
    /// Orders suggestions by estimated performance improvement, highest first.
    /// </summary>
    public static IOrderedEnumerable<QueryRewriteSuggestion> OrderByImpact(
        this IEnumerable<QueryRewriteSuggestion> suggestions)
    {
        return suggestions.OrderByDescending(s => s.EstimatedImprovementPercent);
    }

    /// <summary>
    /// Calculates the sum of all estimated improvements, capped at 100 %
    /// to avoid misleading compound percentages.
    /// </summary>
    public static double GetTotalEstimatedImprovement(
        this IEnumerable<QueryRewriteSuggestion> suggestions)
    {
        return Math.Min(100.0, suggestions.Sum(s => s.EstimatedImprovementPercent));
    }

    /// <summary>
    /// Collects all <see cref="IndexSuggestion"/> items embedded in the rewrite suggestions
    /// into a deduplicated, prioritised flat list.
    /// </summary>
    public static List<IndexSuggestion> GetAllIndexSuggestions(
        this IEnumerable<QueryRewriteSuggestion> suggestions)
    {
        return suggestions
            .SelectMany(s => s.RelatedIndexSuggestions)
            .GroupBy(i => i.IndexName, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .OrderByDescending(i => i.EstimatedPerformanceGain)
            .ToList();
    }

    /// <summary>
    /// Gets a human-readable summary of the full rewrite suggestion set.
    /// </summary>
    public static string GetRewriteSummary(
        this IEnumerable<QueryRewriteSuggestion> suggestions)
    {
        var list = suggestions.ToList();
        if (list.Count == 0)
            return "No rewrite suggestions — query structure is already optimal.";

        var totalImprovement = list.GetTotalEstimatedImprovement();
        var types = string.Join(", ", list.Select(s => s.RewriteType.ToString()).Distinct());

        return $"{list.Count} rewrite suggestion(s) identified ({types}). " +
               $"Total estimated improvement: {totalImprovement:F1}%.";
    }
}
