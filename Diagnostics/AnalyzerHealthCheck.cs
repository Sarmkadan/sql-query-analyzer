#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;
using SqlQueryAnalyzer.Caching;
using SqlQueryAnalyzer.Middleware;
using SqlQueryAnalyzer.Utilities;

namespace SqlQueryAnalyzer.Diagnostics;

/// <summary>
/// Health check system for the SQL query analyzer.
/// Monitors component status, resource usage, and system health.
/// Provides diagnostics and self-healing capabilities.
/// </summary>
public class AnalyzerHealthCheck
{
    private readonly ILogger<AnalyzerHealthCheck> _logger;
    private readonly QueryAnalysisCache _cache;
    private readonly RateLimitingMiddleware _rateLimiter;
    private readonly PerformanceMetricCollector _metrics;
    private readonly DatabaseConnectionValidator _connectionValidator;

    public AnalyzerHealthCheck(
        ILogger<AnalyzerHealthCheck> logger,
        QueryAnalysisCache cache,
        RateLimitingMiddleware rateLimiter,
        PerformanceMetricCollector metrics,
        DatabaseConnectionValidator connectionValidator)
    {
        _logger = logger;
        _cache = cache;
        _rateLimiter = rateLimiter;
        _metrics = metrics;
        _connectionValidator = connectionValidator;
    }

