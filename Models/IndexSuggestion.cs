#nullable enable
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
public sealed class IndexSuggestion
{
    /// <summary>
    /// Gets or sets the suggestion ID.
    /// </summary>
    public string SuggestionId { get; set; } = Guid.NewGuid().ToString();
    /// <summary>
    /// Gets or sets the table name.
    /// </summary>
    public string TableName { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the index name.
    /// </summary>
    public string IndexName { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the index columns.
    /// </summary>
    public List<string> IndexColumns { get; set; } = [];
    /// <summary>
    /// Gets or sets the include columns.
    /// </summary>
    public List<string> IncludeColumns { get; set; } = [];
    /// <summary>
    /// Gets or sets the index type.
    /// </summary>
    public string IndexType { get; set; } = "NONCLUSTERED"; // CLUSTERED, NONCLUSTERED, UNIQUE

    // Performance metrics
    /// <summary>
    /// Gets or sets the estimated performance gain.
    /// </summary>
    public double EstimatedPerformanceGain { get; set; } // 0-100 scale (%)
    /// <summary>
    /// Gets or sets the estimated execution time reduction.
    /// </summary>
    public double EstimatedExecutionTimeReduction { get; set; } // percentage
    /// <summary>
    /// Gets or sets the estimated index size KB.
    /// </summary>
    public int? EstimatedIndexSizeKB { get; set; }
    /// <summary>
    /// Gets or sets the estimated maintenance cost.
    /// </summary>
    public int? EstimatedMaintenanceCost { get; set; } // 1-10 scale

    // SQL generation
    /// <summary>
    /// Gets or sets the generated create script.
    /// </summary>
    public string GeneratedCreateScript { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the generated drop script.
    /// </summary>
    public string GeneratedDropScript { get; set; } = string.Empty;

    // Analysis data
    /// <summary>
    /// Gets or sets the affected queries.
    /// </summary>
    public int AffectedQueries { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether is composite.
    /// </summary>
    public bool IsComposite => IndexColumns.Count > 1;
    /// <summary>
    /// Gets or sets a value indicating whether is covering.
    /// </summary>
    public bool IsCovering => IncludeColumns.Count > 0;
    /// <summary>
    /// Gets or sets the suggested at.
    /// </summary>
    public DateTime SuggestedAt { get; set; } = DateTime.UtcNow;

    // Additional metadata
    /// <summary>
    /// Gets or sets the rationale.
    /// </summary>
    public string Rationale { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the conflicting indexes.
    /// </summary>
    public List<string> ConflictingIndexes { get; set; } = [];
    /// <summary>
    /// Gets or sets a value indicating whether already exists.
    /// </summary>
    public bool AlreadyExists { get; set; }

    /// <summary>
    /// The column name.
    /// </summary>
    public string? ColumnName
    {
        get => IndexColumns.FirstOrDefault();
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            if (IndexColumns.Count == 0)
                IndexColumns.Add(value);
            else
                IndexColumns[0] = value;
        }
    }

    /// <summary>
    /// The columns.
    /// </summary>
    public List<string> Columns
    {
        get => IndexColumns;
        set => IndexColumns = value ?? [];
    }

    /// <summary>
    /// The included columns.
    /// </summary>
    public List<string> IncludedColumns
    {
        get => IncludeColumns;
        set => IncludeColumns = value ?? [];
    }

    /// <summary>
    /// The suggested index name.
    /// </summary>
    public string SuggestedIndexName
    {
        get => IndexName;
        set => IndexName = value;
    }

    /// <summary>
    /// The roi.
    /// </summary>
    public double Roi
    {
        get => EstimatedPerformanceGain;
        set => EstimatedPerformanceGain = value;
    }

    /// <summary>
    /// The estimated size KB.
    /// </summary>
    public int EstimatedSizeKB
    {
        get => EstimatedIndexSizeKB ?? 0;
        set => EstimatedIndexSizeKB = value;
    }

    /// <summary>
    /// The estimated improvement percent.
    /// </summary>
    public double EstimatedImprovementPercent
    {
        get => EstimatedExecutionTimeReduction;
        set => EstimatedExecutionTimeReduction = value;
    }

    // Validate suggestion
    /// <summary>
    /// Is valid.
    /// </summary>
    public bool IsValid() =>
        !string.IsNullOrWhiteSpace(TableName) &&
        !string.IsNullOrWhiteSpace(IndexName) &&
        IndexColumns.Count > 0 &&
        EstimatedPerformanceGain > 0;

    // Generate suggested index name following conventions
    /// <summary>
    /// Generate index name.
    /// </summary>
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
    /// <summary>
    /// Generate create script.
    /// </summary>
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
    /// <summary>
    /// Generate drop script.
    /// </summary>
    public void GenerateDropScript()
    {
        GeneratedDropScript = $"DROP INDEX IF EXISTS {IndexName} ON {TableName};";
    }

    // Get summary
    /// <summary>
    /// Get summary.
    /// </summary>
    public string GetSummary() =>
        $"{IndexName} on {TableName} ({string.Join(", ", IndexColumns)}) - " +
        $"Est. gain: {EstimatedPerformanceGain:F1}%";

    // Check risk level for creating this index
    /// <summary>
    /// Get risk level.
    /// </summary>
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

    /// <summary>
    /// To create index SQL.
    /// </summary>
    public string ToCreateIndexSql()
    {
        if (string.IsNullOrWhiteSpace(IndexName))
            GenerateIndexName();

        if (string.IsNullOrWhiteSpace(GeneratedCreateScript))
            GenerateCreateScript();

        return GeneratedCreateScript;
    }
}
