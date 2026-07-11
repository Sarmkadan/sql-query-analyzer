#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using Microsoft.Extensions.Logging;
using SqlQueryAnalyzer.Models;

namespace SqlQueryAnalyzer.API;

/// <summary>
/// Extension methods for <see cref="AnalysisController"/> to provide additional functionality
/// and convenience methods for query analysis operations.
/// </summary>
public static class AnalysisControllerExtensions
{
    /// <summary>
    /// Creates a default analysis request with common options pre-configured.
    /// </summary>
    /// <param name="controller">The <see cref="AnalysisController"/> instance.</param>
    /// <param name="query">The SQL query to analyze. Cannot be null or whitespace.</param>
    /// <returns>Configured <see cref="AnalysisRequest"/> with default options.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="controller"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="query"/> is null or whitespace.</exception>
    public static AnalysisRequest CreateAnalysisRequest(this AnalysisController controller, string query)
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentException.ThrowIfNullOrWhiteSpace(query);

        return new AnalysisRequest
        {
            Query = query,
            Options = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "include_plan", "true" },
                { "include_stats", "true" },
                { "timeout_seconds", "30" }
            }
        };
    }

    /// <summary>
    /// Creates a batch analysis request with the specified queries.
    /// </summary>
    /// <param name="controller">The <see cref="AnalysisController"/> instance.</param>
    /// <param name="queries">Array of SQL queries to analyze. Can be empty but not null.</param>
    /// <returns>Configured <see cref="BatchAnalysisRequest"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="controller"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="queries"/> is <see langword="null"/>.</exception>
    public static BatchAnalysisRequest CreateBatchRequest(this AnalysisController controller, params string[] queries)
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentNullException.ThrowIfNull(queries);

        return new BatchAnalysisRequest
        {
            Queries = queries,
            MaxDegreeOfParallelism = Environment.ProcessorCount
        };
    }

    /// <summary>
    /// Analyzes a query and returns the result with simplified error handling.
    /// Returns null if analysis fails or returns unsuccessful response.
    /// </summary>
    /// <param name="controller">The <see cref="AnalysisController"/> instance.</param>
    /// <param name="query">The SQL query to analyze.</param>
    /// <returns><see cref="QueryAnalysisResult"/> if successful, otherwise <see langword="null"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="controller"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="query"/> is null or whitespace.</exception>
    public static async Task<QueryAnalysisResult?> AnalyzeQuerySafelyAsync(this AnalysisController controller, string query)
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentException.ThrowIfNullOrWhiteSpace(query);

        var request = controller.CreateAnalysisRequest(query);
        var response = await controller.AnalyzeAsync(request);

        return response.Success && response.Data is not null
            ? response.Data
            : null;
    }

    /// <summary>
    /// Analyzes multiple queries and returns results with error aggregation.
    /// </summary>
    /// <param name="controller">The <see cref="AnalysisController"/> instance.</param>
    /// <param name="queries">Array of SQL queries to analyze.</param>
    /// <returns>Tuple containing success status, results list, and error messages.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="controller"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="queries"/> is <see langword="null"/>.</exception>
    public static async Task<(bool Success, List<QueryAnalysisResult> Results, List<string> Errors)>
        AnalyzeQueriesWithErrorsAsync(this AnalysisController controller, params string[] queries)
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentNullException.ThrowIfNull(queries);

        var request = controller.CreateBatchRequest(queries);
        var response = await controller.AnalyzeBatchAsync(request);

        var results = new List<QueryAnalysisResult>();
        var errors = new List<string>();

        if (response.Success && response.Data is not null)
        {
            results.AddRange(response.Data);
            return (true, results, errors);
        }

        if (response.Errors is not null)
        {
            errors.AddRange(response.Errors);
        }

        return (false, results, errors);
    }
}