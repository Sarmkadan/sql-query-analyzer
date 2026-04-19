#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;
using SqlQueryAnalyzer.Models;
using SqlQueryAnalyzer.Services;

namespace SqlQueryAnalyzer.API;

/// <summary>
/// API controller for query analysis endpoints.
/// Handles HTTP requests for analyzing queries and retrieving results.
/// Can be used with ASP.NET Core or similar web frameworks.
/// </summary>
public sealed class AnalysisController
{
    private readonly IQueryAnalyzerService _analyzerService;
    private readonly ILogger<AnalysisController> _logger;

    public AnalysisController(
        IQueryAnalyzerService analyzerService,
        ILogger<AnalysisController> logger)
    {
        _analyzerService = analyzerService;
        _logger = logger;
    }

    /// <summary>
    /// Analyzes a single SQL query.
    /// POST /api/analyze
    /// </summary>
    public async Task<ApiResponse<QueryAnalysisResult>> AnalyzeAsync(AnalysisRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Query))
            {
                return new ApiResponse<QueryAnalysisResult>
                {
                    Success = false,
                    Message = "Query cannot be empty",
                    StatusCode = 400
                };
            }

            _logger.LogInformation("Analyzing query via API");
            var result = await _analyzerService.AnalyzeQueryAsync(request.Query);

            return new ApiResponse<QueryAnalysisResult>
            {
                Success = true,
                Data = result,
                Message = "Analysis completed successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Analysis failed");
            return new ApiResponse<QueryAnalysisResult>
            {
                Success = false,
                Message = $"Analysis failed: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    /// <summary>
    /// Analyzes multiple queries in batch.
    /// POST /api/analyze/batch
    /// </summary>
    public async Task<ApiResponse<List<QueryAnalysisResult>>> AnalyzeBatchAsync(BatchAnalysisRequest request)
    {
        try
        {
            if (request.Queries == null || request.Queries.Length == 0)
            {
                return new ApiResponse<List<QueryAnalysisResult>>
                {
                    Success = false,
                    Message = "At least one query is required",
                    StatusCode = 400
                };
            }

            _logger.LogInformation($"Batch analyzing {request.Queries.Length} queries");
            var results = new List<QueryAnalysisResult>();

            foreach (var query in request.Queries)
            {
                var result = await _analyzerService.AnalyzeQueryAsync(query);
                results.Add(result);
            }

            return new ApiResponse<List<QueryAnalysisResult>>
            {
                Success = true,
                Data = results,
                Message = $"Analyzed {results.Count} queries successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Batch analysis failed");
            return new ApiResponse<List<QueryAnalysisResult>>
            {
                Success = false,
                Message = $"Batch analysis failed: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    /// <summary>
    /// Gets health status of the analyzer service.
    /// GET /api/health
    /// </summary>
    public Task<ApiResponse<HealthStatus>> GetHealthAsync()
    {
        try
        {
            var status = new HealthStatus
            {
                IsHealthy = true,
                Message = "Analyzer service is operational",
                Timestamp = DateTime.UtcNow,
                Version = "1.0.0"
            };

            return Task.FromResult(new ApiResponse<HealthStatus>
            {
                Success = true,
                Data = status,
                StatusCode = 200
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return Task.FromResult(new ApiResponse<HealthStatus>
            {
                Success = false,
                Message = $"Health check failed: {ex.Message}",
                StatusCode = 503
            });
        }
    }
}

/// <summary>
/// Request to analyze a single query.
/// </summary>
public class AnalysisRequest
{
    public string Query { get; set; } = string.Empty;
    public Dictionary<string, string>? Options { get; set; }
}

/// <summary>
/// Request to analyze multiple queries.
/// </summary>
public class BatchAnalysisRequest
{
    public string[] Queries { get; set; } = Array.Empty<string>();
    public int? MaxDegreeOfParallelism { get; set; }
}

/// <summary>
/// Generic API response wrapper.
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string Message { get; set; } = string.Empty;
    public int StatusCode { get; set; } = 200;
    public List<string> Errors { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public override string ToString() =>
        $"API Response - Status: {StatusCode}, Success: {Success}, Message: {Message}";
}

/// <summary>
/// Health status response.
/// </summary>
public class HealthStatus
{
    public bool IsHealthy { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object>? Details { get; set; }
}
