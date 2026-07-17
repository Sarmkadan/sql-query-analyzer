#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Text;
using SqlQueryAnalyzer.Exceptions;

namespace SqlQueryAnalyzer.Exceptions;

/// <summary>
/// Extension methods for <see cref="SqlQueryAnalyzerException"/> and derived exception types.
/// Provides utility methods for exception handling, formatting, and analysis.
/// </summary>
/// <remarks>
/// All extension methods validate input parameters and throw <see cref="ArgumentNullException"/>
/// for null arguments. Methods use pattern matching for type-safe exception handling.
/// </remarks>
public static class SqlQueryAnalyzerExceptionExtensions
{
    /// <summary>
    /// Creates a formatted error message from the exception, including error code and details.
    /// </summary>
    /// <param name="exception">The exception to format. Cannot be null.</param>
    /// <returns>Formatted error message string</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is null.</exception>
    public static string ToErrorMessage(this SqlQueryAnalyzerException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var builder = new StringBuilder();
        builder.AppendLine(exception.Message);

        if (exception is AnalysisException { ErrorCode: { } errorCode } analysisEx && !string.IsNullOrEmpty(errorCode))
        {
            builder.AppendLine($"Error Code: {errorCode}");
        }

        if (exception is AnalysisException { ErrorDetails: { } errorDetails } analysisEx2 && !string.IsNullOrEmpty(errorDetails))
        {
            builder.AppendLine($"Details: {errorDetails}");
        }

        if (exception is InvalidQueryException { Query: { } query } invalidQueryEx)
        {
            builder.AppendLine($"Query: {query}");

            if (invalidQueryEx.LineNumber.HasValue)
            {
                builder.AppendLine($"Line: {invalidQueryEx.LineNumber}");
            }

            if (invalidQueryEx.ColumnNumber.HasValue)
            {
                builder.AppendLine($"Column: {invalidQueryEx.ColumnNumber}");
            }
        }

        if (exception is DatabaseConnectionException { DatabaseName: { } databaseName } dbEx)
        {
            builder.AppendLine($"Database: {databaseName}");
        }

        if (exception is QueryPlanException { PlanSource: { } planSource } planEx)
        {
            builder.AppendLine($"Plan Source: {planSource}");
        }

        if (exception.InnerException != null)
        {
            builder.AppendLine($"Inner Exception: {exception.InnerException.GetType().Name}");
        }

        return builder.ToString().Trim();
    }

    /// <summary>
    /// Determines if the exception represents a query validation error.
    /// </summary>
    /// <param name="exception">The exception to check. Cannot be null.</param>
    /// <returns>True if the exception is related to query validation</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is null.</exception>
    public static bool IsQueryValidationError(this SqlQueryAnalyzerException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return exception is InvalidQueryException ||
            (exception is AnalysisException analysisEx &&
             analysisEx.ErrorCode == "INVALID_QUERY");
    }

    /// <summary>
    /// Determines if the exception represents a database connection error.
    /// </summary>
    /// <param name="exception">The exception to check. Cannot be null.</param>
    /// <returns>True if the exception is related to database connection</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is null.</exception>
    public static bool IsDatabaseConnectionError(this SqlQueryAnalyzerException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return exception is DatabaseConnectionException ||
            (exception is AnalysisException analysisEx &&
             analysisEx.ErrorCode == "DB_CONNECTION_ERROR");
    }

    /// <summary>
    /// Determines if the exception represents a query plan analysis error.
    /// </summary>
    /// <param name="exception">The exception to check. Cannot be null.</param>
    /// <returns>True if the exception is related to query plan analysis</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is null.</exception>
    public static bool IsQueryPlanError(this SqlQueryAnalyzerException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return exception is QueryPlanException ||
            (exception is AnalysisException analysisEx &&
             analysisEx.ErrorCode == "PLAN_ERROR");
    }

    /// <summary>
    /// Safely extracts the error code from the exception if available.
    /// Returns null for base SqlQueryAnalyzerException without error code.
    /// </summary>
    /// <param name="exception">The exception to extract error code from. Cannot be null.</param>
    /// <returns>Error code string or null</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is null.</exception>
    public static string? GetErrorCode(this SqlQueryAnalyzerException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return (exception as AnalysisException)?.ErrorCode;
    }

    /// <summary>
    /// Safely extracts the error details from the exception if available.
    /// Returns null for base SqlQueryAnalyzerException without error details.
    /// </summary>
    /// <param name="exception">The exception to extract error details from. Cannot be null.</param>
    /// <returns>Error details string or null</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is null.</exception>
    public static string? GetErrorDetails(this SqlQueryAnalyzerException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return (exception as AnalysisException)?.ErrorDetails;
    }

