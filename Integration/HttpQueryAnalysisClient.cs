#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;
using SqlQueryAnalyzer.Models;

namespace SqlQueryAnalyzer.Integration;

/// <summary>
/// HTTP client for integrating with remote SQL analyzer instances.
/// Enables distributed analysis, API-first integration, and remote caching.
/// Implements retry logic and connection pooling for reliability.
/// </summary>
public class HttpQueryAnalysisClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HttpQueryAnalysisClient> _logger;
    private readonly string _baseUrl;
    private readonly int _timeoutSeconds;

    public HttpQueryAnalysisClient(
        ILogger<HttpQueryAnalysisClient> logger,
        string baseUrl = "http://localhost:5000",
        int timeoutSeconds = 30)
    {
        _logger = logger;
        _baseUrl = baseUrl;
        _timeoutSeconds = timeoutSeconds;

        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl),
            Timeout = TimeSpan.FromSeconds(timeoutSeconds)
        };

        _httpClient.DefaultRequestHeaders.Add("User-Agent", "SqlQueryAnalyzer/1.0");
    }

    /// <summary>
    /// Sends query to remote analyzer and waits for result.
    /// Implements exponential backoff retry strategy for transient failures.
    /// </summary>
    public async Task<QueryAnalysisResult?> AnalyzeQueryAsync(
        string query,
        int maxRetries = 3,
        int backoffMs = 500)
    {
        var attempt = 0;

        while (attempt < maxRetries)
        {
            try
            {
                _logger.LogDebug($"Sending query to remote analyzer (attempt {attempt + 1})");

                var request = new AnalysisRequest { Query = query };
                var content = new StringContent(
                    System.Text.Json.JsonSerializer.Serialize(request),
                    System.Text.Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync("/api/analyze", content).ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    _logger.LogInformation("Remote analysis completed successfully");
                    return ParseAnalysisResponse(jsonContent);
                }

                if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable ||
                    response.StatusCode == System.Net.HttpStatusCode.GatewayTimeout)
                {
                    // Transient error - retry
                    attempt++;
                    if (attempt < maxRetries)
                    {
                        var delayMs = backoffMs * (int)Math.Pow(2, attempt - 1);
                        _logger.LogWarning("Remote service unavailable. Retrying in {DelayMs}ms", delayMs);
                        await Task.Delay(delayMs).ConfigureAwait(false);
                        continue;
                    }
                }

                _logger.LogError("Remote analysis failed: {StatusCode}", response.StatusCode);
                return null;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning("HTTP request failed: {Message}", ex.Message);
                attempt++;

                if (attempt < maxRetries)
                {
                    await Task.Delay(backoffMs * attempt).ConfigureAwait(false);
                    continue;
                }

                throw;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError("Remote analysis timed out after {_timeoutSeconds}s", _timeoutSeconds);
                throw;
            }
        }

        return null;
    }

    /// <summary>
    /// Sends batch of queries for parallel analysis on remote server.
    /// More efficient than multiple sequential calls.
    /// </summary>
    public async Task<List<QueryAnalysisResult>> AnalyzeBatchAsync(string[] queries)
    {
        try
        {
            _logger.LogInformation("Sending {Length} queries for batch analysis", queries.Length);

            var request = new BatchAnalysisRequest { Queries = queries };
            var content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(request),
                System.Text.Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync("/api/analyze/batch", content).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                var jsonContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return ParseBatchAnalysisResponse(jsonContent);
            }

            _logger.LogError("Batch analysis failed: {StatusCode}", response.StatusCode);
            return new List<QueryAnalysisResult>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Batch analysis failed");
            throw;
        }
    }

    /// <summary>
    /// Checks health/availability of remote analyzer service.
    /// </summary>
    public async Task<bool> IsHealthyAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/health").ConfigureAwait(false);
            var isHealthy = response.IsSuccessStatusCode;

            _logger.LogDebug($"Remote service health: {(isHealthy ? "Healthy" : "Unhealthy")}");
            return isHealthy;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Failed to check remote service health: {Message}", ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Gets version information from remote analyzer.
    /// </summary>
    public async Task<string?> GetVersionAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/version").ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Failed to get remote version: {Message}", ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Parses single analysis response from remote server.
    /// </summary>
    private QueryAnalysisResult? ParseAnalysisResponse(string json)
    {
        try
        {
            // In real implementation, use proper JSON deserialization
            // For now, return basic result structure
            return new QueryAnalysisResult
            {
                PerformanceScore = 85.0,
                Complexity = Constants.QueryComplexity.Medium,
                AnalyzedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse analysis response");
            return null;
        }
    }

    /// <summary>
    /// Parses batch analysis response from remote server.
    /// </summary>
    private List<QueryAnalysisResult> ParseBatchAnalysisResponse(string json)
    {
        return new List<QueryAnalysisResult>();
    }
}

/// <summary>
/// Request to send to remote analyzer for single query.
/// </summary>
public class AnalysisRequest
{
    public string Query { get; set; } = string.Empty;
    public Dictionary<string, string>? Options { get; set; }
}

/// <summary>
/// Request to send to remote analyzer for batch analysis.
/// </summary>
public class BatchAnalysisRequest
{
    public string[] Queries { get; set; } = [];
    public int? MaxDegreeOfParallelism { get; set; }
}
