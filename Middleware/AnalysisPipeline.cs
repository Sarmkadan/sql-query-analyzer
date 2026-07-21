#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SqlQueryAnalyzer.CLI;
using SqlQueryAnalyzer.Caching;
using SqlQueryAnalyzer.Services;
using SqlQueryAnalyzer.Utilities;

namespace SqlQueryAnalyzer.Middleware;

/// <summary>
/// Core pipeline that coordinates the analysis workflow.
/// Manages a chain of middleware components that process queries sequentially.
/// Separates concerns: validation, normalization, analysis, optimization.
/// </summary>
public class AnalysisPipeline
{
    private readonly List<IAnalysisMiddleware> _middlewares = new();
    private readonly ILogger<AnalysisPipeline> _logger;
    private readonly IQueryAnalyzerService _analyzer;

    public AnalysisPipeline(
        ILogger<AnalysisPipeline> logger,
        IQueryAnalyzerService analyzer)
    {
        _logger = logger;
        _analyzer = analyzer;


        // Register middleware in order of execution
        RegisterMiddleware(new LoggingMiddleware(NullLogger<LoggingMiddleware>.Instance));
        RegisterMiddleware(new ValidationMiddleware(NullLogger<ValidationMiddleware>.Instance));
        RegisterMiddleware(new QueryNormalizationMiddleware(NullLogger<QueryNormalizationMiddleware>.Instance));
        RegisterMiddleware(new CachingMiddleware(QueryAnalysisCache.Instance, NullLogger<CachingMiddleware>.Instance));
        RegisterMiddleware(new AnalysisMiddleware(analyzer, NullLogger<AnalysisMiddleware>.Instance));
        RegisterMiddleware(new OptimizationMiddleware(NullLogger<OptimizationMiddleware>.Instance));
    }

    /// <summary>
    /// Registers a middleware component into the pipeline.
    /// Middlewares execute in registration order.
    /// </summary>
    public void RegisterMiddleware(IAnalysisMiddleware middleware)
    {
        _middlewares.Add(middleware);
    }

    /// <summary>
    /// Executes the complete analysis pipeline for a given context.
    /// Each middleware has opportunity to process or modify the context.
    /// </summary>
    public async Task ExecuteAsync(AnalysisContext context)
    {
        _logger.LogInformation("Starting analysis pipeline");

        try
        {
            foreach (var middleware in _middlewares)
            {
                if (!context.ShouldContinue)
                {
                    _logger.LogWarning("Pipeline execution halted by middleware");
                    break;
                }

                _logger.LogDebug($"Executing middleware: {middleware.GetType().Name}");
                await middleware.ExecuteAsync(context);
            }

            _logger.LogInformation("Analysis pipeline completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Pipeline execution failed");
            throw;
        }
    }

    /// <summary>
    /// Clears all registered middlewares and resets pipeline to initial state.
    /// Useful for testing or dynamic reconfiguration.
    /// </summary>
    public void Clear() => _middlewares.Clear();

    /// <summary>
    /// Returns count of registered middlewares for diagnostic purposes.
    /// </summary>
    public int MiddlewareCount => _middlewares.Count;
}

/// <summary>
/// Interface for middleware components in the analysis pipeline.
/// Each middleware handles a specific aspect of query analysis.
/// </summary>
public interface IAnalysisMiddleware
{
    /// <summary>
    /// Executes the middleware logic on the provided context.
    /// May modify context or set Result property.
    /// Must set ShouldContinue to false to halt pipeline.
    /// </summary>
    Task ExecuteAsync(AnalysisContext context);
}

/// <summary>
/// Middleware that logs query and context information at each pipeline stage.
/// Helps with debugging and performance monitoring.
/// </summary>
public class LoggingMiddleware : IAnalysisMiddleware
{
    private readonly ILogger<LoggingMiddleware> _logger;

    public LoggingMiddleware(ILogger<LoggingMiddleware> logger)
    {
        _logger = logger;
    }

    public Task ExecuteAsync(AnalysisContext context)
    {
        var queryPreview = context.Query.Length > 80
            ? context.Query[..80] + "..."
            : context.Query;

        _logger.LogInformation($"Processing query: {queryPreview}");
        _logger.LogDebug($"Arguments - Verbose: {context.Arguments.Verbose}, Format: {context.Arguments.OutputFormat}");

        return Task.CompletedTask;
    }
}

