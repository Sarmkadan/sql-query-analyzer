#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Generic;
using System.Globalization;

namespace SqlQueryAnalyzer.Integration;

/// <summary>
/// Provides validation helpers for <see cref="HttpQueryAnalysisClient"/> instances.
/// Validates constructor arguments, method parameters, and internal state.
/// </summary>
public static class HttpQueryAnalysisClientValidation
{
    /// <summary>
    /// Validates the specified <see cref="HttpQueryAnalysisClient"/> instance.
    /// </summary>
    /// <param name="value">The HTTP query analysis client to validate.</param>
    /// <returns>An immutable list of validation errors; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this HttpQueryAnalysisClient? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // The HttpQueryAnalysisClient class has private fields but exposes public methods
        // We validate the instance itself (non-null check already done above)
        // Additional validation would require reflection or exposing internal state
        // For now, we consider the instance valid if it's not null

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="HttpQueryAnalysisClient"/> instance is valid.
    /// </summary>
    /// <param name="value">The HTTP query analysis client to check.</param>
    /// <returns>True if the instance is valid; otherwise false.</returns>
    public static bool IsValid(this HttpQueryAnalysisClient? value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="HttpQueryAnalysisClient"/> instance is valid.
    /// </summary>
    /// <param name="value">The HTTP query analysis client to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails, containing error messages.</exception>
    public static void EnsureValid(this HttpQueryAnalysisClient? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);

        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"HttpQueryAnalysisClient validation failed:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", errors)}");
        }
    }

    /// <summary>
    /// Validates the query parameter for <see cref="HttpQueryAnalysisClient.AnalyzeQueryAsync(string)"/>.
    /// </summary>
    /// <param name="query">The SQL query to validate.</param>
    /// <returns>An immutable list of validation errors; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateQuery(this string? query)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(query))
        {
            errors.Add("Query cannot be null, empty, or whitespace.");
        }
        else if (query.Length > 100000)
        {
            errors.Add("Query length cannot exceed 100,000 characters.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified query is valid.
    /// </summary>
    /// <param name="query">The SQL query to check.</param>
    /// <returns>True if the query is valid; otherwise false.</returns>
    public static bool IsValidQuery(this string? query)
    {
        return ValidateQuery(query).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified query is valid.
    /// </summary>
    /// <param name="query">The SQL query to validate.</param>
    /// <exception cref="ArgumentException">Thrown when validation fails, containing error messages.</exception>
    public static void EnsureValidQuery(this string? query)
    {
        var errors = ValidateQuery(query);

        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"Query validation failed:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", errors)}");
        }
    }

    /// <summary>
    /// Validates the queries array for <see cref="HttpQueryAnalysisClient.AnalyzeBatchAsync(string[])"/>.
    /// </summary>
    /// <param name="queries">The array of SQL queries to validate.</param>
    /// <returns>An immutable list of validation errors; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateQueries(this string[]? queries)
    {
        var errors = new List<string>();

        if (queries is null)
        {
            errors.Add("Queries array cannot be null.");
            return errors.AsReadOnly();
        }

        if (queries.Length == 0)
        {
            errors.Add("Queries array cannot be empty.");
        }
        else if (queries.Length > 1000)
        {
            errors.Add("Queries array cannot contain more than 1,000 queries.");
        }

        for (var i = 0; i < queries.Length; i++)
        {
            var query = queries[i];
            if (string.IsNullOrWhiteSpace(query))
            {
                errors.Add($"Query at index {i} cannot be null, empty, or whitespace.");
                break;
            }
            else if (query.Length > 100000)
            {
                errors.Add($"Query at index {i} length cannot exceed 100,000 characters.");
                break;
            }
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified queries array is valid.
    /// </summary>
    /// <param name="queries">The array of SQL queries to check.</param>
    /// <returns>True if the queries array is valid; otherwise false.</returns>
    public static bool IsValidQueries(this string[]? queries)
    {
        return ValidateQueries(queries).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified queries array is valid.
    /// </summary>
    /// <param name="queries">The array of SQL queries to validate.</param>
    /// <exception cref="ArgumentException">Thrown when validation fails, containing error messages.</exception>
    public static void EnsureValidQueries(this string[]? queries)
    {
        var errors = ValidateQueries(queries);

        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"Queries array validation failed:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", errors)}");
        }
    }

    /// <summary>
    /// Validates the options dictionary for query analysis.
    /// </summary>
    /// <param name="options">The options dictionary to validate.</param>
    /// <returns>An immutable list of validation errors; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateOptions(this Dictionary<string, string>? options)
    {
        var errors = new List<string>();

        if (options is null)
        {
            return errors.AsReadOnly();
        }

        if (options.Count > 100)
        {
            errors.Add("Options dictionary cannot contain more than 100 entries.");
        }

        foreach (var (key, value) in options)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                errors.Add("Option key cannot be null, empty, or whitespace.");
                break;
            }

            if (key.Length > 100)
            {
                errors.Add("Option key cannot exceed 100 characters.");
                break;
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                errors.Add($"Option value for key '{key}' cannot be null, empty, or whitespace.");
                break;
            }

            if (value.Length > 1000)
            {
                errors.Add($"Option value for key '{key}' cannot exceed 1,000 characters.");
                break;
            }
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified options dictionary is valid.
    /// </summary>
    /// <param name="options">The options dictionary to check.</param>
    /// <returns>True if the options dictionary is valid; otherwise false.</returns>
    public static bool IsValidOptions(this Dictionary<string, string>? options)
    {
        return ValidateOptions(options).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified options dictionary is valid.
    /// </summary>
    /// <param name="options">The options dictionary to validate.</param>
    /// <exception cref="ArgumentException">Thrown when validation fails, containing error messages.</exception>
    public static void EnsureValidOptions(this Dictionary<string, string>? options)
    {
        var errors = ValidateOptions(options);

        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"Options dictionary validation failed:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", errors)}");
        }
    }

    /// <summary>
    /// Validates the max degree of parallelism parameter.
    /// </summary>
    /// <param name="maxDegreeOfParallelism">The maximum degree of parallelism.</param>
    /// <returns>An immutable list of validation errors; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateMaxDegreeOfParallelism(this int? maxDegreeOfParallelism)
    {
        var errors = new List<string>();

        if (maxDegreeOfParallelism.HasValue)
        {
            var value = maxDegreeOfParallelism.Value;
            if (value <= 0)
            {
                errors.Add("Max degree of parallelism must be greater than zero when specified.");
            }
            else if (value > 100)
            {
                errors.Add("Max degree of parallelism cannot exceed 100.");
            }
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified max degree of parallelism is valid.
    /// </summary>
    /// <param name="maxDegreeOfParallelism">The maximum degree of parallelism to check.</param>
    /// <returns>True if the max degree of parallelism is valid; otherwise false.</returns>
    public static bool IsValidMaxDegreeOfParallelism(this int? maxDegreeOfParallelism)
    {
        return ValidateMaxDegreeOfParallelism(maxDegreeOfParallelism).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified max degree of parallelism is valid.
    /// </summary>
    /// <param name="maxDegreeOfParallelism">The maximum degree of parallelism to validate.</param>
    /// <exception cref="ArgumentException">Thrown when validation fails, containing error messages.</exception>
    public static void EnsureValidMaxDegreeOfParallelism(this int? maxDegreeOfParallelism)
    {
        var errors = ValidateMaxDegreeOfParallelism(maxDegreeOfParallelism);

        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"Max degree of parallelism validation failed:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", errors)}");
        }
    }

    /// <summary>
    /// Validates the timeout parameter in seconds.
    /// </summary>
    /// <param name="timeoutSeconds">The timeout in seconds.</param>
    /// <returns>An immutable list of validation errors; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateTimeoutSeconds(this int timeoutSeconds)
    {
        var errors = new List<string>();

        if (timeoutSeconds <= 0)
        {
            errors.Add("Timeout seconds must be greater than zero.");
        }
        else if (timeoutSeconds > 3600)
        {
            errors.Add("Timeout seconds cannot exceed 3,600 (1 hour).");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified timeout in seconds is valid.
    /// </summary>
    /// <param name="timeoutSeconds">The timeout in seconds to check.</param>
    /// <returns>True if the timeout is valid; otherwise false.</returns>
    public static bool IsValidTimeoutSeconds(this int timeoutSeconds)
    {
        return ValidateTimeoutSeconds(timeoutSeconds).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified timeout in seconds is valid.
    /// </summary>
    /// <param name="timeoutSeconds">The timeout in seconds to validate.</param>
    /// <exception cref="ArgumentException">Thrown when validation fails, containing error messages.</exception>
    public static void EnsureValidTimeoutSeconds(this int timeoutSeconds)
    {
        var errors = ValidateTimeoutSeconds(timeoutSeconds);

        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"Timeout seconds validation failed:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", errors)}");
        }
    }
}