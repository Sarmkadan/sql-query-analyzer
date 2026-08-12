#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;

namespace SqlQueryAnalyzer.Utilities;

/// <summary>
/// Collects and aggregates performance metrics across the analyzer.
/// Tracks analysis times, cache performance, error rates, throughput.
/// Provides insights for optimization and monitoring.
/// </summary>
public class PerformanceMetricCollector
{
    private readonly ILogger<PerformanceMetricCollector> _logger;
    private readonly List<AnalysisMetric> _metrics = new();
    private readonly object _lock = new object();
    private DateTime _startTime = DateTime.UtcNow;

    public PerformanceMetricCollector(ILogger<PerformanceMetricCollector> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Records an analysis metric.
    /// </summary>
    public void RecordAnalysisMetric(
        string queryId,
        double executionTimeMs,
        int issueCount,
        bool cacheHit,
        bool successful)
    {
        _logger.LogInformation("Recording analysis metric for {QueryId}: {ExecutionTimeMs}ms, {IssueCount} issues, cache hit: {CacheHit}, successful: {Successful}",
            queryId, executionTimeMs, issueCount, cacheHit, successful);

        lock (_lock)
        {
            _metrics.Add(new AnalysisMetric
            {
                QueryId = queryId,
                ExecutionTimeMs = executionTimeMs,
                IssueCount = issueCount,
                CacheHit = cacheHit,
                Successful = successful,
                Timestamp = DateTime.UtcNow
            });
        }

        _logger.LogInformation("Analysis metric recorded for {QueryId}", queryId);
    }

    /// <summary>
    /// Calculates average analysis time across all metrics.
    /// </summary>
    public double GetAverageExecutionTimeMs()
    {
        lock (_lock)
        {
            return _metrics.Count > 0
                ? _metrics.Average(m => m.ExecutionTimeMs)
                : 0;
        }
    }

    /// <summary>
    /// Calculates cache hit ratio.
    /// </summary>
    public double GetCacheHitRatio()
    {
        lock (_lock)
        {
            if (_metrics.Count == 0)
                return 0;

            var hits = _metrics.Count(m => m.CacheHit);
            return hits * 100.0 / _metrics.Count;
        }
    }

    /// <summary>
    /// Calculates success rate of analyses.
    /// </summary>
    public double GetSuccessRate()
    {
        lock (_lock)
        {
            if (_metrics.Count == 0)
                return 100;

            var successful = _metrics.Count(m => m.Successful);
            return successful * 100.0 / _metrics.Count;
        }
    }

    /// <summary>
    /// Gets throughput (queries analyzed per second).
    /// </summary>
    public double GetThroughput()
    {
        lock (_lock)
        {
            var elapsed = DateTime.UtcNow - _startTime;
            if (elapsed.TotalSeconds == 0)
                return 0;

            return _metrics.Count / elapsed.TotalSeconds;
        }
    }

    /// <summary>
    /// Gets percentile execution time.
    /// </summary>
    public double GetPercentileExecutionTime(double percentile)
    {
        lock (_lock)
        {
            if (_metrics.Count == 0)
                return 0;

            var sorted = _metrics.OrderBy(m => m.ExecutionTimeMs).ToList();
            var index = (int)Math.Ceiling(percentile / 100.0 * sorted.Count) - 1;
            return sorted[Math.Max(0, index)].ExecutionTimeMs;
        }
    }

    /// <summary>
    /// Gets performance report for diagnostics.
    /// </summary>
    public PerformanceReport GetReport()
    {
        lock (_lock)
        {
            return new PerformanceReport
            {
                TotalAnalyses = _metrics.Count,
                SuccessfulAnalyses = _metrics.Count(m => m.Successful),
                FailedAnalyses = _metrics.Count(m => !m.Successful),
                CacheHits = _metrics.Count(m => m.CacheHit),
                CacheMisses = _metrics.Count(m => !m.CacheHit),
                AverageExecutionTimeMs = GetAverageExecutionTimeMs(),
                P50ExecutionTimeMs = GetPercentileExecutionTime(50),
                P95ExecutionTimeMs = GetPercentileExecutionTime(95),
                P99ExecutionTimeMs = GetPercentileExecutionTime(99),
                CacheHitRatio = GetCacheHitRatio(),
                SuccessRate = GetSuccessRate(),
                Throughput = GetThroughput(),
                TotalIssuesDetected = _metrics.Sum(m => m.IssueCount),
                CollectionDuration = DateTime.UtcNow - _startTime
            };
        }
    }

    /// <summary>
    /// Resets all collected metrics.
    /// </summary>
    public void Reset()
    {
        lock (_lock)
        {
            _metrics.Clear();
            _startTime = DateTime.UtcNow;
            _logger.LogInformation("Performance metrics reset");
        }
    }

    /// <summary>
    /// Gets count of collected metrics.
    /// </summary>
    public int Count
    {
        get
        {
            lock (_lock)
            {
                return _metrics.Count;
            }
        }
    }
}

/// <summary>
/// Individual analysis metric record.
/// </summary>
internal class AnalysisMetric
{
    public string QueryId { get; set; } = string.Empty;
    public double ExecutionTimeMs { get; set; }
    public int IssueCount { get; set; }
    public bool CacheHit { get; set; }
    public bool Successful { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Comprehensive performance report.
/// </summary>
public class PerformanceReport
{
    public int TotalAnalyses { get; set; }
    public int SuccessfulAnalyses { get; set; }
    public int FailedAnalyses { get; set; }
    public int CacheHits { get; set; }
    public int CacheMisses { get; set; }
    public double AverageExecutionTimeMs { get; set; }
    public double P50ExecutionTimeMs { get; set; }
    public double P95ExecutionTimeMs { get; set; }
    public double P99ExecutionTimeMs { get; set; }
    public double CacheHitRatio { get; set; }
    public double SuccessRate { get; set; }
    public double Throughput { get; set; }
    public int TotalIssuesDetected { get; set; }
    public TimeSpan CollectionDuration { get; set; }

    public override string ToString() =>
        $@"Performance Report:
  Total Analyses: {TotalAnalyses} ({SuccessfulAnalyses} successful, {FailedAnalyses} failed)
  Execution Time: avg={AverageExecutionTimeMs:F2}ms, p50={P50ExecutionTimeMs:F2}ms, p95={P95ExecutionTimeMs:F2}ms, p99={P99ExecutionTimeMs:F2}ms
  Cache: {CacheHits} hits, {CacheMisses} misses ({CacheHitRatio:F1}% ratio)
  Success Rate: {SuccessRate:F1}%
  Throughput: {Throughput:F2} queries/sec
  Total Issues Detected: {TotalIssuesDetected}
  Collection Duration: {CollectionDuration.TotalSeconds:F1}s";
}
