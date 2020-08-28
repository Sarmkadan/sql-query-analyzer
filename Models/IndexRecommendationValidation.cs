#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace SqlQueryAnalyzer.Models;

/// <summary>
/// Provides validation helpers for <see cref="IndexRecommendation"/> instances.
/// </summary>
public static class IndexRecommendationValidation
{
    /// <summary>
    /// Validates the specified <see cref="IndexRecommendation"/> instance.
    /// </summary>
    /// <param name="value">The index recommendation to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this IndexRecommendation value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate RecommendationId
        if (string.IsNullOrWhiteSpace(value.RecommendationId))
        {
            errors.Add("RecommendationId must not be null or whitespace.");
        }
        else if (!IsValidGuid(value.RecommendationId))
        {
            errors.Add("RecommendationId must be a valid GUID.");
        }

        // Validate TableName
        if (string.IsNullOrWhiteSpace(value.TableName))
        {
            errors.Add("TableName must not be null or whitespace.");
        }
        else if (value.TableName.Length > 128)
        {
            errors.Add("TableName must not exceed 128 characters.");
        }

        // Validate KeyColumns
        if (value.KeyColumns is null)
        {
            errors.Add("KeyColumns must not be null.");
        }
        else if (value.KeyColumns.Count == 0)
        {
            errors.Add("KeyColumns must contain at least one column.");
        }
        else
        {
            foreach (var column in value.KeyColumns)
            {
                if (string.IsNullOrWhiteSpace(column))
                {
                    errors.Add("Each KeyColumn must not be null or whitespace.");
                    break;
                }

                if (column.Length > 128)
                {
                    errors.Add("Each KeyColumn must not exceed 128 characters.");
                    break;
                }

                if (!IsValidSqlIdentifier(column))
                {
                    errors.Add($"KeyColumn '{column}' is not a valid SQL identifier.");
                    break;
                }
            }
        }

        // Validate IncludeColumns
        if (value.IncludeColumns is not null)
        {
            foreach (var column in value.IncludeColumns)
            {
                if (string.IsNullOrWhiteSpace(column))
                {
                    errors.Add("Each IncludeColumn must not be null or whitespace.");
                    break;
                }

                if (column.Length > 128)
                {
                    errors.Add("Each IncludeColumn must not exceed 128 characters.");
                    break;
                }

                if (!IsValidSqlIdentifier(column))
                {
                    errors.Add($"IncludeColumn '{column}' is not a valid SQL identifier.");
                    break;
                }
            }
        }

        // Validate IndexType
        if (string.IsNullOrWhiteSpace(value.IndexType))
        {
            errors.Add("IndexType must not be null or whitespace.");
        }
        else if (value.IndexType.Length > 32)
        {
            errors.Add("IndexType must not exceed 32 characters.");
        }
        else if (!IsValidIndexType(value.IndexType))
        {
            errors.Add("IndexType must be either 'CLUSTERED' or 'NONCLUSTERED'.");
        }

        // Validate ImpactScore
        if (value.ImpactScore < 0 || value.ImpactScore > 100)
        {
            errors.Add("ImpactScore must be between 0 and 100 inclusive.");
        }

        // Validate Rationale
        if (string.IsNullOrWhiteSpace(value.Rationale))
        {
            errors.Add("Rationale must not be null or whitespace.");
        }
        else if (value.Rationale.Length > 2048)
        {
            errors.Add("Rationale must not exceed 2048 characters.");
        }

        // Validate GeneratedScript
        if (string.IsNullOrWhiteSpace(value.GeneratedScript))
        {
            errors.Add("GeneratedScript must not be null or whitespace.");
        }
        else if (value.GeneratedScript.Length > 8192)
        {
            errors.Add("GeneratedScript must not exceed 8192 characters.");
        }

        // Validate Source
        // Source is an enum, so it's always valid

        // Validate RecommendedAt
        if (value.RecommendedAt == default)
        {
            errors.Add("RecommendedAt must not be the default DateTime value.");
        }
        else if (value.RecommendedAt > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add("RecommendedAt must not be in the future by more than 5 minutes.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="IndexRecommendation"/> is valid.
    /// </summary>
    /// <param name="value">The index recommendation to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this IndexRecommendation value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="IndexRecommendation"/> is valid, throwing an <see cref="ArgumentException"/> if not.
    /// </summary>
    /// <param name="value">The index recommendation to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the recommendation is invalid.</exception>
    public static void EnsureValid(this IndexRecommendation value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"IndexRecommendation is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }

    private static bool IsValidGuid(string value)
    {
        if (value.Length != 36 && value.Length != 32)
        {
            return false;
        }

        return Guid.TryParse(value, out _);
    }

    private static bool IsValidSqlIdentifier(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        // SQL identifiers can contain letters, digits, underscores, and dollar signs
        // Cannot start with a digit
        if (char.IsDigit(value[0]))
        {
            return false;
        }

        foreach (var c in value)
        {
            if (!char.IsLetterOrDigit(c) && c != '_' && c != '$')
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsValidIndexType(string value)
    {
        return string.Equals(value, "CLUSTERED", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(value, "NONCLUSTERED", StringComparison.OrdinalIgnoreCase);
    }
}