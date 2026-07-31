#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SqlQueryAnalyzer.Constants;
using SqlQueryAnalyzer.DTOs;
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
    /// Analyzes multiple queries in batch using AnalysisRequestDto objects.
    /// POST /api/analyze/batch/advanced
    /// Accepts an array of AnalysisRequestDto objects, runs the pipeline per query,
    /// returns per‑query results plus summary counts.
    /// </summary>
    public async Task<ApiResponse<BatchAnalysisResponseDto>> AnalyzeAdvancedBatchAsync(List<AnalysisRequestDto> requests)
    {
        try
        {
            if (requests == null || requests.Count == 0)
            {
                return new ApiResponse<BatchAnalysisResponseDto>
                {
                    Success = false,
                    Message = "At least one request is required",
                    StatusCode = 400
                };
            }

            if (requests.Count > 100)
            {
                return new ApiResponse<BatchAnalysisResponseDto>
                {
                    Success = false,
                    Message = "Batch size cannot exceed 100 queries",
                    StatusCode = 400
                };
            }

            // Validation: ensure each request has a non‑empty query.
            var validationErrors = new List<string>();
            for (int i = 0; i < requests.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(requests[i].QueryText))
                {
                    validationErrors.Add($"Request {i}: Query cannot be empty");
                }
            }

            if (validationErrors.Any())
            {
                return new ApiResponse<BatchAnalysisResponseDto>
                {
                    Success = false,
                    Message = "Validation errors",
                    StatusCode = 400,
                    Errors = validationErrors
                };
            }

            _logger.LogInformation($"Advanced batch analyzing {requests.Count} queries");

            var results = new List<AnalysisResponseDto>();
            var errors = new List<string>();
            var totalAnalysisTimeMs = 0L;
            var totalScore = 0.0;
            var nPlusOnePatterns = new List<string>();

            foreach (var request in requests)
            {
                try
                {
                    var startTime = DateTime.UtcNow;
                    var result = await _analyzerService.AnalyzeQueryAsync(request.QueryText);
                    var analysisTimeMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;

                    totalAnalysisTimeMs += analysisTimeMs;

                    var responseDto = new AnalysisResponseDto
                    {
                        QueryId = result.QueryId,
                        PerformanceScore = result.PerformanceScore,
                        ComplexityLevel = result.Complexity.ToString(),
                        IssueCount = result.Issues.Count,
                        CriticalIssueCount = result.Issues.Count(i => i.Severity == IssueSeverity.Critical),
                        Issues = result.Issues.Select(i => new PerformanceIssueDto
                        {
                            IssueType = i.IssueType.ToString(),
                            Severity = i.Severity.ToString(),
                            Description = i.Description,
                            EstimatedImpact = i.EstimatedPerformanceImpact,
                            RecommendedFix = i.RecommendedFix,
                            Priority = i.Priority
                        }).ToList(),
                        IndexSuggestions = result.IndexSuggestions.Select(s => new IndexSuggestionDto
                        {
                            TableName = s.TableName,
                            IndexName = s.IndexName,
                            Columns = s.IndexColumns,
                            IncludeColumns = s.IncludeColumns,
                            EstimatedGain = s.EstimatedPerformanceGain,
                            CreateScript = s.GeneratedCreateScript,
                            AffectedQueries = s.AffectedQueries
                        }).ToList(),
                        Summary = result.GetSummary(),
                        AnalysisTimeMs = analysisTimeMs
                    };

                    results.Add(responseDto);
                    totalScore += result.PerformanceScore;

                    if (result.Issues.Any(i => i.IssueType == IssueType.NPlusOne))
                    {
                        nPlusOnePatterns.Add(result.QueryId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to analyze query in batch");
                    errors.Add($"Query failed: {ex.Message}");
                }
            }

            var batchResponse = new BatchAnalysisResponseDto
            {
                TotalQueries = requests.Count,
                SuccessfulAnalyses = results.Count,
                FailedAnalyses = errors.Count,
                Results = results,
                NPlusOnePatterns = nPlusOnePatterns,
                AverageScore = results.Count > 0 ? totalScore / results.Count : 0,
                TotalAnalysisTimeMs = totalAnalysisTimeMs
            };

            return new ApiResponse<BatchAnalysisResponseDto>
            {
                Success = errors.Count == 0,
                Data = batchResponse,
                Message = errors.Count == 0
                    ? $"Analyzed {results.Count} queries successfully"
                    : $"Analyzed {results.Count} queries with {errors.Count} failures",
                StatusCode = errors.Count == 0 ? 200 : 207
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Advanced batch analysis failed");
            return new ApiResponse<BatchAnalysisResponseDto>
            {
                Success = false,
                Message = $"Advanced batch analysis failed: {ex.Message}",
                StatusCode = 500
            };
        }
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
    public string[] Queries { get; set; } = [];
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
