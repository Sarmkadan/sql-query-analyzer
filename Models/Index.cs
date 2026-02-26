#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlQueryAnalyzer.Models;

/// <summary>
/// Represents a database index
/// </summary>
public sealed class Index
{
    public string IndexId { get; set; } = Guid.NewGuid().ToString();
    public string IndexName { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public string SchemaName { get; set; } = "dbo";
    public IndexType IndexType { get; set; } = IndexType.Nonclustered;
    public bool IsUnique { get; set; }
    public bool IsPrimaryKey { get; set; }
    public bool IsDisabled { get; set; }
    public bool IsFiltered { get; set; }

    // Column information
    public List<IndexColumn> Columns { get; set; } = [];
    public List<string> IncludeColumns { get; set; } = [];

    // Storage and performance
    public long SizeInBytes { get; set; }
    public int PageCount { get; set; }
    public string FileGroup { get; set; } = "PRIMARY";
    public string? FilterPredicate { get; set; }

    // Usage statistics
    public long UserSeeks { get; set; }
    public long UserScans { get; set; }
    public long UserLookups { get; set; }
    public long UserUpdates { get; set; }
    public long LastUserSeekTime { get; set; }
    public long LastUserScanTime { get; set; }

    // Fragmentation
    public double FragmentationPercentage { get; set; }
    public int FragmentCount { get; set; }

    // Maintenance
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? LastModifiedDate { get; set; }
    public DateTime? LastStatisticsUpdate { get; set; }
    public int TotalMaintenanceOperations { get; set; }

    // Index health
    public IndexHealth HealthStatus { get; set; } = IndexHealth.Healthy;
    public string? HealthNotes { get; set; }

    // Calculate total usage
    public long TotalUsageCount => UserSeeks + UserScans + UserLookups;

    // Check if index is being used
    public bool IsUsed => TotalUsageCount > 0;

    // Check if index should be considered for removal
    public bool IsCandidateForRemoval => !IsUsed && !IsPrimaryKey && !IsUnique && UserUpdates > 0;

    // Get index display name
    public string GetQualifiedName() => $"{SchemaName}.{TableName}.{IndexName}";

    // Get column list for display
    public string GetColumnList() =>
        string.Join(", ", Columns.OrderBy(c => c.KeyOrdinal)
            .Select(c => $"{c.ColumnName}{(c.IsDescending ? " DESC" : "")}"));

    // Get include column list
    public string GetIncludeList() =>
        IncludeColumns.Count > 0 ? $"INCLUDE ({string.Join(", ", IncludeColumns)})" : string.Empty;

    // Check if index is fragmented
    public bool IsFragmented => FragmentationPercentage > 10;

    // Get fragmentation status
    public string GetFragmentationStatus() => FragmentationPercentage switch
    {
        < 5 => "Optimal",
        < 10 => "Good",
        < 30 => "Moderate - Reorganize recommended",
        _ => "Severe - Rebuild recommended"
    };

    // Estimate maintenance cost
    public int EstimateCost()
    {
        var cost = 1;

        // Based on size
        if (SizeInBytes > 1000000000) // 1GB
            cost += 3;
        else if (SizeInBytes > 100000000) // 100MB
            cost += 2;

        // Based on updates
        if (UserUpdates > 10000)
            cost += 2;

        // Based on fragmentation
        if (IsFragmented)
            cost += 1;

        return Math.Min(10, cost);
    }

    // Generate REBUILD script
    public string GenerateRebuildScript() =>
        $"ALTER INDEX {IndexName} ON {SchemaName}.{TableName} REBUILD;";

    // Generate REORGANIZE script
    public string GenerateReorganizeScript() =>
        $"ALTER INDEX {IndexName} ON {SchemaName}.{TableName} REORGANIZE;";

    // Generate CREATE script
    public string GenerateCreateScript()
    {
        var typeKeyword = IndexType switch
        {
            IndexType.Clustered => "CLUSTERED",
            IndexType.Unique => "UNIQUE NONCLUSTERED",
            _ => "NONCLUSTERED"
        };

        var columnList = GetColumnList();
        var includeClause = GetIncludeList();
        var uniqueKeyword = IsUnique ? "UNIQUE" : string.Empty;
        var filterClause = IsFiltered && FilterPredicate != null ? $" WHERE {FilterPredicate}" : string.Empty;

        return $"CREATE {uniqueKeyword} {typeKeyword} INDEX {IndexName} " +
               $"ON {SchemaName}.{TableName} ({columnList}) " +
               $"{includeClause}{filterClause};";
    }

    // Get usage summary
    public string GetUsageSummary() =>
        $"Seeks: {UserSeeks:N0} | Scans: {UserScans:N0} | Lookups: {UserLookups:N0} | Updates: {UserUpdates:N0}";

    // Validate index
    public bool IsValid() =>
        !string.IsNullOrWhiteSpace(IndexName) &&
        !string.IsNullOrWhiteSpace(TableName) &&
        Columns.Count > 0;
}

/// <summary>
/// Represents a column in an index
/// </summary>
public sealed class IndexColumn
{
    public string ColumnName { get; set; } = string.Empty;
    public int KeyOrdinal { get; set; }
    public bool IsDescending { get; set; }
    public bool IsIncluded { get; set; }
}

/// <summary>
/// Index type enumeration
/// </summary>
public enum IndexType
{
    Clustered = 1,
    Nonclustered = 2,
    Unique = 3,
    FullText = 4,
    Spatial = 5,
    Columnstore = 6
}

/// <summary>
/// Index health status
/// </summary>
public enum IndexHealth
{
    Healthy = 0,
    NeedsReorganization = 1,
    NeedsRebuild = 2,
    Corrupted = 3,
    Unknown = 4
}
