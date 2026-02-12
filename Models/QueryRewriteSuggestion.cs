#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

namespace SqlQueryAnalyzer.Models;

/// <summary>
/// Kinds of SQL query transformations that can be suggested to improve performance.
/// </summary>
public enum RewriteType
{
    /// <summary>Replace SELECT * with an explicit column list.</summary>
    ExplicitColumnSelection = 1,

    /// <summary>Convert an IN (SELECT ...) or correlated subquery to a JOIN.</summary>
    SubqueryToJoin = 2,

    /// <summary>Replace OR predicates with UNION ALL to allow individual index seeks.</summary>
    OrToUnionAll = 3,

    /// <summary>Move a scalar function off an indexed column to restore SARGability.</summary>
    FunctionSargability = 4,

    /// <summary>Replace LIKE with a leading wildcard with a full-text CONTAINS predicate.</summary>
    LikeToFullText = 5,

    /// <summary>Add TOP / FETCH NEXT to constrain an unbounded result set.</summary>
    ResultSetPagination = 6,

    /// <summary>Replace UNION (implicit DISTINCT sort) with UNION ALL when duplicates are acceptable.</summary>
    UnionToUnionAll = 7,

    /// <summary>Add an explicit index hint to guide the query optimizer.</summary>
    IndexHint = 8,

    /// <summary>Rewrite NOT IN (SELECT ...) to NOT EXISTS for correct NULL handling and better plans.</summary>
    NotInToNotExists = 9
}

/// <summary>
/// A suggested SQL query rewrite together with the index recommendations
/// that make the transformation most effective.
/// </summary>
public sealed class QueryRewriteSuggestion
{
    /// <summary>Gets or sets the unique identifier for this suggestion.</summary>
    public string SuggestionId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Gets or sets the original SQL text.</summary>
    public string OriginalQuery { get; set; } = string.Empty;

    /// <summary>Gets or sets the rewritten SQL text or annotated template.</summary>
    public string RewrittenQuery { get; set; } = string.Empty;

    /// <summary>Gets or sets the kind of transformation applied.</summary>
    public RewriteType RewriteType { get; set; }

    /// <summary>Gets or sets the SQL clause primarily targeted (SELECT, WHERE, JOIN, UNION, etc.).</summary>
    public string AffectedClause { get; set; } = string.Empty;

    /// <summary>Gets or sets the human-readable rationale for this rewrite.</summary>
    public string Rationale { get; set; } = string.Empty;

    /// <summary>Gets or sets additional caveats or implementation notes for the developer.</summary>
    public string AdditionalNotes { get; set; } = string.Empty;

    /// <summary>Gets or sets the estimated performance improvement as a percentage (0–100).</summary>
    public double EstimatedImprovementPercent { get; set; }

    /// <summary>Gets or sets whether applying this rewrite changes the observable result set.</summary>
    public bool IsBreakingChange { get; set; }

    /// <summary>Gets or sets whether the rewrite is safe to apply programmatically without manual review.</summary>
    public bool IsAutoApplicable { get; set; }

    /// <summary>Gets or sets the suggestion priority (1 = highest).</summary>
    public int Priority { get; set; }

    /// <summary>Gets or sets index suggestions that complement this rewrite.</summary>
    public List<IndexSuggestion> RelatedIndexSuggestions { get; set; } = [];

    /// <summary>Gets or sets when this suggestion was generated.</summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Returns <see langword="true"/> when the suggestion contains enough data to be acted on.</summary>
    public bool IsValid() =>
        !string.IsNullOrWhiteSpace(RewrittenQuery) &&
        !string.IsNullOrWhiteSpace(Rationale) &&
        RewrittenQuery != OriginalQuery;

    /// <summary>Gets the risk level for applying this rewrite: LOW, MEDIUM, or HIGH.</summary>
    public string GetRiskLevel()
    {
        if (IsBreakingChange) return "HIGH";
        if (!IsAutoApplicable) return "MEDIUM";
        return "LOW";
    }

    /// <summary>Gets a one-line human-readable summary of this suggestion.</summary>
    public string GetSummary() =>
        $"[{RewriteType}] {Rationale} — Est. improvement: {EstimatedImprovementPercent:F1}%";

    /// <summary>Gets a structured dictionary suitable for JSON serialization.</summary>
    public Dictionary<string, object> ToJsonDictionary() =>
        new()
        {
            { "suggestionId", SuggestionId },
            { "rewriteType", RewriteType.ToString() },
            { "affectedClause", AffectedClause },
            { "rationale", Rationale },
            { "estimatedImprovementPercent", EstimatedImprovementPercent },
            { "isBreakingChange", IsBreakingChange },
            { "isAutoApplicable", IsAutoApplicable },
            { "priority", Priority },
            { "riskLevel", GetRiskLevel() },
            { "relatedIndexSuggestions", RelatedIndexSuggestions.Count }
        };
}
