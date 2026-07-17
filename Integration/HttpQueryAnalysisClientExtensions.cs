#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using SqlQueryAnalyzer.Models;
using SqlQueryAnalyzer.Constants;

namespace SqlQueryAnalyzer.Integration;

/// <summary>
/// Extension methods for <see cref="HttpQueryAnalysisClient"/> that provide
/// additional functionality for working with query analysis results and batch operations.
/// </summary>
public static class HttpQueryAnalysisClientExtensions
{
    /// <summary>
    /// Analyzes multiple queries with a specified degree of parallelism.
    /// </summary>
    /// <param name="client">The HTTP query analysis client instance.</param>
    /// <param name="queries">The queries to analyze.</param>
    /// <param name="maxDegreeOfParallelism">Maximum degree of parallelism for analysis.</param>
    /// <returns>Read-only list of analysis results in the same order as input queries.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="client"/> or <paramref name="queries"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="queries"/> is empty.</exception>
    public static async Task<IReadOnlyList<QueryAnalysisResult>> AnalyzeQueriesAsync(
        this HttpQueryAnalysisClient client,
        string[] queries,
        int? maxDegreeOfParallelism = null)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(queries);

        if (queries.Length == 0)
        {
            throw new ArgumentException("Queries collection cannot be empty.", nameof(queries));
        }

        var results = await client.AnalyzeBatchAsync(queries);

        // Ensure results are in the same order as input queries
        if (results.Count != queries.Length)
        {
            throw new InvalidOperationException(
                $"Batch analysis returned {results.Count} results but expected {queries.Length}. Query order cannot be guaranteed.");
        }

