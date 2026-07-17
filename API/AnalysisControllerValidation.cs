#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SqlQueryAnalyzer.API;

/// <summary>
/// Provides validation helpers for AnalysisController and related API types.
/// </summary>
public static class AnalysisControllerValidation
{
    /// <summary>
    /// Validates an AnalysisRequest instance.
    /// </summary>
    /// <param name="request">The request to validate</param>
    /// <returns>List of validation errors, empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="request"/> is null</exception>
    public static IReadOnlyList<string> Validate(this AnalysisRequest? request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Query))
        {
            errors.Add("Query cannot be null, empty, or whitespace");
        }

        if (request.Options is { Count: > 0 })
        {
            foreach (var option in request.Options)
            {
                if (string.IsNullOrWhiteSpace(option.Key))
                {
                    errors.Add("Options dictionary contains a null or empty key");
                }

                if (string.IsNullOrWhiteSpace(option.Value))
                {
                    errors.Add(string.IsNullOrWhiteSpace(option.Key)
                        ? "Option has a null or empty value"
                        : $"Option '{option.Key}' has a null or empty value");
                }
            }
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates a BatchAnalysisRequest instance.
    /// </summary>
    /// <param name="request">The request to validate</param>
    /// <returns>List of validation errors, empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="request"/> is null</exception>
    public static IReadOnlyList<string> Validate(this BatchAnalysisRequest? request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var errors = new List<string>();

        if (request.Queries is not { Length: > 0 })
        {
            errors.Add("Queries collection cannot be null or empty");
        }
        else
        {
            for (int i = 0; i < request.Queries.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(request.Queries[i]))
                {
                    errors.Add($"Query at index {i} cannot be null, empty, or whitespace");
                }
            }
        }

        if (request.MaxDegreeOfParallelism.HasValue)
        {
            if (request.MaxDegreeOfParallelism <= 0)
            {
                errors.Add("MaxDegreeOfParallelism must be a positive integer if specified");
            }
            else if (request.MaxDegreeOfParallelism > 64)
            {
                errors.Add("MaxDegreeOfParallelism cannot exceed 64 for practical parallelism limits");
            }
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates an ApiResponse instance.
    /// </summary>
    /// <param name="response">The response to validate</param>
    /// <returns>List of validation errors, empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="response"/> is null</exception>
    public static IReadOnlyList<string> Validate<T>(this ApiResponse<T>? response)
    {
        ArgumentNullException.ThrowIfNull(response);

        var errors = new List<string>();

        if (response.StatusCode is < 100 or > 599)
        {
            errors.Add("StatusCode must be between 100 and 599");
        }

        if (string.IsNullOrWhiteSpace(response.Message))
        {
            errors.Add("Message cannot be null, empty, or whitespace");
        }

        if (response.Timestamp == default)
        {
            errors.Add("Timestamp must be set to a valid DateTime");
        }
        else if (response.Timestamp.Kind != DateTimeKind.Utc)
        {
            errors.Add("Timestamp must be in UTC format");
        }
        else if (response.Timestamp > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add("Timestamp cannot be in the future");
        }

        if (response.Errors is null)
        {
            errors.Add("Errors collection cannot be null");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates a HealthStatus instance.
    /// </summary>
    /// <param name="status">The health status to validate</param>
    /// <returns>List of validation errors, empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="status"/> is null</exception>
    public static IReadOnlyList<string> Validate(this HealthStatus? status)
    {
        ArgumentNullException.ThrowIfNull(status);

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(status.Message))
        {
            errors.Add("Message cannot be null, empty, or whitespace");
        }

        if (string.IsNullOrWhiteSpace(status.Version))
        {
            errors.Add("Version cannot be null, empty, or whitespace");
        }

        if (status.Timestamp == default)
        {
            errors.Add("Timestamp must be set to a valid DateTime");
        }
        else if (status.Timestamp.Kind != DateTimeKind.Utc)
        {
            errors.Add("Timestamp must be in UTC format");
        }
        else if (status.Timestamp > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add("Timestamp cannot be in the future");
        }

        if (status.IsHealthy && !string.IsNullOrWhiteSpace(status.Message) && status.Message.Contains("failed", StringComparison.OrdinalIgnoreCase))
        {
            errors.Add("Healthy status should not contain failure-related messages");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified AnalysisRequest is valid.
    /// </summary>
    /// <param name="request">The request to check</param>
    /// <returns>True if valid; otherwise, false</returns>
    public static bool IsValid(this AnalysisRequest? request) => Validate(request).Count == 0;

    /// <summary>
    /// Determines whether the specified BatchAnalysisRequest is valid.
    /// </summary>
    /// <param name="request">The request to validate</param>
    /// <returns>True if valid; otherwise, false</returns>
    public static bool IsValid(this BatchAnalysisRequest? request) => Validate(request).Count == 0;

    /// <summary>
    /// Determines whether the specified ApiResponse is valid.
    /// </summary>
    /// <param name="response">The response to check</param>
    /// <returns>True if valid; otherwise, false</returns>
    public static bool IsValid<T>(this ApiResponse<T>? response) => Validate(response).Count == 0;

    /// <summary>
    /// Determines whether the specified HealthStatus is valid.
    /// </summary>
    /// <param name="status">The health status to check</param>
    /// <returns>True if valid; otherwise, false</returns>
    public static bool IsValid(this HealthStatus? status) => Validate(status).Count == 0;

    /// <summary>
    /// Ensures that the specified AnalysisRequest is valid, throwing an exception if not.
    /// </summary>
    /// <param name="request">The request to validate</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="request"/> is invalid</exception>
    public static void EnsureValid(this AnalysisRequest? request)
    {
        var errors = Validate(request);
        if (errors.Count > 0)
        {
            throw new ArgumentException($"AnalysisRequest is invalid. Errors: {string.Join("; ", errors)}");
        }
    }

    /// <summary>
    /// Ensures that the specified BatchAnalysisRequest is valid, throwing an exception if not.
    /// </summary>
    /// <param name="request">The request to validate</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="request"/> is invalid</exception>
    public static void EnsureValid(this BatchAnalysisRequest? request)
    {
        var errors = Validate(request);
        if (errors.Count > 0)
        {
            throw new ArgumentException($"BatchAnalysisRequest is invalid. Errors: {string.Join("; ", errors)}");
        }
    }

    /// <summary>
    /// Ensures that the specified ApiResponse is valid, throwing an exception if not.
    /// </summary>
    /// <param name="response">The response to validate</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="response"/> is invalid</exception>
    public static void EnsureValid<T>(this ApiResponse<T>? response)
    {
        var errors = Validate(response);
        if (errors.Count > 0)
        {
            throw new ArgumentException($"ApiResponse is invalid. Errors: {string.Join("; ", errors)}");
        }
    }

    /// <summary>
    /// Ensures that the specified HealthStatus is valid, throwing an exception if not.
    /// </summary>
    /// <param name="status">The health status to validate</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="status"/> is invalid</exception>
    public static void EnsureValid(this HealthStatus? status)
    {
        var errors = Validate(status);
        if (errors.Count > 0)
        {
            throw new ArgumentException($"HealthStatus is invalid. Errors: {string.Join("; ", errors)}");
        }
    }
}