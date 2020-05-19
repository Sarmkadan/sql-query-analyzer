#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SqlQueryAnalyzer.Models;

/// <summary>
/// Represents a structured slow-query log entry.
/// </summary>
public sealed class SlowQueryEntry
{
    /// <summary>Unique identifier for the parsed entry.</summary>
    public string EntryId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>SQL text extracted from the log.</summary>
    public string QueryText { get; set; } = string.Empty;

    /// <summary>Total query duration.</summary>
    public TimeSpan Duration { get; set; }

    /// <summary>Time spent waiting on locks.</summary>
    public TimeSpan LockTime { get; set; }

    /// <summary>Rows examined by the query.</summary>
    public long RowsExamined { get; set; }

    /// <summary>Rows returned to the caller.</summary>
    public long RowsSent { get; set; }

    /// <summary>Timestamp of the logged execution.</summary>
    public DateTime Timestamp { get; set; }

    /// <summary>User and host information from the log entry.</summary>
    public string UserHost { get; set; } = string.Empty;

    /// <summary>Database name associated with the query.</summary>
    public string Database { get; set; } = string.Empty;

    /// <summary>Source engine for the entry.</summary>
    public string LogSource { get; set; } = string.Empty;

    /// <summary>Additional engine-specific attributes.</summary>
    public Dictionary<string, string> Metadata { get; set; } = [];

    /// <summary>Ratio of rows returned to rows examined.</summary>
    public double EfficiencyRatio => RowsExamined > 0 ? (double)RowsSent / RowsExamined : 0;

    /// <summary>Indicates whether the entry likely represents a full scan.</summary>
    public bool IsFullScan => RowsExamined > 0 && RowsSent > 0 && EfficiencyRatio < 0.01;

    /// <summary>
    /// Builds a short textual summary of the slow query entry.
    /// </summary>
    public string GetSummary() =>
        $"[{LogSource}] {Duration.TotalMilliseconds:F0}ms | rows: {RowsSent}/{RowsExamined} | {QueryText.Substring(0, Math.Min(60, QueryText.Length))}";
}
