// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Generic;
using System.Linq;

namespace SqlQueryAnalyzer.Models;

/// <summary>
/// Represents an index suggestion to improve query performance
/// </summary>
public class IndexSuggestion
{
    public string SuggestionId { get; set; } = Guid.NewGuid().ToString();
    public string TableName { get; set; } = string.Empty;
    public string IndexName { get; set; } = string.Empty;
    public List<string> IndexColumns { get; set; } = [];
    public List<string> IncludeColumns { get; set; } = [];
    public string IndexType { get; set; } = "NONCLUSTERED"; // CLUSTERED, NONCLUSTERED, UNIQUE

    // Performance metrics
    public double EstimatedPerformanceGain { get; set; } // 0-100 scale (%)
    public double EstimatedExecutionTimeReduction { get; set; } // percentage
    public int? EstimatedIndexSizeKB { get; set; }
    public int? EstimatedMaintenanceCost { get; set; } // 1-10 scale

    // SQL generation
    public string GeneratedCreateScript { get; set; } = string.Empty;
    public string GeneratedDropScript { get; set; } = string.Empty;

    // Analysis data
    public int AffectedQueries { get; set; }
    public bool IsComposite => IndexColumns.Count > 1;
    public bool IsCovering => IncludeColumns.Count > 0;
    public DateTime SuggestedAt { get; set; } = DateTime.UtcNow;

    // Additional metadata
    public string Rationale { get; set; } = string.Empty;
    public List<string> ConflictingIndexes { get; set; } = [];
    public bool AlreadyExists { get; set; }

    // Validate suggestion
    public bool IsValid() =>
        !string.IsNullOrWhiteSpace(TableName) &&
        !string.IsNullOrWhiteSpace(IndexName) &&
        IndexColumns.Count > 0 &&
        EstimatedPerformanceGain > 0;

    // Generate suggested index name following conventions
    public void GenerateIndexName()
    {
        var columnPart = string.Join("_", IndexColumns);
        var typePrefix = IndexType switch
        {
            "CLUSTERED" => "CX",
            "UNIQUE" => "UX",
            _ => "IX"
        };
        IndexName = $"{typePrefix}_{TableName}_{columnPart}";
    }

    // Generate CREATE INDEX script
    public void GenerateCreateScript()
    {
        var columnList = string.Join(", ", IndexColumns);
        var includeClause = IncludeColumns.Count > 0
            ? $" INCLUDE ({string.Join(", ", IncludeColumns)})"
            : string.Empty;

        GeneratedCreateScript =
            $"CREATE {IndexType} INDEX {IndexName} " +
            $"ON {TableName} ({columnList}){includeClause};";
    }

    // Generate DROP INDEX script
    public void GenerateDropScript()
    {
        GeneratedDropScript = $"DROP INDEX IF EXISTS {IndexName} ON {TableName};";
    }

    // Get summary
    public string GetSummary() =>
        $"{IndexName} on {TableName} ({string.Join(", ", IndexColumns)}) - " +
        $"Est. gain: {EstimatedPerformanceGain:F1}%";

    // Check risk level for creating this index
    public string GetRiskLevel()
    {
        var maintenanceCost = EstimatedMaintenanceCost ?? 5;
        var sizeKB = EstimatedIndexSizeKB ?? 1000;

        if (maintenanceCost >= 8 || sizeKB > 100000)
            return "HIGH";
        if (maintenanceCost >= 5 || sizeKB > 10000)
            return "MEDIUM";
        return "LOW";
    }
}
