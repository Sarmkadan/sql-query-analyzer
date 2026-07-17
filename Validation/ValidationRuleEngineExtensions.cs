using System;
using System.Collections.Generic;

namespace SqlQueryAnalyzer.Validation;

/// <summary>
/// Provides extension methods for <see cref="ValidationRuleEngine"/>.
/// </summary>
public static class ValidationRuleEngineExtensions
{
    /// <summary>
    /// Validates a query and checks if it is valid.
    /// </summary>
    /// <param name="engine">The validation rule engine.</param>
    /// <param name="query">The SQL query to validate.</param>
    /// <returns>True if the query is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="engine"/> is null.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="query"/> is null.
    /// </exception>
    public static bool IsValidQuery(this ValidationRuleEngine engine, string query)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(query);
        return engine.ValidateQuery(query).IsValid;
    }

    /// <summary>
    /// Validates a query and returns the count of errors found.
    /// </summary>
    /// <param name="engine">The validation rule engine.</param>
    /// <param name="query">The SQL query to validate.</param>
    /// <returns>The number of errors.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="engine"/> is null.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="query"/> is null.
    /// </exception>
    public static int GetErrorCount(this ValidationRuleEngine engine, string query)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(query);
        return engine.ValidateQuery(query).Errors.Count;
    }

    /// <summary>
    /// Validates a query and returns the count of warnings found.
    /// </summary>
    /// <param name="engine">The validation rule engine.</param>
    /// <param name="query">The SQL query to validate.</param>
    /// <returns>The number of warnings.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="engine"/> is null.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="query"/> is null.
    /// </exception>
    public static int GetWarningCount(this ValidationRuleEngine engine, string query)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(query);
        return engine.ValidateQuery(query).Warnings.Count;
    }
}
