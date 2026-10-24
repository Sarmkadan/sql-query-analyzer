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
    /// <summary>
    /// Gets or sets the index ID.
    /// </summary>
    public string IndexId { get; set; } = Guid.NewGuid().ToString();
    /// <summary>
    /// Gets or sets the index name.
    /// </summary>
    public string IndexName { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the table name.
    /// </summary>
    public string TableName { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the schema name.
    /// </summary>
    public string SchemaName { get; set; } = "dbo";
    /// <summary>
    /// Gets or sets the index type.
    /// </summary>
    public IndexType IndexType { get; set; } = IndexType.Nonclustered;
    /// <summary>
    /// Gets or sets a value indicating whether is unique.
    /// </summary>
    public bool IsUnique { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether is primary key.
    /// </summary>
    public bool IsPrimaryKey { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether is disabled.
    /// </summary>
    public bool IsDisabled { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether is filtered.
    /// </summary>
    public bool IsFiltered { get; set; }

    // Column information
    /// <summary>
    /// Gets or sets the columns.
    /// </summary>
    public List<IndexColumn> Columns { get; set; } = [];
    /// <summary>
    /// Gets or sets the include columns.
    /// </summary>
    public List<string> IncludeColumns { get; set; } = [];

    // Storage and performance
    /// <summary>
    /// Gets or sets the size in bytes.
    /// </summary>
    public long SizeInBytes { get; set; }
    /// <summary>
    /// Gets or sets the page count.
    /// </summary>
    public int PageCount { get; set; }
    /// <summary>
    /// Gets or sets the file group.
    /// </summary>
    public string FileGroup { get; set; } = "PRIMARY";
    /// <summary>
    /// Gets or sets the filter predicate.
    /// </summary>
    public string? FilterPredicate { get; set; }

    // Usage statistics
    /// <summary>
    /// Gets or sets the user seeks.
    /// </summary>
    public long UserSeeks { get; set; }
    /// <summary>
    /// Gets or sets the user scans.
    /// </summary>
    public long UserScans { get; set; }
    /// <summary>
    /// Gets or sets the user lookups.
    /// </summary>
    public long UserLookups { get; set; }
    /// <summary>
    /// Gets or sets the user updates.
    /// </summary>
    public long UserUpdates { get; set; }
    /// <summary>
    /// Gets or sets the last user seek time.
    /// </summary>
    public long LastUserSeekTime { get; set; }
    /// <summary>
    /// Gets or sets the last user scan time.
    /// </summary>
    public long LastUserScanTime { get; set; }

    // Fragmentation
    /// <summary>
    /// Gets or sets the fragmentation percentage.
    /// </summary>
    public double FragmentationPercentage { get; set; }
    /// <summary>
    /// The fragmentation percent.
    /// </summary>
    public double FragmentationPercent
    {
        get => FragmentationPercentage;
        set => FragmentationPercentage = value;
    }

    /// <summary>
    /// Gets or sets the fragment count.
    /// </summary>
    public int FragmentCount { get; set; }

    // Maintenance
    /// <summary>
    /// Gets or sets the created date.
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    /// <summary>
    /// Gets or sets the last modified date.
    /// </summary>
    public DateTime? LastModifiedDate { get; set; }
    /// <summary>
    /// Gets or sets the last statistics update.
    /// </summary>
    public DateTime? LastStatisticsUpdate { get; set; }
    /// <summary>
    /// Gets or sets the total maintenance operations.
    /// </summary>
    public int TotalMaintenanceOperations { get; set; }

    // Index health
    /// <summary>
    /// Gets or sets the health status.
    /// </summary>
    public IndexHealth HealthStatus { get; set; } = IndexHealth.Healthy;
    /// <summary>
    /// Gets or sets the health notes.
    /// </summary>
    public string? HealthNotes { get; set; }

    // Calculate total usage
    /// <summary>
    /// The total usage count.
    /// </summary>
    public long TotalUsageCount => UserSeeks + UserScans + UserLookups;

    // Check if index is being used
    /// <summary>
    /// Gets or sets a value indicating whether is used.
    /// </summary>
    public bool IsUsed => TotalUsageCount > 0;

    // Check if index should be considered for removal
    /// <summary>
    /// Gets or sets a value indicating whether is candidate for removal.
    /// </summary>
    public bool IsCandidateForRemoval => !IsUsed && !IsPrimaryKey && !IsUnique && UserUpdates > 0;

    // Get index display name
    /// <summary>
    /// Get qualified name.
    /// </summary>
    public string GetQualifiedName() => $"{SchemaName}.{TableName}.{IndexName}";

    // Get column list for display
    /// <summary>
    /// Get column list.
    /// </summary>
    public string GetColumnList() =>
        string.Join(", ", Columns.OrderBy(c => c.KeyOrdinal)
            .Select(c => $"{c.ColumnName}{(c.IsDescending ? " DESC" : "")}"));

    // Get include column list
    /// <summary>
    /// Get include list.
    /// </summary>
    public string GetIncludeList() =>
        IncludeColumns.Count > 0 ? $"INCLUDE ({string.Join(", ", IncludeColumns)})" : string.Empty;

    // Check if index is fragmented
    /// <summary>
    /// Gets or sets a value indicating whether is fragmented.
    /// </summary>
    public bool IsFragmented => FragmentationPercentage > 10;

    // Get fragmentation status
    /// <summary>
    /// Get fragmentation status.
    /// </summary>
    public string GetFragmentationStatus() => FragmentationPercentage switch
    {
        < 5 => "Optimal",
        < 10 => "Good",
        < 30 => "Moderate - Reorganize recommended",
        _ => "Severe - Rebuild recommended"
    };

    // Estimate maintenance cost
    /// <summary>
    /// Estimate cost.
    /// </summary>
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
    /// <summary>
    /// Generate rebuild script.
    /// </summary>
    public string GenerateRebuildScript() =>
        $"ALTER INDEX {IndexName} ON {SchemaName}.{TableName} REBUILD;";

    // Generate REORGANIZE script
    /// <summary>
    /// Generate reorganize script.
    /// </summary>
    public string GenerateReorganizeScript() =>
        $"ALTER INDEX {IndexName} ON {SchemaName}.{TableName} REORGANIZE;";

    // Generate CREATE script
    /// <summary>
    /// Generate create script.
    /// </summary>
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

    /// <summary>
    /// The size in KB.
    /// </summary>
    public long SizeInKB
    {
        get => SizeInBytes / 1024;
        set => SizeInBytes = value * 1024;
    }

    /// <summary>
    /// The unused days.
    /// </summary>
    public int UnusedDays => TotalUsageCount > 0 ? 0 : Math.Max(0, (int)(DateTime.UtcNow - CreatedDate).TotalDays);

    // Get usage summary
    /// <summary>
    /// Get usage summary.
    /// </summary>
    public string GetUsageSummary() =>
        $"Seeks: {UserSeeks:N0} | Scans: {UserScans:N0} | Lookups: {UserLookups:N0} | Updates: {UserUpdates:N0}";

    // Validate index
    /// <summary>
    /// Is valid.
    /// </summary>
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
    /// <summary>
    /// Gets or sets the column name.
    /// </summary>
    public string ColumnName { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the key ordinal.
    /// </summary>
    public int KeyOrdinal { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether is descending.
    /// </summary>
    public bool IsDescending { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether is included.
    /// </summary>
    public bool IsIncluded { get; set; }
}

/// <summary>
/// Index type enumeration
/// </summary>
public enum IndexType
{
    /// <summary>
    /// The clustered.
    /// </summary>
    Clustered = 1,
    /// <summary>
    /// The nonclustered.
    /// </summary>
    Nonclustered = 2,
    /// <summary>
    /// The unique.
    /// </summary>
    Unique = 3,
    /// <summary>
    /// The full text.
    /// </summary>
    FullText = 4,
    /// <summary>
    /// The spatial.
    /// </summary>
    Spatial = 5,
    /// <summary>
    /// The columnstore.
    /// </summary>
    Columnstore = 6
}

/// <summary>
/// Index health status
/// </summary>
public enum IndexHealth
{
    /// <summary>
    /// The healthy.
    /// </summary>
    Healthy = 0,
    /// <summary>
    /// The needs reorganization.
    /// </summary>
    NeedsReorganization = 1,
    /// <summary>
    /// The needs rebuild.
    /// </summary>
    NeedsRebuild = 2,
    /// <summary>
    /// The corrupted.
    /// </summary>
    Corrupted = 3,
    /// <summary>
    /// The unknown.
    /// </summary>
    Unknown = 4
}
