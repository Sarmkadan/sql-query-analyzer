#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using System.Globalization;

namespace SqlQueryAnalyzer.Middleware;

/// <summary>
/// Validation helpers for <see cref="ErrorHandlingMiddlewareExtensions"/> extension methods.
/// Provides validation logic for ensuring extension method parameters are valid.
/// </summary>
public static class ErrorHandlingMiddlewareExtensionsValidation
{
    /// <summary>
    /// Validates the parameters passed to <see cref="ErrorHandlingMiddlewareExtensions.ExecuteWithErrorHandlingAsync"/>.
    /// </summary>
    /// <returns>List of validation problems; empty if valid</returns>
    public static IReadOnlyList<string> Validate()
    {
        return Array.Empty<string>();
    }

    /// <summary>
    /// Checks if the parameters passed to <see cref="ErrorHandlingMiddlewareExtensions.ExecuteWithErrorHandlingAsync"/> are valid.
    /// </summary>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValid()
    {
        return true;
    }

    /// <summary>
    /// Ensures the parameters passed to <see cref="ErrorHandlingMiddlewareExtensions.ExecuteWithErrorHandlingAsync"/> are valid.
    /// Throws <see cref="ArgumentException"/> if validation fails.
    /// </summary>
    /// <exception cref="ArgumentException">Validation failed with specific error messages</exception>
    public static void EnsureValid()
    {
        // No validation needed for this method
    }