        return results.AsReadOnly();
    }

    /// <summary>
    /// Analyzes a single query with optional analysis options.
    /// </summary>
    /// <param name="client">The HTTP query analysis client instance.</param>
    /// <param name="query">The SQL query to analyze.</param>
    /// <param name="options">Optional analysis options (e.g., timeout, rules to apply).</param>
    /// <returns>The analysis result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="client"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="query"/> is empty or whitespace.</exception>
    public static async Task<QueryAnalysisResult> AnalyzeQueryAsync(
        this HttpQueryAnalysisClient client,
        string query,
        Dictionary<string, string>? options = null)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentException.ThrowIfNullOrWhiteSpace(query, nameof(query));

        return await client.AnalyzeQueryAsync(query, options: options);
    }

    /// <summary>
    /// Checks if the remote analyzer service is healthy with retry logic.
    /// </summary>
    /// <param name="client">The HTTP query analysis client instance.</param>
    /// <param name="maxRetries">Maximum number of retry attempts.</param>
    /// <param name="delayMs">Initial delay in milliseconds between retries.</param>
    /// <returns>True if service is healthy; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="client"/> is null.</exception>
    public static async Task<bool> IsHealthyWithRetryAsync(
        this HttpQueryAnalysisClient client,
        int maxRetries = 3,
        int delayMs = 1000)
    {
        ArgumentNullException.ThrowIfNull(client);

        var attempt = 0;

        while (attempt < maxRetries)
        {
            var isHealthy = await client.IsHealthyAsync();

            if (isHealthy)
            {
                return true;
            }

            attempt++;

            if (attempt < maxRetries)
            {
                await Task.Delay(delayMs);
            }
        }

        return false;
    }

    /// <summary>
    /// Gets the version information from the remote analyzer with fallback.
    /// </summary>
    /// <param name="client">The HTTP query analysis client instance.</param>
    /// <param name="fallbackVersion">Version string to return if remote call fails.</param>
    /// <returns>The version string or the fallback version.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="client"/> is null.</exception>
    public static async Task<string> GetVersionAsync(
        this HttpQueryAnalysisClient client,
        string fallbackVersion = "1.0.0")
    {
        ArgumentNullException.ThrowIfNull(client);

        var version = await client.GetVersionAsync();
        return version ?? fallbackVersion;
    }

    /// <summary>
    /// Analyzes queries with timeout and returns results or throws if timeout is exceeded.
    /// </summary>
    /// <param name="client">The HTTP query analysis client instance.</param>
    /// <param name="queries">The queries to analyze.</param>
    /// <param name="timeout">Timeout for the analysis operation.</param>
    /// <param name="maxDegreeOfParallelism">Maximum degree of parallelism.</param>
    /// <returns>Read-only list of analysis results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="client"/> or <paramref name="queries"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="queries"/> is empty.</exception>
    /// <exception cref="TimeoutException">Thrown when analysis exceeds the specified timeout.</exception>
    public static async Task<IReadOnlyList<QueryAnalysisResult>> AnalyzeWithTimeoutAsync(
        this HttpQueryAnalysisClient client,
        string[] queries,
        TimeSpan timeout,
        int? maxDegreeOfParallelism = null)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(queries);

        if (queries.Length == 0)
        {
            throw new ArgumentException("Queries collection cannot be empty.", nameof(queries));
        }

        using var cts = new CancellationTokenSource(timeout);

        try
        {
            return await client.AnalyzeQueriesAsync(queries, maxDegreeOfParallelism)
                .WaitAsync(cts.Token)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw new TimeoutException(
                $"Query analysis timed out after {timeout.TotalSeconds:F1} seconds.");
        }
    }

    /// <summary>
    /// Analyzes queries with timeout specified as milliseconds.
    /// </summary>
    /// <param name="client">The HTTP query analysis client instance.</param>
    /// <param name="queries">The queries to analyze.</param>
    /// <param name="timeoutMs">Timeout in milliseconds for the analysis operation.</param>
    /// <param name="maxDegreeOfParallelism">Maximum degree of parallelism.</param>
    /// <returns>Read-only list of analysis results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="client"/> or <paramref name="queries"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="queries"/> is empty.</exception>
    /// <exception cref="TimeoutException">Thrown when analysis exceeds the specified timeout.</exception>
    public static async Task<IReadOnlyList<QueryAnalysisResult>> AnalyzeWithTimeoutAsync(
        this HttpQueryAnalysisClient client,
        string[] queries,
        int timeoutMs,
        int? maxDegreeOfParallelism = null)
    {
        return await client.AnalyzeWithTimeoutAsync(
            queries,
            TimeSpan.FromMilliseconds(timeoutMs),
            maxDegreeOfParallelism);
    }

    /// <summary>
    /// Gets performance metrics for a collection of queries by analyzing them multiple times
    /// and calculating average performance scores.
    /// </summary>
    /// <param name="client">The HTTP query analysis client instance.</param>
    /// <param name="queries">The queries to analyze.</param>
    /// <param name="iterations">Number of analysis iterations to perform.</param>
    /// <param name="maxDegreeOfParallelism">Maximum degree of parallelism.</param>
    /// <returns>Dictionary mapping queries to their average performance metrics.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="client"/> or <paramref name="queries"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="queries"/> is empty or <paramref name="iterations"/> is less than 1.</exception>
    public static async Task<Dictionary<string, double>> GetPerformanceMetricsAsync(
        this HttpQueryAnalysisClient client,
        string[] queries,
        int iterations = 3,
        int? maxDegreeOfParallelism = null)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(queries);

        if (queries.Length == 0)
        {
            throw new ArgumentException("Queries collection cannot be empty.", nameof(queries));
        }

        if (iterations < 1)
        {
            throw new ArgumentException("Iterations must be at least 1.", nameof(iterations));
        }

        var metrics = new Dictionary<string, double>(queries.Length);
        var totalScores = new double[queries.Length];

        for (var i = 0; i < iterations; i++)
        {
            var results = await client.AnalyzeQueriesAsync(queries, maxDegreeOfParallelism);

            for (var j = 0; j < results.Count; j++)
            {
                totalScores[j] += results[j].PerformanceScore;
            }
        }

        for (var i = 0; i < queries.Length; i++)
        {
            metrics[queries[i]] = totalScores[i] / iterations;
        }

        return metrics;
    }

    /// <summary>
    /// Filters queries by their complexity level after analysis.
    /// </summary>
    /// <param name="client">The HTTP query analysis client instance.</param>
    /// <param name="queries">The queries to analyze.</param>
    /// <param name="minComplexity">Minimum complexity level to include.</param>
    /// <param name="maxComplexity">Maximum complexity level to include.</param>
    /// <param name="maxDegreeOfParallelism">Maximum degree of parallelism.</param>
    /// <returns>Read-only list of queries that match the complexity filter.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="client"/> or <paramref name="queries"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="queries"/> is empty.</exception>
    public static async Task<IReadOnlyList<string>> FilterQueriesByComplexityAsync(
        this HttpQueryAnalysisClient client,
        string[] queries,
        QueryComplexity minComplexity = QueryComplexity.Low,
        QueryComplexity maxComplexity = QueryComplexity.High,
        int? maxDegreeOfParallelism = null)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(queries);

        if (queries.Length == 0)
        {
            throw new ArgumentException("Queries collection cannot be empty.", nameof(queries));
        }

        var results = await client.AnalyzeQueriesAsync(queries, maxDegreeOfParallelism);

        var filteredQueries = new List<string>();

        for (var i = 0; i < results.Count; i++)
        {
            var result = results[i];

            if (result.Complexity >= minComplexity && result.Complexity <= maxComplexity)
            {
                filteredQueries.Add(queries[i]);
            }
        }

        return filteredQueries.AsReadOnly();
    }
}