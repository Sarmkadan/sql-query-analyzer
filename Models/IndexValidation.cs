#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace SqlQueryAnalyzer.Models;

/// <summary>
/// Provides validation helpers for <see cref="Index"/> instances.
/// </summary>
public static class IndexValidation
{
    /// <summary>
    /// Validates the specified <see cref="Index"/> instance.
    /// </summary>
    /// <param name="value">The index to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this Index value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate IndexId
        if (string.IsNullOrWhiteSpace(value.IndexId))
        {
            errors.Add("IndexId must not be null or whitespace.");
        }
        else if (!IsValidGuid(value.IndexId))
        {
            errors.Add("IndexId must be a valid GUID.");
        }

        // Validate IndexName
        if (string.IsNullOrWhiteSpace(value.IndexName))
        {
            errors.Add("IndexName must not be null or whitespace.");
        }
        else if (value.IndexName.Length > 128)
        {
            errors.Add("IndexName must not exceed 128 characters.");
        }
        else if (!IsValidSqlIdentifier(value.IndexName))
        {
            errors.Add("IndexName must be a valid SQL identifier.");
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
        else if (!IsValidSqlIdentifier(value.TableName))
        {
            errors.Add("TableName must be a valid SQL identifier.");
        }

        // Validate SchemaName
        if (string.IsNullOrWhiteSpace(value.SchemaName))
        {
            errors.Add("SchemaName must not be null or whitespace.");
        }
        else if (value.SchemaName.Length > 128)
        {
            errors.Add("SchemaName must not exceed 128 characters.");
        }
        else if (!IsValidSqlIdentifier(value.SchemaName))
        {
            errors.Add("SchemaName must be a valid SQL identifier.");
        }

        // Validate IndexType
        // IndexType is an enum, so it's always valid

        // Validate FileGroup
        if (string.IsNullOrWhiteSpace(value.FileGroup))
        {
            errors.Add("FileGroup must not be null or whitespace.");
        }
        else if (value.FileGroup.Length > 128)
        {
            errors.Add("FileGroup must not exceed 128 characters.");
        }
        else if (!IsValidSqlIdentifier(value.FileGroup))
        {
            errors.Add("FileGroup must be a valid SQL identifier.");
        }

        // Validate FilterPredicate
        if (value.FilterPredicate is not null && value.FilterPredicate.Length > 2048)
        {
            errors.Add("FilterPredicate must not exceed 2048 characters.");
        }

        // Validate Columns collection
        if (value.Columns is null)
        {
            errors.Add("Columns must not be null.");
        }
        else if (value.Columns.Count == 0)
        {
            errors.Add("Columns must contain at least one column.");
        }
        else
        {
            foreach (var column in value.Columns)
            {
                if (column is null)
                {
                    errors.Add("Each Column must not be null.");
                    break;
                }

                if (string.IsNullOrWhiteSpace(column.ColumnName))
                {
                    errors.Add("Each Column.ColumnName must not be null or whitespace.");
                    break;
                }

                if (column.ColumnName.Length > 128)
                {
                    errors.Add("Each Column.ColumnName must not exceed 128 characters.");
                    break;
                }

                if (!IsValidSqlIdentifier(column.ColumnName))
                {
                    errors.Add($"Column.ColumnName '{column.ColumnName}' is not a valid SQL identifier.");
                    break;
                }

                if (column.KeyOrdinal < 0)
                {
                    errors.Add("Each Column.KeyOrdinal must be non-negative.");
                    break;
                }
            }
        }

        // Validate IncludeColumns collection
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

        // Validate SizeInBytes
        if (value.SizeInBytes < 0)
        {
            errors.Add($"SizeInBytes must be non-negative, but was {value.SizeInBytes}.");
        }

        // Validate PageCount
        if (value.PageCount < 0)
        {
            errors.Add($"PageCount must be non-negative, but was {value.PageCount}.");
        }

        // Validate usage statistics
        ValidateNonNegativeLong(errors, nameof(Index.UserSeeks), value.UserSeeks);

        // Validate usage statistics
        ValidateNonNegativeLong(errors, nameof(Index.UserScans), value.UserScans);

        // Validate usage statistics
        ValidateNonNegativeLong(errors, nameof(Index.UserLookups), value.UserLookups);

        // Validate usage statistics
        ValidateNonNegativeLong(errors, nameof(Index.UserUpdates), value.UserUpdates);

        // Validate usage statistics
        ValidateNonNegativeLong(errors, nameof(Index.LastUserSeekTime), value.LastUserSeekTime);

        // Validate LastUserScanTime
        if (value.LastUserScanTime < 0)
        {
            errors.Add($"LastUserScanTime must be non-negative, but was {value.LastUserScanTime}.");
        }

        // Validate usage statistics
        ValidateNonNegativeLong(errors, nameof(Index.LastUserScanTime), value.LastUserScanTime);

        // Validate FragmentationPercentage
        if (value.FragmentationPercentage < 0 || value.FragmentationPercentage > 100)
        {
            errors.Add("FragmentationPercentage must be between 0 and 100 inclusive.");
        }

        // Validate FragmentationCount
        if (value.FragmentCount < 0)
        {
            errors.Add($"FragmentCount must be non-negative, but was {value.FragmentCount}.");
        }

        // Validate CreatedDate
        if (value.CreatedDate == default)
        {
            errors.Add("CreatedDate must be a valid non-default DateTime.");
        }
        else if (value.CreatedDate > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add("CreatedDate must not be in the future by more than 5 minutes.");
        }

        // Validate LastModifiedDate
        if (value.LastModifiedDate.HasValue && value.LastModifiedDate.Value > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add("LastModifiedDate must not be in the future by more than 5 minutes.");
        }

        // Validate LastStatisticsUpdate
        if (value.LastStatisticsUpdate.HasValue && value.LastStatisticsUpdate.Value > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add("LastStatisticsUpdate must not be in the future by more than 5 minutes.");
        }

        // Validate TotalMaintenanceOperations
        if (value.TotalMaintenanceOperations < 0)
        {
            errors.Add($"TotalMaintenanceOperations must be non-negative, but was {value.TotalMaintenanceOperations}.");
        }

        // Validate HealthStatus
        // HealthStatus is an enum, so it's always valid

        // Validate HealthNotes
        if (value.HealthNotes is not null && value.HealthNotes.Length > 1024)
        {
            errors.Add("HealthNotes must not exceed 1024 characters.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="Index"/> is valid.
    /// </summary>
    /// <param name="value">The index to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this Index value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="Index"/> is valid, throwing an <see cref="ArgumentException"/> if not.
    /// </summary>
    /// <param name="value">The index to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the index is invalid.</exception>
    public static void EnsureValid(this Index value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"Index is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }

    /// <summary>
    /// Validates that a long value is non-negative.
    /// </summary>
    /// <param name="errors">The error list to append to.</param>
    /// <param name="propertyName">The name of the property being validated.</param>
    /// <param name="value">The value to validate.</param>
    private static void ValidateNonNegativeLong(List<string> errors, string propertyName, long value)
    {
        if (value < 0)
        {
            errors.Add($"{propertyName} must be non-negative, but was {value}.");
        }
    }

    private static bool IsValidGuid(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        if (value.Length != 36 && value.Length != 32)
        {
            return false;
        }

        return Guid.TryParse(value, out _);
    }

    private static bool IsValidSqlIdentifier(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

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
}