    /// <summary>
    /// Validates the parameters passed to <see cref="ErrorHandlingMiddlewareExtensions.CreateErrorReport"/>.
    /// </summary>
    /// <param name="errorMessage">The error message to validate</param>
    /// <param name="context">The context where the error occurred</param>
    /// <returns>List of validation problems; empty if valid</returns>
    /// <exception cref="ArgumentNullException"><paramref name="errorMessage"/> is null</exception>
    /// <exception cref="ArgumentException"><paramref name="context"/> is null or empty</exception>
    public static IReadOnlyList<string> Validate(
        string? errorMessage,
        string? context)
    {
        ArgumentNullException.ThrowIfNull(errorMessage);
        ArgumentException.ThrowIfNullOrEmpty(context);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(errorMessage))
        {
            problems.Add("Error message cannot be null, empty, or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(context))
        {
            problems.Add("Context cannot be null, empty, or whitespace.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if the parameters passed to <see cref="ErrorHandlingMiddlewareExtensions.CreateErrorReport"/> are valid.
    /// </summary>
    /// <param name="errorMessage">The error message to validate</param>
    /// <param name="context">The context where the error occurred</param>
    /// <returns>True if valid; false otherwise</returns>
    /// <exception cref="ArgumentNullException"><paramref name="errorMessage"/> is null</exception>
    /// <exception cref="ArgumentException"><paramref name="context"/> is null or empty</exception>
    public static bool IsValid(
        string? errorMessage,
        string? context)
    {
        return Validate(errorMessage, context).Count == 0;
    }

    /// <summary>
    /// Ensures the parameters passed to <see cref="ErrorHandlingMiddlewareExtensions.CreateErrorReport"/> are valid.
    /// Throws <see cref="ArgumentException"/> if validation fails.
    /// </summary>
    /// <param name="errorMessage">The error message to validate</param>
    /// <param name="context">The context where the error occurred</param>
    /// <exception cref="ArgumentNullException"><paramref name="errorMessage"/> is null</exception>
    /// <exception cref="ArgumentException"><paramref name="context"/> is null or empty</exception>
    /// <exception cref="ArgumentException">Validation failed with specific error messages</exception>
    public static void EnsureValid(
        string? errorMessage,
        string? context)
    {
        var problems = Validate(errorMessage, context);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Validation failed for ErrorHandlingMiddlewareExtensions.CreateErrorReport:\n{string.Join("\n", problems)}");
        }
    }

    /// <summary>
    /// Validates the parameters passed to <see cref="ErrorHandlingMiddlewareExtensions.ExecuteWithRetryAsync{T}"/>.
    /// </summary>
    /// <param name="operation">The operation to validate</param>
    /// <param name="operationName">The descriptive name for logging</param>
    /// <param name="maxRetries">Maximum number of retry attempts</param>
    /// <returns>List of validation problems; empty if valid</returns>
    /// <exception cref="ArgumentNullException"><paramref name="operation"/> is null</exception>
    /// <exception cref="ArgumentException"><paramref name="operationName"/> is null or empty</exception>
    public static IReadOnlyList<string> Validate<T>(
        Func<Task<T>>? operation,
        string? operationName,
        int maxRetries)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentException.ThrowIfNullOrEmpty(operationName);

        var problems = new List<string>();

        if (maxRetries < 1)
        {
            problems.Add("Max retries must be at least 1.");
        }

        if (string.IsNullOrWhiteSpace(operationName))
        {
            problems.Add("Operation name cannot be null, empty, or whitespace.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if the parameters passed to <see cref="ErrorHandlingMiddlewareExtensions.ExecuteWithRetryAsync{T}"/> are valid.
    /// </summary>
    /// <param name="operation">The operation to validate</param>
    /// <param name="operationName">The descriptive name for logging</param>
    /// <param name="maxRetries">Maximum number of retry attempts</param>
    /// <returns>True if valid; false otherwise</returns>
    /// <exception cref="ArgumentNullException"><paramref name="operation"/> is null</exception>
    /// <exception cref="ArgumentException"><paramref name="operationName"/> is null or empty</exception>
    public static bool IsValid<T>(
        Func<Task<T>>? operation,
        string? operationName,
        int maxRetries)
    {
        return Validate(operation, operationName, maxRetries).Count == 0;
    }

    /// <summary>
    /// Ensures the parameters passed to <see cref="ErrorHandlingMiddlewareExtensions.ExecuteWithRetryAsync{T}"/> are valid.
    /// Throws <see cref="ArgumentException"/> if validation fails.
    /// </summary>
    /// <param name="operation">The operation to validate</param>
    /// <param name="operationName">The descriptive name for logging</param>
    /// <param name="maxRetries">Maximum number of retry attempts</param>
    /// <exception cref="ArgumentNullException"><paramref name="operation"/> is null</exception>
    /// <exception cref="ArgumentException"><paramref name="operationName"/> is null or empty</exception>
    /// <exception cref="ArgumentException">Validation failed with specific error messages</exception>
    public static void EnsureValid<T>(
        Func<Task<T>>? operation,
        string? operationName,
        int maxRetries)
    {
        var problems = Validate(operation, operationName, maxRetries);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Validation failed for ErrorHandlingMiddlewareExtensions.ExecuteWithRetryAsync<T>:\n{string.Join("\n", problems)}");
        }
    }

    /// <summary>
    /// Validates the parameters passed to <see cref="ErrorHandlingMiddlewareExtensions.FormatErrorMessage"/>.
    /// </summary>
    /// <param name="ex">The exception that occurred</param>
    /// <param name="context">Context where the error occurred</param>
    /// <returns>List of validation problems; empty if valid</returns>
    /// <exception cref="ArgumentNullException"><paramref name="ex"/> is null</exception>
    /// <exception cref="ArgumentException"><paramref name="context"/> is null or empty</exception>
    public static IReadOnlyList<string> Validate(
        Exception? ex,
        string? context)
    {
        ArgumentNullException.ThrowIfNull(ex);
        ArgumentException.ThrowIfNullOrEmpty(context);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(context))
        {
            problems.Add("Context cannot be null, empty, or whitespace.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if the parameters passed to <see cref="ErrorHandlingMiddlewareExtensions.FormatErrorMessage"/> are valid.
    /// </summary>
    /// <param name="ex">The exception that occurred</param>
    /// <param name="context">Context where the error occurred</param>
    /// <returns>True if valid; false otherwise</returns>
    /// <exception cref="ArgumentNullException"><paramref name="ex"/> is null</exception>
    /// <exception cref="ArgumentException"><paramref name="context"/> is null or empty</exception>
    public static bool IsValid(
        Exception? ex,
        string? context)
    {
        return Validate(ex, context).Count == 0;
    }

    /// <summary>
    /// Ensures the parameters passed to <see cref="ErrorHandlingMiddlewareExtensions.FormatErrorMessage"/> are valid.
    /// Throws <see cref="ArgumentException"/> if validation fails.
    /// </summary>
    /// <param name="ex">The exception that occurred</param>
    /// <param name="context">Context where the error occurred</param>
    /// <exception cref="ArgumentNullException"><paramref name="ex"/> is null</exception>
    /// <exception cref="ArgumentException"><paramref name="context"/> is null or empty</exception>
    /// <exception cref="ArgumentException">Validation failed with specific error messages</exception>
    public static void EnsureValid(
        Exception? ex,
        string? context)
    {
        var problems = Validate(ex, context);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Validation failed for ErrorHandlingMiddlewareExtensions.FormatErrorMessage:\n{string.Join("\n", problems)}");
        }
    }

    /// <summary>
    /// Validates the parameters passed to <see cref="ErrorHandlingMiddlewareExtensions.ExecuteWithCacheFallbackAsync{T}"/>.
    /// </summary>
    /// <param name="operation">The primary operation to attempt</param>
    /// <param name="cachedResultProvider">The cached result to return if operation fails</param>
    /// <param name="operationName">Descriptive name for logging</param>
    /// <returns>List of validation problems; empty if valid</returns>
    /// <exception cref="ArgumentNullException"><paramref name="operation"/> is null</exception>
    /// <exception cref="ArgumentNullException"><paramref name="cachedResultProvider"/> is null</exception>
    /// <exception cref="ArgumentException"><paramref name="operationName"/> is null or empty</exception>
    public static IReadOnlyList<string> Validate<T>(
        Func<Task<T>>? operation,
        Func<T>? cachedResultProvider,
        string? operationName)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(cachedResultProvider);
        ArgumentException.ThrowIfNullOrEmpty(operationName);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(operationName))
        {
            problems.Add("Operation name cannot be null, empty, or whitespace.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if the parameters passed to <see cref="ErrorHandlingMiddlewareExtensions.ExecuteWithCacheFallbackAsync{T}"/> are valid.
    /// </summary>
    /// <param name="operation">The primary operation to attempt</param>
    /// <param name="cachedResultProvider">The cached result to return if operation fails</param>
    /// <param name="operationName">Descriptive name for logging</param>
    /// <returns>True if valid; false otherwise</returns>
    /// <exception cref="ArgumentNullException"><paramref name="operation"/> is null</exception>
    /// <exception cref="ArgumentNullException"><paramref name="cachedResultProvider"/> is null</exception>
    /// <exception cref="ArgumentException"><paramref name="operationName"/> is null or empty</exception>
    public static bool IsValid<T>(
        Func<Task<T>>? operation,
        Func<T>? cachedResultProvider,
        string? operationName)
    {
        return Validate(operation, cachedResultProvider, operationName).Count == 0;
    }

    /// <summary>
    /// Ensures the parameters passed to <see cref="ErrorHandlingMiddlewareExtensions.ExecuteWithCacheFallbackAsync{T}"/> are valid.
    /// Throws <see cref="ArgumentException"/> if validation fails.
    /// </summary>
    /// <param name="operation">The primary operation to attempt</param>
    /// <param name="cachedResultProvider">The cached result to return if operation fails</param>
    /// <param name="operationName">Descriptive name for logging</param>
    /// <exception cref="ArgumentNullException"><paramref name="operation"/> is null</exception>
    /// <exception cref="ArgumentNullException"><paramref name="cachedResultProvider"/> is null</exception>
    /// <exception cref="ArgumentException"><paramref name="operationName"/> is null or empty</exception>
    /// <exception cref="ArgumentException">Validation failed with specific error messages</exception>
    public static void EnsureValid<T>(
        Func<Task<T>>? operation,
        Func<T>? cachedResultProvider,
        string? operationName)
    {
        var problems = Validate(operation, cachedResultProvider, operationName);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Validation failed for ErrorHandlingMiddlewareExtensions.ExecuteWithCacheFallbackAsync<T>:\n{string.Join("\n", problems)}");
        }
    }
}