using System;
using System.Collections.Generic;

namespace SqlQueryAnalyzer.Models
{
    /// <summary>
    /// Provides validation helpers for <see cref="IndexSuggestion"/> instances.
    /// </summary>
    public static class IndexSuggestionValidation
    {
        /// <summary>
        /// Validates the specified <see cref="IndexSuggestion"/> instance.
        /// </summary>
        /// <param name="value">The index suggestion to validate.</param>
        /// <returns>A list of validation errors; empty if the instance is valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this IndexSuggestion value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = new List<string>();

            // Validate SuggestionId
            if (string.IsNullOrWhiteSpace(value.SuggestionId))
            {
                errors.Add("SuggestionId must be a non-empty string.");
            }

            // Validate TableName
            if (string.IsNullOrWhiteSpace(value.TableName))
            {
                errors.Add("TableName must be a non-empty string.");
            }

            // Validate IndexName
            if (string.IsNullOrWhiteSpace(value.IndexName))
            {
                errors.Add("IndexName must be a non-empty string.");
            }

            // Validate IndexColumns - required collection
            ArgumentNullException.ThrowIfNull(value.IndexColumns);
            if (value.IndexColumns.Count == 0)
            {
                errors.Add("IndexColumns must contain at least one column.");
            }
            else
            {
                for (int i = 0; i < value.IndexColumns.Count; i++)
                {
                    if (string.IsNullOrWhiteSpace(value.IndexColumns[i]))
                    {
                        errors.Add($"IndexColumns[{i}] must be a non-empty string.");
                    }
                }
            }

            // Validate IncludeColumns (optional collection)
            if (value.IncludeColumns is not null)
            {
                for (int i = 0; i < value.IncludeColumns.Count; i++)
                {
                    if (string.IsNullOrWhiteSpace(value.IncludeColumns[i]))
                    {
                        errors.Add($"IncludeColumns[{i}] must be a non-empty string.");
                    }
                }
            }

            // Validate IndexType
            if (string.IsNullOrWhiteSpace(value.IndexType))
            {
                errors.Add("IndexType must be a non-empty string.");
            }
            else if (value.IndexType.Contains(' ', StringComparison.Ordinal))
            {
                errors.Add("IndexType must not contain spaces.");
            }

            // Validate EstimatedPerformanceGain - must be valid and within range
            if (double.IsNaN(value.EstimatedPerformanceGain) || double.IsInfinity(value.EstimatedPerformanceGain))
            {
                errors.Add("EstimatedPerformanceGain must be a valid number.");
            }
            else if (value.EstimatedPerformanceGain < 0)
            {
                errors.Add("EstimatedPerformanceGain must be non-negative.");
            }

            // Validate EstimatedExecutionTimeReduction - percentage between 0-100
            if (double.IsNaN(value.EstimatedExecutionTimeReduction) || double.IsInfinity(value.EstimatedExecutionTimeReduction))
            {
                errors.Add("EstimatedExecutionTimeReduction must be a valid number.");
            }
            else if (value.EstimatedExecutionTimeReduction is < 0 or > 100)
            {
                errors.Add("EstimatedExecutionTimeReduction must be between 0 and 100.");
            }

            // Validate EstimatedIndexSizeKB - must be positive if specified
            if (value.EstimatedIndexSizeKB.HasValue && value.EstimatedIndexSizeKB <= 0)
            {
                errors.Add("EstimatedIndexSizeKB must be positive if specified.");
            }

            // Validate EstimatedMaintenanceCost - must be non-negative if specified
            if (value.EstimatedMaintenanceCost.HasValue && value.EstimatedMaintenanceCost < 0)
            {
                errors.Add("EstimatedMaintenanceCost must be non-negative if specified.");
            }

            // Validate GeneratedCreateScript
            if (string.IsNullOrWhiteSpace(value.GeneratedCreateScript))
            {
                errors.Add("GeneratedCreateScript must be a non-empty string.");
            }

            // Validate GeneratedDropScript
            if (string.IsNullOrWhiteSpace(value.GeneratedDropScript))
            {
                errors.Add("GeneratedDropScript must be a non-empty string.");
            }

            // Validate AffectedQueries - must be positive
            if (value.AffectedQueries <= 0)
            {
                errors.Add("AffectedQueries must be positive.");
            }

            // Validate SuggestedAt - must be valid and not too far in the future
            if (value.SuggestedAt == default)
            {
                errors.Add("SuggestedAt must be a valid DateTime.");
            }
            else if (value.SuggestedAt > DateTime.UtcNow.AddHours(1))
            {
                errors.Add("SuggestedAt cannot be in the future by more than one hour.");
            }

            // Validate Rationale
            if (string.IsNullOrWhiteSpace(value.Rationale))
            {
                errors.Add("Rationale must be a non-empty string.");
            }

            // Validate ConflictingIndexes (optional collection)
            if (value.ConflictingIndexes is not null)
            {
                for (int i = 0; i < value.ConflictingIndexes.Count; i++)
                {
                    if (string.IsNullOrWhiteSpace(value.ConflictingIndexes[i]))
                    {
                        errors.Add($"ConflictingIndexes[{i}] must be a non-empty string.");
                    }
                }
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the specified <see cref="IndexSuggestion"/> instance is valid.
        /// </summary>
        /// <param name="value">The index suggestion to check.</param>
        /// <returns>True if the instance is valid; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static bool IsValid(this IndexSuggestion value)
        {
            return value.Validate().Count == 0;
        }

        /// <summary>
        /// Ensures that the specified <see cref="IndexSuggestion"/> instance is valid.
        /// </summary>
        /// <param name="value">The index suggestion to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the instance is invalid, containing a list of validation errors.</exception>
        public static void EnsureValid(this IndexSuggestion value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = value.Validate();
            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    $"IndexSuggestion is invalid. Errors:\n{string.Join("\n", errors)}");
            }
        }
    }
}