/// <summary>
/// Middleware that validates query syntax and arguments before analysis.
/// Prevents invalid queries from consuming analyzer resources.
/// </summary>
public class ValidationMiddleware : IAnalysisMiddleware
{
    private readonly ILogger<ValidationMiddleware> _logger;

    public ValidationMiddleware(ILogger<ValidationMiddleware> logger)
    {
        _logger = logger;
    }

    public Task ExecuteAsync(AnalysisContext context)
    {
        try
        {
            var isValid = QueryValidator.IsValidQuery(context.Query);
            if (!isValid)
            {
                _logger.LogWarning("Query validation failed");
                context.ShouldContinue = false;
                throw new InvalidOperationException("Query failed validation checks");
            }

            _logger.LogDebug("Query validation passed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Validation middleware error");
            throw;
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Middleware that normalizes query syntax without changing logic.
/// Removes unnecessary whitespace, standardizes capitalization, etc.
/// Improves analysis consistency across different input formats.
/// </summary>
public class QueryNormalizationMiddleware : IAnalysisMiddleware
{
    private readonly ILogger<QueryNormalizationMiddleware> _logger;

    public QueryNormalizationMiddleware(ILogger<QueryNormalizationMiddleware> logger)
    {
        _logger = logger;
    }

    public Task ExecuteAsync(AnalysisContext context)
    {
        try
        {
            var normalizer = new QueryNormalizer();
            context.Query = normalizer.Normalize(context.Query);
            _logger.LogDebug("Query normalization completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Normalization middleware error");
            throw;
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Middleware that performs the actual analysis using IQueryAnalyzerService.
/// Populates context.Result with analysis findings.
/// </summary>
public class AnalysisMiddleware : IAnalysisMiddleware
{
    private readonly IQueryAnalyzerService _analyzer;
    private readonly ILogger<AnalysisMiddleware> _logger;

    public AnalysisMiddleware(IQueryAnalyzerService analyzer, ILogger<AnalysisMiddleware> logger)
    {
        _analyzer = analyzer;
        _logger = logger;
    }

    public async Task ExecuteAsync(AnalysisContext context)
    {
        try
        {
            _logger.LogInformation("Starting query analysis");
            context.Result = await _analyzer.AnalyzeQueryAsync(context.Query);
            _logger.LogInformation($"Analysis complete. Score: {context.Result.PerformanceScore:F1}/100");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Analysis middleware error");
            throw;
        }
    }
}

/// <summary>
/// Middleware that applies post-analysis optimizations.
/// Filters results, applies severity filters, limits output size.
/// </summary>
public class OptimizationMiddleware : IAnalysisMiddleware
{
    private readonly ILogger<OptimizationMiddleware> _logger;

    public OptimizationMiddleware(ILogger<OptimizationMiddleware> logger)
    {
        _logger = logger;
    }

    public Task ExecuteAsync(AnalysisContext context)
    {
        if (context.Result == null)
            return Task.CompletedTask;

        try
        {
            // Store result in cache after analysis
            QueryAnalysisCache.Instance.Set(context.Query, context.Result);

            // Apply severity filter if specified
            if (!string.IsNullOrEmpty(context.Arguments.FilterBySeverity))
            {
                ApplySeverityFilter(context);
            }

            // Apply result limit if specified
            if (context.Arguments.MaxResults.HasValue)
            {
                ApplyResultLimit(context);
            }

            _logger.LogDebug($"Optimization complete. Final issue count: {context.Result.Issues.Count}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Optimization middleware error");
            throw;
        }

        return Task.CompletedTask;
    }

    private void ApplySeverityFilter(AnalysisContext context)
    {
        if (!Enum.TryParse<Constants.IssueSeverity>(context.Arguments.FilterBySeverity, out var severity))
            return;

        var initialCount = context.Result!.Issues.Count;
        context.Result.Issues = context.Result.Issues
            .Where(i => i.Severity == severity)
            .ToList();

        _logger.LogDebug($"Severity filter applied: {initialCount} → {context.Result.Issues.Count} issues");
    }

    private void ApplyResultLimit(AnalysisContext context)
    {
        if (context.Arguments.MaxResults.HasValue && context.Result!.Issues.Count > context.Arguments.MaxResults)
        {
            var initialCount = context.Result.Issues.Count;
            context.Result.Issues = context.Result.Issues
                .Take(context.Arguments.MaxResults.Value)
                .ToList();

            _logger.LogDebug($"Result limit applied: {initialCount} → {context.Result.Issues.Count} issues");
        }
    }
}
