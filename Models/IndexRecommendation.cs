#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SqlQueryAnalyzer.Models;

/// <summary>
/// Represents a recommended index derived from query analysis.
/// </summary>
public sealed class IndexRecommendation
{
    /// <summary>Unique identifier for the recommendation.</summary>
    public string RecommendationId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Target table for the recommended index.</summary>
    public string TableName { get; set; } = string.Empty;

    /// <summary>Key columns that should define the index.</summary>
    public List<string> KeyColumns { get; set; } = [];

    /// <summary>Optional included columns that make the index covering.</summary>
    public List<string> IncludeColumns { get; set; } = [];

    /// <summary>Type of index to create.</summary>
    public string IndexType { get; set; } = "NONCLUSTERED";

    /// <summary>Estimated performance impact on a 0-100 scale.</summary>
    public double ImpactScore { get; set; }

    /// <summary>Human-readable explanation for the recommendation.</summary>
    public string Rationale { get; set; } = string.Empty;

    /// <summary>Generated CREATE INDEX script.</summary>
    public string GeneratedScript { get; set; } = string.Empty;

    /// <summary>Clause or heuristic that produced the recommendation.</summary>
    public RecommendationSource Source { get; set; }

    /// <summary>Timestamp when the recommendation was created.</summary>
    public DateTime RecommendedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Generates a CREATE INDEX statement for this recommendation.
    /// </summary>
    public void GenerateScript()
    {
        var cols = string.Join(", ", KeyColumns);
        var include = IncludeColumns.Count > 0 ? $" INCLUDE ({string.Join(", ", IncludeColumns)})" : string.Empty;
        var name = $"IX_{TableName}_{string.Join("_", KeyColumns)}";
        GeneratedScript = $"CREATE NONCLUSTERED INDEX {name} ON {TableName} ({cols}){include};";
    }
}

/// <summary>
/// Identifies the clause that produced an index recommendation.
/// </summary>
public enum RecommendationSource
{
    WhereClause,
    JoinCondition,
    OrderBy,
    GroupBy,
    Composite
}
