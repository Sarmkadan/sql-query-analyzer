// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;
using SqlQueryAnalyzer.Exceptions;

namespace SqlQueryAnalyzer.Middleware;

/// <summary>
/// Centralized error handling and recovery mechanism for the analysis pipeline.
/// Provides graceful degradation, retry logic, and detailed error reporting.
/// Different strategies for different error types (transient vs permanent).
/// </summary>
public class ErrorHandlingMiddleware
{
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    private readonly int _maxRetries;
    private readonly TimeSpan _retryDelay;

    public ErrorHandlingMiddleware(
        ILogger<ErrorHandlingMiddleware> logger,
        int maxRetries = 3,
        int retryDelayMs = 1000)
    {
        _logger = logger;
        _maxRetries = maxRetries;
        _retryDelay = TimeSpan.FromMilliseconds(retryDelayMs);
    }

    /// <summary>
    /// Wraps a function with error handling, retry logic, and logging.
    /// Treats different exception types appropriately.
    /// </summary>
    public async Task<T> ExecuteWithErrorHandlingAsync<T>(
        Func<Task<T>> operation,
        string operationName,
        T? defaultValue = default)
    {
        int attempt = 0;

        while (attempt < _maxRetries)
        {
            try
            {
                _logger.LogDebug($"Executing {operationName} (attempt {attempt + 1}/{_maxRetries})");
                return await operation();
            }
            catch (AnalysisException ex) when (IsTransientError(ex))
            {
                attempt++;
                if (attempt >= _maxRetries)
                {
                    _logger.LogError(ex, $"{operationName} failed after {_maxRetries} attempts");
                    throw;
                }

                _logger.LogWarning($"{operationName} failed (transient). Retrying in {_retryDelay.TotalMilliseconds}ms");
                await Task.Delay(_retryDelay);
            }
            catch (AnalysisException ex)
            {
                _logger.LogError(ex, $"{operationName} failed with analysis error: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{operationName} failed unexpectedly");
                throw;
            }
        }

        _logger.LogWarning($"{operationName} exhausted retries, returning default value");
        return defaultValue!;
    }

    /// <summary>
    /// Determines if an exception is likely transient (connection timeout, temporary resource unavailable).
    /// Transient errors are candidates for retry logic.
    /// </summary>
    private bool IsTransientError(AnalysisException ex)
    {
        // Connection-related errors are typically transient
        return ex.Message.Contains("connection", StringComparison.OrdinalIgnoreCase) ||
               ex.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase) ||
               ex.Message.Contains("unavailable", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Creates a summary error report for logging and user notification.
    /// Includes error context, suggestions for resolution.
    /// </summary>
    public ErrorReport CreateErrorReport(Exception ex, string context)
    {
        return new ErrorReport
        {
            ErrorMessage = ex.Message,
            ErrorType = ex.GetType().Name,
            StackTrace = ex.StackTrace ?? "No stack trace available",
            Context = context,
            Timestamp = DateTime.UtcNow,
            IsRecoverable = IsTransientError(ex as AnalysisException),
            Suggestion = GetRecoverySuggestion(ex)
        };
    }

    /// <summary>
    /// Provides actionable recovery suggestions based on error type.
    /// Helps users understand what went wrong and how to fix it.
    /// </summary>
    private string GetRecoverySuggestion(Exception ex)
    {
        return ex switch
        {
            FileNotFoundException => "Query file not found. Verify the file path exists.",
            ArgumentException => "Invalid query or parameters. Review the input and try again.",
            TimeoutException => "Operation timed out. Try reducing query complexity or increasing timeout.",
            AnalysisException ae when ae.Message.Contains("connection") =>
                "Database connection failed. Verify connection string and database availability.",
            _ => "An unexpected error occurred. Check logs for more details."
        };
    }
}

/// <summary>
/// Represents a comprehensive error report with context and recovery information.
/// Used for user-facing error messages and diagnostic logging.
/// </summary>
public class ErrorReport
{
    public string ErrorMessage { get; set; } = string.Empty;
    public string ErrorType { get; set; } = string.Empty;
    public string StackTrace { get; set; } = string.Empty;
    public string Context { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public bool IsRecoverable { get; set; }
    public string Suggestion { get; set; } = string.Empty;

    /// <summary>
    /// Formats error report as user-friendly message.
    /// </summary>
    public override string ToString() =>
        $"Error: {ErrorMessage}\n" +
        $"Type: {ErrorType}\n" +
        $"Context: {Context}\n" +
        $"Recoverable: {(IsRecoverable ? "Yes - retry may succeed" : "No - fix required")}\n" +
        $"Suggestion: {Suggestion}\n" +
        $"Time: {Timestamp:yyyy-MM-dd HH:mm:ss}";
}

/// <summary>
/// Graceful degradation strategy when part of analysis fails.
/// Returns partial results rather than complete failure.
/// </summary>
public class DegradationStrategy
{
    private readonly ILogger<DegradationStrategy> _logger;

    public DegradationStrategy(ILogger<DegradationStrategy> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Attempts operation with fallback to simpler alternative.
    /// Useful when advanced analysis features fail.
    /// </summary>
    public async Task<T> ExecuteWithDegradationAsync<T>(
        Func<Task<T>> primaryOperation,
        Func<Task<T>> degradedOperation,
        string operationName)
    {
        try
        {
            return await primaryOperation();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                $"{operationName} failed, attempting degraded operation: {ex.Message}");

            try
            {
                var result = await degradedOperation();
                _logger.LogInformation($"Degraded operation succeeded");
                return result;
            }
            catch (Exception degradedEx)
            {
                _logger.LogError($"Degraded operation also failed: {degradedEx.Message}");
                throw;
            }
        }
    }
}
