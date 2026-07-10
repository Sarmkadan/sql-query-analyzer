#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using Microsoft.Extensions.Logging;
using SqlQueryAnalyzer.Exceptions;

namespace SqlQueryAnalyzer.Middleware;

/// <summary>
/// Extension methods for ErrorHandlingMiddleware providing additional utility functionality
/// for error handling, logging, and recovery operations.
/// </summary>
public static class ErrorHandlingMiddlewareExtensions
{
    /// <summary>
    /// Executes an action with error handling and returns void.
    /// Useful for fire-and-forget operations that still need error handling.
    /// </summary>
    /// <param name="middleware">The error handling middleware instance</param>
    /// <param name="action">The action to execute</param>
    /// <param name="operationName">Descriptive name for logging purposes</param>
    /// <returns>True if successful, false if operation failed</returns>
    public static async Task<bool> ExecuteWithErrorHandlingAsync(
        this ErrorHandlingMiddleware middleware,
        Func<Task> action,
        string operationName)
    {
        try
        {
            await middleware.ExecuteWithErrorHandlingAsync(async () =>
            {
                await action();
                return true;
            }, operationName, false);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Creates an error report from a string message and context.
    /// Useful for generating error reports from non-exception scenarios.
    /// </summary>
    /// <param name="middleware">The error handling middleware instance</param>
    /// <param name="errorMessage">The error message</param>
    /// <param name="context">Context where the error occurred</param>
    /// <returns>ErrorReport instance with the provided information</returns>
    public static ErrorReport CreateErrorReport(
        this ErrorHandlingMiddleware middleware,
        string errorMessage,
        string context)
    {
        return new ErrorReport
        {
            ErrorMessage = errorMessage,
            ErrorType = "ManualError",
            StackTrace = "No stack trace (manual error)",
            Context = context,
            Timestamp = DateTime.UtcNow,
            IsRecoverable = false,
            Suggestion = "Review the operation and retry if appropriate"
        };
    }

    /// <summary>
    /// Executes an operation with simple retry logic when it fails.
    /// Provides basic retry functionality without full degradation strategy.
    /// </summary>
    /// <param name="middleware">The error handling middleware instance</param>
    /// <param name="operation">The operation to attempt</param>
    /// <param name="operationName">Descriptive name for logging</param>
    /// <param name="maxRetries">Maximum number of retry attempts</param>
    /// <returns>The successful result</returns>
    public static async Task<T> ExecuteWithRetryAsync<T>(
        this ErrorHandlingMiddleware middleware,
        Func<Task<T>> operation,
        string operationName,
        int maxRetries = 3)
    {
        int attempt = 0;
        TimeSpan retryDelay = TimeSpan.FromMilliseconds(1000);

        while (attempt < maxRetries)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex) when (attempt < maxRetries - 1)
            {
                attempt++;
                if (attempt >= maxRetries)
                {
                    throw;
                }
                await Task.Delay(retryDelay);
            }
        }

        throw new InvalidOperationException("Should not reach this point");
    }

    /// <summary>
    /// Creates a formatted error message string for logging or user display.
    /// Combines error details into a single readable string.
    /// </summary>
    /// <param name="middleware">The error handling middleware instance</param>
    /// <param name="ex">The exception that occurred</param>
    /// <param name="context">Context where the error occurred</param>
    /// <returns>Formatted error message</returns>
    public static string FormatErrorMessage(
        this ErrorHandlingMiddleware middleware,
        Exception ex,
        string context)
    {
        var report = middleware.CreateErrorReport(ex, context);
        return $"[{report.Timestamp:yyyy-MM-dd HH:mm:ss}] [{report.ErrorType}] {report.ErrorMessage}\nContext: {report.Context}\nRecoverable: {report.IsRecoverable}\nSuggestion: {report.Suggestion}";
    }

    /// <summary>
    /// Attempts to execute an operation with automatic degradation to a cached result.
    /// Useful for operations that can benefit from cached results when real processing fails.
    /// </summary>
    /// <param name="middleware">The error handling middleware instance</param>
    /// <param name="operation">The primary operation to attempt</param>
    /// <param name="cachedResult">The cached result to return if operation fails</param>
    /// <param name="operationName">Descriptive name for logging</param>
    /// <returns>The successful result or cached result if operation fails</returns>
    public static async Task<T> ExecuteWithCacheFallbackAsync<T>(
        this ErrorHandlingMiddleware middleware,
        Func<Task<T>> operation,
        Func<T> cachedResultProvider,
        string operationName)
    {
        try
        {
            return await middleware.ExecuteWithErrorHandlingAsync(operation, operationName, cachedResultProvider());
        }
        catch
        {
            return cachedResultProvider();
        }
    }
}