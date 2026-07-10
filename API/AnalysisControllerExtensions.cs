#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;
using SqlQueryAnalyzer.Models;

namespace SqlQueryAnalyzer.API;

/// <summary>
/// Extension methods for AnalysisController to provide additional functionality
/// and convenience methods for query analysis operations.
/// </summary>
public static class AnalysisControllerExtensions
{
    /// <summary>
    /// Creates a default analysis request with common options pre-configured.
    /// </summary>
    /// <param name="controller">The AnalysisController instance</param>
    /// <param name="query">The SQL query to analyze</param>
    /// <returns>Configured AnalysisRequest with default options</returns>
    public static AnalysisRequest CreateAnalysisRequest(this AnalysisController controller, string query)
    {
        return new AnalysisRequest
        {
            Query = query ?? string.Empty,
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
    /// <param name="controller">The AnalysisController instance</param>
    /// <param name="queries">Array of SQL queries to analyze</param>
    /// <returns>Configured BatchAnalysisRequest</returns>
    public static BatchAnalysisRequest CreateBatchRequest(this AnalysisController controller, params string[] queries)
    {
        return new BatchAnalysisRequest
        {
            Queries = queries ?? Array.Empty<string>(),
            MaxDegreeOfParallelism = Environment.ProcessorCount
        };
    }

    /// <summary>
    /// Analyzes a query and returns the result with simplified error handling.
    /// </summary>
    /// <param name="controller">The AnalysisController instance</param>
    /// <param name="query">The SQL query to analyze</param>
    /// <returns>QueryAnalysisResult or null if analysis fails</returns>
    public static async Task<QueryAnalysisResult?> AnalyzeQuerySafelyAsync(this AnalysisController controller, string query)
    {
        var request = controller.CreateAnalysisRequest(query);
        var response = await controller.AnalyzeAsync(request);

        if (response.Success && response.Data != null)
        {
            return response.Data;
        }

        return null;
    }

    /// <summary>
    /// Analyzes multiple queries and returns results with error aggregation.
    /// </summary>
    /// <param name="controller">The AnalysisController instance</param>
    /// <param name="queries">Array of SQL queries to analyze</param>
    /// <returns>Tuple containing success status, results list, and error messages</returns>
    public static async Task<(bool Success, List<QueryAnalysisResult> Results, List<string> Errors)>
        AnalyzeQueriesWithErrorsAsync(this AnalysisController controller, params string[] queries)
    {
        var request = controller.CreateBatchRequest(queries);
        var response = await controller.AnalyzeBatchAsync(request);

        var results = new List<QueryAnalysisResult>();
        var errors = new List<string>();

        if (response.Success && response.Data != null)
        {
            results.AddRange(response.Data);
            return (true, results, errors);
        }

        if (response.Errors != null)
        {
            errors.AddRange(response.Errors);
        }

        return (false, results, errors);
    }
}