    /// <summary>
    /// Creates a user-friendly error summary suitable for logging or display.
    /// </summary>
    /// <param name="exception">The exception to summarize. Cannot be null.</param>
    /// <param name="includeStackTrace">Whether to include stack trace (default: false)</param>
    /// <returns>Formatted summary string</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is null.</exception>
    public static string ToUserFriendlySummary(
        this SqlQueryAnalyzerException exception,
        bool includeStackTrace = false)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var builder = new StringBuilder();
        builder.AppendLine($"SQL Query Analyzer Error: {exception.Message}");

        var errorCode = exception.GetErrorCode();
        if (!string.IsNullOrEmpty(errorCode))
        {
            builder.AppendLine($"Error Code: {errorCode}");
        }

        if (exception is InvalidQueryException { Query: { } query } invalidQueryEx && !string.IsNullOrEmpty(query))
        {
            var queryPreview = query.Length > 100
                ? query[..100] + "..."
                : query;
            builder.AppendLine($"Invalid Query: {queryPreview}");
        }

        if (exception is DatabaseConnectionException { DatabaseName: { } databaseName })
        {
            builder.AppendLine($"Database: {databaseName}");
        }

        if (includeStackTrace && !string.IsNullOrEmpty(exception.StackTrace))
        {
            builder.AppendLine();
            builder.AppendLine("Stack Trace:");
            builder.AppendLine(exception.StackTrace);
        }

        return builder.ToString().Trim();
    }

    /// <summary>
    /// Determines if the exception is a critical error that should halt processing.
    /// </summary>
    /// <param name="exception">The exception to check. Cannot be null.</param>
    /// <returns>True if the exception is critical</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is null.</exception>
    public static bool IsCriticalError(this SqlQueryAnalyzerException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var errorCode = exception.GetErrorCode();

        // Critical errors typically have error codes starting with critical patterns
        return !string.IsNullOrEmpty(errorCode) &&
            (errorCode.StartsWith("CRITICAL_", StringComparison.OrdinalIgnoreCase) ||
             errorCode.StartsWith("FATAL_", StringComparison.OrdinalIgnoreCase) ||
             errorCode == "DB_CONNECTION_ERROR" ||
             errorCode == "INTEGRATION_ERROR");
    }

    /// <summary>
    /// Creates a detailed exception report with all available information.
    /// Useful for debugging and error reporting.
    /// </summary>
    /// <param name="exception">The exception to report. Cannot be null.</param>
    /// <returns>Detailed exception report</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is null.</exception>
    public static string GenerateExceptionReport(this SqlQueryAnalyzerException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var builder = new StringBuilder();
        builder.AppendLine("=== SQL Query Analyzer Exception Report ===");
        builder.AppendLine();

        builder.AppendLine("[Exception Type]");
        builder.AppendLine(exception.GetType().FullName ?? exception.GetType().Name);
        builder.AppendLine();

        builder.AppendLine("[Message]");
        builder.AppendLine(exception.Message);
        builder.AppendLine();

        builder.AppendLine("[Error Code]");
        builder.AppendLine(exception.GetErrorCode() ?? "N/A");
        builder.AppendLine();

        builder.AppendLine("[Error Details]");
        builder.AppendLine(exception.GetErrorDetails() ?? "N/A");
        builder.AppendLine();

        if (exception is InvalidQueryException { Query: { } query } invalidQueryEx)
        {
            builder.AppendLine("[Query Information]");
            builder.AppendLine($"Query: {query}");
            builder.AppendLine($"Line: {invalidQueryEx.LineNumber?.ToString() ?? "N/A"}");
            builder.AppendLine($"Column: {invalidQueryEx.ColumnNumber?.ToString() ?? "N/A"}");
            builder.AppendLine();
        }

        if (exception is DatabaseConnectionException { DatabaseName: { } databaseName } dbEx)
        {
            builder.AppendLine("[Database Information]");
            builder.AppendLine($"Database: {databaseName}");
            builder.AppendLine($"Connection String: {dbEx.ConnectionString?.Substring(0, Math.Min(50, dbEx.ConnectionString.Length)) ?? "N/A"}");
            builder.AppendLine();
        }

        if (exception is QueryPlanException { PlanSource: { } planSource } planEx)
        {
            builder.AppendLine("[Query Plan Information]");
            builder.AppendLine($"Plan Source: {planSource}");
            builder.AppendLine();
        }

        builder.AppendLine("[Is Critical Error]");
        builder.AppendLine(exception.IsCriticalError() ? "Yes" : "No");
        builder.AppendLine();

        if (exception.InnerException != null)
        {
            builder.AppendLine("[Inner Exception]");
            builder.AppendLine(exception.InnerException.ToString());
            builder.AppendLine();
        }

        builder.AppendLine("[Stack Trace]");
        builder.AppendLine(exception.StackTrace ?? "N/A");
        builder.AppendLine();

        builder.AppendLine("=== End of Report ===");

        return builder.ToString();
    }
}