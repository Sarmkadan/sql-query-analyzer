#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using SqlQueryAnalyzer.Models;

namespace SqlQueryAnalyzer.Services;

/// <summary>
/// Collects and aggregates query execution statistics over time.
/// Tracks execution counts, durations, row counts, and identifies slow queries.
/// </summary>
public sealed class StatisticsCollector
{
    private readonly ILogger<StatisticsCollector> _logger;
    private readonly ConcurrentDictionary<string, QueryExecutionStats> _stats = new();
    private readonly object _lock = new();

    /// <summary>
    /// Initializes a new instance of <see cref="StatisticsCollector"/>.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    public StatisticsCollector(ILogger<StatisticsCollector> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Records a single query execution with its duration and row count.
    /// </summary>
    /// <param name="queryHash">A hash or identifier for the query template.</param>
    /// <param name="duration">How long the query took to execute.</param>
    /// <param name="rowsAffected">Number of rows returned or affected.</param>
    public void Record(string queryHash, TimeSpan duration, long rowsAffected)
    {
        if (string.IsNullOrEmpty(queryHash))
            return;

        _stats.AddOrUpdate(queryHash,
            _ => new QueryExecutionStats
            {
                QueryHash = queryHash,
                ExecutionCount = 1,
                TotalDuration = duration,
                MinDuration = duration,
                MaxDuration = duration,
                TotalRowsAffected = rowsAffected,
                FirstSeen = DateTimeOffset.UtcNow,
                LastSeen = DateTimeOffset.UtcNow
            },
            (_, existing) =>
            {
                lock (_lock)
                {
                    existing.ExecutionCount++;
                    existing.TotalDuration += duration;
                    if (duration < existing.MinDuration) existing.MinDuration = duration;
                    if (duration > existing.MaxDuration) existing.MaxDuration = duration;
                    existing.TotalRowsAffected += rowsAffected;
                    existing.LastSeen = DateTimeOffset.UtcNow;
                }
                return existing;
            });

        _logger.LogDebug("Recorded execution for {QueryHash}: {Duration}ms", queryHash, duration.TotalMilliseconds);
    }

    /// <summary>
    /// Returns statistics for a specific query by its hash.
    /// </summary>
    /// <param name="queryHash">The query identifier.</param>
    /// <returns>Execution stats, or null if not tracked.</returns>
    public QueryExecutionStats? GetStats(string queryHash)
    {
        return _stats.TryGetValue(queryHash, out var stats) ? stats : null;
    }

    /// <summary>
    /// Returns the top N slowest queries by average execution time.
    /// </summary>
    /// <param name="topN">Number of results to return.</param>
    /// <returns>Ordered list of slowest queries.</returns>
    public List<QueryExecutionStats> GetSlowestQueries(int topN = 10)
    {
        return _stats.Values
            .OrderByDescending(s => s.AverageDuration)
            .Take(topN)
            .ToList();
    }

    /// <summary>
    /// Returns the most frequently executed queries.
    /// </summary>
    /// <param name="topN">Number of results to return.</param>
    /// <returns>Ordered list of most executed queries.</returns>
    public List<QueryExecutionStats> GetMostFrequent(int topN = 10)
    {
        return _stats.Values
            .OrderByDescending(s => s.ExecutionCount)
            .Take(topN)
            .ToList();
    }

    /// <summary>
    /// Resets all collected statistics.
    /// </summary>
    public void Reset()
    {
        _stats.Clear();
        _logger.LogInformation("Statistics collector reset");
    }
}

/// <summary>
/// Aggregated execution statistics for a single query template.
/// </summary>
public sealed class QueryExecutionStats
{
    /// <summary>Unique hash identifying the query template.</summary>
    public string QueryHash { get; set; } = string.Empty;

    /// <summary>Total number of times this query was executed.</summary>
    public long ExecutionCount { get; set; }

    /// <summary>Sum of all execution durations.</summary>
    public TimeSpan TotalDuration { get; set; }

    /// <summary>Shortest observed execution time.</summary>
    public TimeSpan MinDuration { get; set; }

    /// <summary>Longest observed execution time.</summary>
    public TimeSpan MaxDuration { get; set; }

    /// <summary>Average execution time.</summary>
    public TimeSpan AverageDuration => ExecutionCount > 0
        ? TimeSpan.FromTicks(TotalDuration.Ticks / ExecutionCount)
        : TimeSpan.Zero;

    /// <summary>Total rows returned or affected across all executions.</summary>
    public long TotalRowsAffected { get; set; }

    /// <summary>When this query was first observed.</summary>
    public DateTimeOffset FirstSeen { get; set; }

    /// <summary>When this query was last observed.</summary>
    public DateTimeOffset LastSeen { get; set; }
}
