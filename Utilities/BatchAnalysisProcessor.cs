#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;
using SqlQueryAnalyzer.Models;
using SqlQueryAnalyzer.Services;

namespace SqlQueryAnalyzer.Utilities;

/// <summary>
/// Processes multiple queries for analysis in batch mode.
/// Implements parallel processing with configurable thread count.
/// Handles errors gracefully and provides progress tracking.
/// </summary>
public class BatchAnalysisProcessor
{
    private readonly IQueryAnalyzerService _analyzerService;
    private readonly ILogger<BatchAnalysisProcessor> _logger;
    private int _maxParallel;
    private Progress<BatchProgress>? _progressReporter;

    public BatchAnalysisProcessor(
        IQueryAnalyzerService analyzerService,
        ILogger<BatchAnalysisProcessor> logger,
        int maxParallel = 0)
    {
        _analyzerService = analyzerService;
        _logger = logger;
        _maxParallel = maxParallel > 0 ? maxParallel : Environment.ProcessorCount;
    }

    /// <summary>
    /// Analyzes a batch of queries in parallel.
    /// Returns list of analysis results in input order.
    /// </summary>
    public async Task<List<QueryAnalysisResult>> AnalyzeBatchAsync(
        string[] queries,
        Action<BatchProgress>? onProgress = null,
        CancellationToken cancellationToken = default)
    {
        if (queries == null || queries.Length == 0)
        {
            return new List<QueryAnalysisResult>();
        }

        _progressReporter = onProgress != null ? new Progress<BatchProgress>(onProgress) : null;

        _logger.LogInformation($"Starting batch analysis of {queries.Length} queries with {_maxParallel} threads");

        var results = new QueryAnalysisResult[queries.Length];
        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = _maxParallel,
            CancellationToken = cancellationToken
        };

        var processedCount = 0;
        var lockObj = new object();

        try
        {
            await Parallel.ForEachAsync(
                Enumerable.Range(0, queries.Length),
                options,
                async (index, ct) =>
                {
                    try
                    {
                        var query = queries[index];
                        _logger.LogDebug($"Analyzing query {index + 1}/{queries.Length}");

                        results[index] = await _analyzerService.AnalyzeQueryAsync(query);

                        lock (lockObj)
                        {
                            processedCount++;
                            _progressReporter?.Report(new BatchProgress
                            {
                                ProcessedCount = processedCount,
                                TotalCount = queries.Length,
                                CurrentQueryIndex = index,
                                PercentComplete = (processedCount * 100.0) / queries.Length
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error analyzing query at index {index}");
                        results[index] = new QueryAnalysisResult
                        {
                            Query = queries[index],
                            PerformanceScore = 0,
                            Issues = new List<PerformanceIssue>
                            {
                                new PerformanceIssue
                                {
                                    IssueType = Constants.IssueType.TableScan,
                                    Severity = Constants.IssueSeverity.Critical,
                                    Description = $"Analysis failed: {ex.Message}"
                                }
                            }
                        };
                    }
                });
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Batch analysis was cancelled");
            throw;
        }

        var finalResults = results.Where(r => r != null).ToList();
        _logger.LogInformation($"Batch analysis complete. {finalResults.Count}/{queries.Length} successful");

        return finalResults;
    }

    /// <summary>
    /// Analyzes queries from file (one per line).
    /// </summary>
    public async Task<List<QueryAnalysisResult>> AnalyzeBatchFromFileAsync(
        string filePath,
        Action<BatchProgress>? onProgress = null,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Query file not found: {filePath}");
        }

        var queries = await File.ReadAllLinesAsync(filePath, cancellationToken);
        var validQueries = queries.Where(q => !string.IsNullOrWhiteSpace(q)).ToArray();

        _logger.LogInformation($"Loaded {validQueries.Length} queries from {filePath}");

        return await AnalyzeBatchAsync(validQueries, onProgress, cancellationToken);
    }

    /// <summary>
    /// Sets maximum parallelism level.
    /// </summary>
    public void SetMaxParallel(int maxParallel)
    {
        _maxParallel = Math.Max(1, Math.Min(maxParallel, Environment.ProcessorCount * 2));
        _logger.LogDebug($"Max parallel threads set to {_maxParallel}");
    }

    /// <summary>
    /// Analyzes a file split by delimiter (e.g., batches of queries).
    /// </summary>
    public async Task<List<QueryAnalysisResult>> AnalyzeBatchFromDelimitedFileAsync(
        string filePath,
        string delimiter = "GO", // SQL Server batch delimiter
        Action<BatchProgress>? onProgress = null,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Query file not found: {filePath}");
        }

        var content = await File.ReadAllTextAsync(filePath, cancellationToken);
        var queries = content
            .Split(new[] { delimiter }, StringSplitOptions.RemoveEmptyEntries)
            .Select(q => q.Trim())
            .Where(q => !string.IsNullOrWhiteSpace(q))
            .ToArray();

        _logger.LogInformation($"Extracted {queries.Length} queries from {filePath} (delimiter: {delimiter})");

        return await AnalyzeBatchAsync(queries, onProgress, cancellationToken);
    }
}

/// <summary>
/// Reports progress of batch analysis.
/// </summary>
public class BatchProgress
{
    public int ProcessedCount { get; set; }
    public int TotalCount { get; set; }
    public int CurrentQueryIndex { get; set; }
    public double PercentComplete { get; set; }

    public override string ToString() =>
        $"Progress: {ProcessedCount}/{TotalCount} ({PercentComplete:F1}%)";
}