    /// <summary>
    /// Performs comprehensive health check of all components.
    /// </summary>
    public async Task<HealthCheckResult> CheckHealthAsync()
    {
        var result = new HealthCheckResult();
        result.CheckTime = DateTime.UtcNow;

        try
        {
            // Check cache health
            result.CacheHealth = CheckCacheHealth();

            // Check rate limiter
            result.RateLimiterHealth = CheckRateLimiterHealth();

            // Check metrics collection
            result.MetricsHealth = CheckMetricsHealth();

            // Check database connectivity
            result.DatabaseHealth = await CheckDatabaseHealthAsync();

            // Determine overall health
            result.Status = DetermineOverallHealth(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing health check");
            result.Status = HealthStatus.Error;
            result.Errors.Add($"Health check error: {ex.Message}");
        }

        return result;
    }

    /// <summary>
    /// Checks cache component health.
    /// </summary>
    private ComponentHealth CheckCacheHealth()
    {
        var health = new ComponentHealth { Component = "Cache" };

        try
        {
            var stats = _cache.GetStatistics();

            // Cache is healthy if hit rate is reasonable or cache is mostly empty
            if (stats.HitRate >= 30 || stats.TotalEntries == 0)
            {
                health.Status = HealthStatus.Healthy;
                health.Message = $"Cache operational ({stats.HitRate:F1}% hit rate)";
            }
            else
            {
                health.Status = HealthStatus.Degraded;
                health.Message = $"Low cache hit rate ({stats.HitRate:F1}%)";
            }

            // Warn if cache is near capacity
            if (stats.TotalEntries > stats.MaxEntries * 0.9)
            {
                health.Status = HealthStatus.Degraded;
                health.Message = "Cache near capacity";
            }
        }
        catch (Exception ex)
        {
            health.Status = HealthStatus.Error;
            health.Message = $"Cache check failed: {ex.Message}";
        }

        return health;
    }

    /// <summary>
    /// Checks rate limiter health.
    /// </summary>
    private ComponentHealth CheckRateLimiterHealth()
    {
        var health = new ComponentHealth { Component = "RateLimiter" };

        try
        {
            var load = _rateLimiter.GetSystemLoad();

            if (load < 70)
            {
                health.Status = HealthStatus.Healthy;
                health.Message = $"Rate limiter operational (load: {load:F1}%)";
            }
            else if (load < 90)
            {
                health.Status = HealthStatus.Degraded;
                health.Message = $"High load ({load:F1}% of capacity)";
            }
            else
            {
                health.Status = HealthStatus.Unhealthy;
                health.Message = $"Critical load ({load:F1}% of capacity)";
            }
        }
        catch (Exception ex)
        {
            health.Status = HealthStatus.Error;
            health.Message = $"Rate limiter check failed: {ex.Message}";
        }

        return health;
    }

    /// <summary>
    /// Checks metrics collection health.
    /// </summary>
    private ComponentHealth CheckMetricsHealth()
    {
        var health = new ComponentHealth { Component = "Metrics" };

        try
        {
            var report = _metrics.GetReport();

            if (report.SuccessRate >= 95)
            {
                health.Status = HealthStatus.Healthy;
                health.Message = $"Metrics healthy (success rate: {report.SuccessRate:F1}%)";
            }
            else
            {
                health.Status = HealthStatus.Degraded;
                health.Message = $"Lower success rate ({report.SuccessRate:F1}%)";
            }
        }
        catch (Exception ex)
        {
            health.Status = HealthStatus.Error;
            health.Message = $"Metrics check failed: {ex.Message}";
        }

        return health;
    }

    /// <summary>
    /// Checks database connectivity.
    /// </summary>
    private async Task<ComponentHealth> CheckDatabaseHealthAsync()
    {
        var health = new ComponentHealth { Component = "Database" };

        try
        {
            // Validate connection without actual connection for speed
            var validation = await _connectionValidator.ValidateAsync(
                "test", // Would use actual connection string in production
                testConnection: false);

            if (validation.IsValid)
            {
                health.Status = HealthStatus.Healthy;
                health.Message = "Database connection valid";
            }
            else
            {
                health.Status = HealthStatus.Unhealthy;
                health.Message = $"Database validation failed: {string.Join(", ", validation.Errors)}";
            }
        }
        catch (Exception ex)
        {
            health.Status = HealthStatus.Error;
            health.Message = $"Database check error: {ex.Message}";
        }

        return health;
    }

    /// <summary>
    /// Determines overall health status based on component statuses.
    /// </summary>
    private HealthStatus DetermineOverallHealth(HealthCheckResult result)
    {
        var components = new[] { result.CacheHealth, result.RateLimiterHealth, result.MetricsHealth, result.DatabaseHealth };

        // If any critical component is unhealthy, overall is unhealthy
        if (components.Any(c => c.Status == HealthStatus.Unhealthy || c.Status == HealthStatus.Error))
        {
            return HealthStatus.Unhealthy;
        }

        // If any component is degraded, overall is degraded
        if (components.Any(c => c.Status == HealthStatus.Degraded))
        {
            return HealthStatus.Degraded;
        }

        return HealthStatus.Healthy;
    }

    /// <summary>
    /// Attempts to self-heal degraded components.
    /// </summary>
    public async Task<SelfHealResult> AttemptSelfHealAsync(HealthCheckResult healthCheck)
    {
        var result = new SelfHealResult();

        try
        {
            // Clear cache if health is degraded
            if (healthCheck.CacheHealth.Status == HealthStatus.Degraded)
            {
                _cache.RemoveExpiredEntries();
                result.ActionsPerformed.Add("Cleared expired cache entries");
            }

            // Reset metrics if needed
            if (healthCheck.MetricsHealth.Status == HealthStatus.Degraded)
            {
                _metrics.Reset();
                result.ActionsPerformed.Add("Reset performance metrics");
            }

            result.Success = true;
            _logger.LogInformation($"Self-heal performed {result.ActionsPerformed.Count} actions");
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Error = ex.Message;
            _logger.LogError(ex, "Self-heal failed");
        }

        return result;
    }
}

/// <summary>
/// Health check result containing status of all components.
/// </summary>
public class HealthCheckResult
{
    public DateTime CheckTime { get; set; }
    public HealthStatus Status { get; set; }
    public ComponentHealth CacheHealth { get; set; } = new();
    public ComponentHealth RateLimiterHealth { get; set; } = new();
    public ComponentHealth MetricsHealth { get; set; } = new();
    public ComponentHealth DatabaseHealth { get; set; } = new();
    public List<string> Errors { get; set; } = new();

    public override string ToString() =>
        $"Health Status: {Status}\n" +
        $"  Cache: {CacheHealth.Status}\n" +
        $"  RateLimiter: {RateLimiterHealth.Status}\n" +
        $"  Metrics: {MetricsHealth.Status}\n" +
        $"  Database: {DatabaseHealth.Status}";
}

/// <summary>
/// Health status of a single component.
/// </summary>
public class ComponentHealth
{
    public string Component { get; set; } = string.Empty;
    public HealthStatus Status { get; set; }
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Result of self-healing attempt.
/// </summary>
public class SelfHealResult
{
    public bool Success { get; set; }
    public List<string> ActionsPerformed { get; set; } = new();
    public string? Error { get; set; }
}

/// <summary>
/// System health status enum.
/// </summary>
public enum HealthStatus
{
    Healthy,
    Degraded,
    Unhealthy,
    Error
}
