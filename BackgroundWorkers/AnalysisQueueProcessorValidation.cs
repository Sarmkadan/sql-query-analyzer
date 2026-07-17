#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Globalization;

namespace SqlQueryAnalyzer.BackgroundWorkers;

/// <summary>
/// Provides validation helpers for <see cref="AnalysisTask"/> instances.
/// Validates all public members according to their semantic meaning and constraints.
/// </summary>
public static class AnalysisQueueProcessorValidation
{
    /// <summary>
    /// Validates the specified <see cref="AnalysisTask"/> instance.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>A list of validation problems (empty if valid).</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this AnalysisTask? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate TaskId
        if (string.IsNullOrWhiteSpace(value.TaskId))
        {
            errors.Add("TaskId cannot be null, empty, or whitespace.");
        }

        // Validate Query
        if (string.IsNullOrWhiteSpace(value.Query))
        {
            errors.Add("Query cannot be null, empty, or whitespace.");
        }

        // Validate Status
        // AnalysisTaskStatus enum values are always valid, no validation needed

        // Validate CreatedAt
        if (value.CreatedAt == default)
        {
            errors.Add("CreatedAt cannot be the default DateTime value.");
        }
        else if (value.CreatedAt > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add("CreatedAt cannot be in the future.");
        }

        // Validate StartedAt
        if (value.StartedAt.HasValue)
        {
            if (value.StartedAt.Value == default)
            {
                errors.Add("StartedAt cannot be the default DateTime value.");
            }
            else if (value.StartedAt.Value < value.CreatedAt)
            {
                errors.Add("StartedAt cannot be earlier than CreatedAt.");
            }
            else if (value.StartedAt.Value > DateTime.UtcNow.AddMinutes(5))
            {
                errors.Add("StartedAt cannot be in the future.");
            }
        }

        // Validate CompletedAt
        if (value.CompletedAt.HasValue)
        {
            if (value.CompletedAt.Value == default)
            {
                errors.Add("CompletedAt cannot be the default DateTime value.");
            }
            else if (value.CompletedAt.Value < value.CreatedAt)
            {
                errors.Add("CompletedAt cannot be earlier than CreatedAt.");
            }
            else if (value.StartedAt.HasValue && value.CompletedAt.Value < value.StartedAt.Value)
            {
                errors.Add("CompletedAt cannot be earlier than StartedAt.");
            }
            else if (value.CompletedAt.Value > DateTime.UtcNow.AddMinutes(5))
            {
                errors.Add("CompletedAt cannot be in the future.");
            }
        }

        // Validate Result
        // QueryAnalysisResult is a complex object, we validate its presence based on Status
        if (value.Status == AnalysisTaskStatus.Completed && value.Result == null)
        {
            errors.Add("Result must be set when Status is Completed.");
        }

        if (value.Status != AnalysisTaskStatus.Completed && value.Result != null)
        {
            errors.Add("Result should only be set when Status is Completed.");
        }

        // Validate ErrorMessage
        if (value.Status == AnalysisTaskStatus.Failed && string.IsNullOrWhiteSpace(value.ErrorMessage))
        {
            errors.Add("ErrorMessage must be set when Status is Failed.");
        }

        if (value.Status != AnalysisTaskStatus.Failed && !string.IsNullOrWhiteSpace(value.ErrorMessage))
        {
            errors.Add("ErrorMessage should only be set when Status is Failed.");
        }

        // Validate time consistency
        if (value.StartedAt.HasValue && value.CompletedAt.HasValue)
        {
            var processingTime = value.CompletedAt.Value - value.StartedAt.Value;
            if (processingTime.TotalMilliseconds < 0)
            {
                errors.Add("CompletedAt cannot be earlier than StartedAt.");
            }
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="AnalysisTask"/> is valid.
    /// </summary>
    /// <param name="value">The instance to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this AnalysisTask? value) => value is not null && Validate(value).Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="AnalysisTask"/> is valid.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <exception cref="ArgumentException">Thrown if the instance is not valid, containing details of all validation problems.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static void EnsureValid(this AnalysisTask? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"AnalysisTask is not valid. Problems:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }
}