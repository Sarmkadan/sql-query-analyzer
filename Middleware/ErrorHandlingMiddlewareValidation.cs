#nullable enable

using System.Globalization;

namespace SqlQueryAnalyzer.Middleware;

/// <summary>
/// Provides validation helpers for <see cref="ErrorHandlingMiddleware"/> instances.
/// Ensures middleware configuration and state are valid before use.
/// </summary>
public static class ErrorHandlingMiddlewareValidation
{
    /// <summary>
    /// Validates an <see cref="ErrorHandlingMiddleware"/> instance.
    /// </summary>
    /// <param name="value">The middleware instance to validate.</param>
    /// <returns>A list of validation problems (empty if valid).</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ErrorHandlingMiddleware value)
    {
        ArgumentNullException.ThrowIfNull(value);

        // Middleware validation requires actual instance inspection which isn't possible
        // through this static validation helper. Validation happens at runtime when
        // middleware processes requests.
        return Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether an <see cref="ErrorHandlingMiddleware"/> instance is valid.
    /// </summary>
    /// <param name="value">The middleware instance to check.</param>
    /// <returns>True if the instance is valid; otherwise, false.</returns>
    public static bool IsValid(this ErrorHandlingMiddleware value) => value is not null;

    /// <summary>
    /// Ensures that an <see cref="ErrorHandlingMiddleware"/> instance is valid.
    /// </summary>
    /// <param name="value">The middleware instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static void EnsureValid(this ErrorHandlingMiddleware value)
    {
        ArgumentNullException.ThrowIfNull(value);
    }

    /// <summary>
    /// Validates an <see cref="ErrorReport"/> instance.
    /// </summary>
    /// <param name="report">The error report to validate.</param>
    /// <returns>A list of validation problems (empty if valid).</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="report"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ErrorReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(report.ErrorMessage))
        {
            problems.Add("ErrorMessage cannot be null or whitespace");
        }

        if (string.IsNullOrWhiteSpace(report.ErrorType))
        {
            problems.Add("ErrorType cannot be null or whitespace");
        }

        if (string.IsNullOrWhiteSpace(report.StackTrace))
        {
            problems.Add("StackTrace cannot be null or whitespace");
        }

        if (string.IsNullOrWhiteSpace(report.Context))
        {
            problems.Add("Context cannot be null or whitespace");
        }

        if (report.Timestamp == default)
        {
            problems.Add("Timestamp must be a valid DateTime (cannot be default)");
        }

        if (string.IsNullOrWhiteSpace(report.Suggestion))
        {
            problems.Add("Suggestion cannot be null or whitespace");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether an <see cref="ErrorReport"/> instance is valid.
    /// </summary>
    /// <param name="report">The error report to check.</param>
    /// <returns>True if the report is valid; otherwise, false.</returns>
    public static bool IsValid(this ErrorReport report) => report?.Validate().Count == 0;

    /// <summary>
    /// Ensures that an <see cref="ErrorReport"/> instance is valid.
    /// </summary>
    /// <param name="report">The error report to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="report"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the report is not valid, containing a list of validation problems.</exception>
    public static void EnsureValid(this ErrorReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var problems = report.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"ErrorReport is not valid. Problems: {string.Join("; ", problems)}");
        }
    }

    /// <summary>
    /// Validates a <see cref="DegradationStrategy"/> instance.
    /// </summary>
    /// <param name="strategy">The degradation strategy to validate.</param>
    /// <returns>A list of validation problems (empty if valid).</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="strategy"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this DegradationStrategy strategy)
    {
        ArgumentNullException.ThrowIfNull(strategy);
        return Array.Empty<string>(); // DegradationStrategy has no public state to validate
    }

    /// <summary>
    /// Determines whether a <see cref="DegradationStrategy"/> instance is valid.
    /// </summary>
    /// <param name="strategy">The degradation strategy to check.</param>
    /// <returns>True if the strategy is valid; otherwise, false.</returns>
    public static bool IsValid(this DegradationStrategy strategy) => strategy is not null;

    /// <summary>
    /// Ensures that a <see cref="DegradationStrategy"/> instance is valid.
    /// </summary>
    /// <param name="strategy">The degradation strategy to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="strategy"/> is null.</exception>
    public static void EnsureValid(this DegradationStrategy strategy)
    {
        ArgumentNullException.ThrowIfNull(strategy);
    }
}