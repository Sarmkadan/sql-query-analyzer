#nullable enable

using System;
using System.Collections.Generic;

namespace SqlQueryAnalyzer.Models;

/// <summary>
/// Provides validation helpers for <see cref="DatabaseQuery"/> instances.
/// </summary>
public static class DatabaseQueryValidation
{
    /// <summary>
    /// Validates a <see cref="DatabaseQuery"/> instance and returns a list of human-readable validation problems.
    /// </summary>
    /// <param name="value">The query to validate.</param>
    /// <returns>A read-only list of validation problems; empty if the query is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this DatabaseQuery value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate QueryId
        if (string.IsNullOrWhiteSpace(value.QueryId))
        {
            problems.Add("QueryId cannot be null or whitespace.");
        }
        else if (Guid.TryParse(value.QueryId, out var queryIdGuid) && queryIdGuid == Guid.Empty)
        {
            problems.Add("QueryId should not be an empty GUID.");
        }

        // Validate QueryText
        if (string.IsNullOrWhiteSpace(value.QueryText))
        {
            problems.Add("QueryText cannot be null or whitespace.");
        }

        // Validate QueryType
        if (value.QueryType is QueryType.Unknown)
        {
            problems.Add("QueryType must be set to a valid value (cannot be Unknown).");
        }

        // Validate DatabaseType
        if (value.DatabaseType is DatabaseType.Unknown)
        {
            problems.Add("DatabaseType must be set to a valid value (cannot be Unknown).");
        }

        // Validate SchemaName
        if (string.IsNullOrWhiteSpace(value.SchemaName))
        {
            problems.Add("SchemaName cannot be null or whitespace.");
        }
        else if (value.SchemaName.Contains(' '))
        {
            problems.Add("SchemaName cannot contain spaces.");
        }

        // Validate CreatedBy
        if (string.IsNullOrWhiteSpace(value.CreatedBy))
        {
            problems.Add("CreatedBy cannot be null or whitespace.");
        }

        // Validate CreatedDate
        if (value.CreatedDate == default)
        {
            problems.Add("CreatedDate must be set to a valid date (cannot be default(DateTime)).");
        }
        else if (value.CreatedDate.Kind != DateTimeKind.Utc)
        {
            problems.Add("CreatedDate should be in UTC format.");
        }

        // Validate optional ModifiedBy
        if (string.IsNullOrWhiteSpace(value.ModifiedBy) && value.ModifiedDate.HasValue)
        {
            problems.Add("ModifiedBy must be set when ModifiedDate is set.");
        }

        // Validate ModifiedDate
        if (value.ModifiedDate.HasValue && value.ModifiedDate.Value == default)
        {
            problems.Add("ModifiedDate must be a valid date when set (cannot be default(DateTime)).");
        }
        else if (value.ModifiedDate.HasValue && value.ModifiedDate.Value.Kind != DateTimeKind.Utc)
        {
            problems.Add("ModifiedDate should be in UTC format when set.");
        }

        // Validate ReferencedTables
        if (value.ReferencedTables is null)
        {
            problems.Add("ReferencedTables cannot be null.");
        }
        else if (value.ReferencedTables.Count == 0)
        {
            problems.Add("ReferencedTables should contain at least one table reference.");
        }
        else
        {
            foreach (var table in value.ReferencedTables)
            {
                if (string.IsNullOrWhiteSpace(table))
                {
                    problems.Add("ReferencedTables contains null or whitespace entries.");
                    break;
                }
            }
        }

        // Validate ReferencedColumns
        if (value.ReferencedColumns is null)
        {
            problems.Add("ReferencedColumns cannot be null.");
        }
        else
        {
            foreach (var column in value.ReferencedColumns)
            {
                if (string.IsNullOrWhiteSpace(column))
                {
                    problems.Add("ReferencedColumns contains null or whitespace entries.");
                    break;
                }
            }
        }

        // Validate JoinConditions
        if (value.JoinConditions is null)
        {
            problems.Add("JoinConditions cannot be null.");
        }
        else
        {
            foreach (var condition in value.JoinConditions)
            {
                if (string.IsNullOrWhiteSpace(condition))
                {
                    problems.Add("JoinConditions contains null or whitespace entries.");
                    break;
                }
            }
        }

        // Validate WhereConditions
        if (value.WhereConditions is null)
        {
            problems.Add("WhereConditions cannot be null.");
        }
        else
        {
            foreach (var condition in value.WhereConditions)
            {
                if (string.IsNullOrWhiteSpace(condition))
                {
                    problems.Add("WhereConditions contains null or whitespace entries.");
                    break;
                }
            }
        }

        // Validate Parameters
        if (value.Parameters is null)
        {
            problems.Add("Parameters dictionary cannot be null.");
        }
        else
        {
            foreach (var kvp in value.Parameters)
            {
                if (kvp.Key is null)
                {
                    problems.Add("Parameters contains null keys.");
                    break;
                }

                if (string.IsNullOrWhiteSpace(kvp.Key))
                {
                    problems.Add("Parameters contains empty or whitespace keys.");
                    break;
                }

                if (kvp.Value is null)
                {
                    problems.Add($"Parameters['{kvp.Key}'] cannot be null.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(kvp.Value.ParameterName))
                {
                    problems.Add($"ParameterInfo for '{kvp.Key}' has null or whitespace ParameterName.");
                }

                if (string.IsNullOrWhiteSpace(kvp.Value.DataType))
                {
                    problems.Add($"ParameterInfo for '{kvp.Key}' has null or whitespace DataType.");
                }
            }
        }

        // Validate VariableDeclarations
        if (value.VariableDeclarations is null)
        {
            problems.Add("VariableDeclarations dictionary cannot be null.");
        }
        else
        {
            foreach (var kvp in value.VariableDeclarations)
            {
                if (kvp.Key is null)
                {
                    problems.Add("VariableDeclarations contains null keys.");
                    break;
                }

                if (string.IsNullOrWhiteSpace(kvp.Key))
                {
                    problems.Add("VariableDeclarations contains empty or whitespace keys.");
                    break;
                }

                if (kvp.Value is null)
                {
                    problems.Add($"VariableDeclarations['{kvp.Key}'] cannot be null.");
                }
                else if (string.IsNullOrWhiteSpace(kvp.Value))
                {
                    problems.Add($"VariableDeclarations['{kvp.Key}'] cannot be empty or whitespace.");
                }
            }
        }

        // Validate LineCount
        if (value.LineCount < 0)
        {
            problems.Add("LineCount cannot be negative.");
        }
        else if (value.LineCount == 0 && !string.IsNullOrWhiteSpace(value.QueryText))
        {
            problems.Add("LineCount should reflect the actual number of lines in QueryText.");
        }

        // Validate optional fields
        if (!string.IsNullOrWhiteSpace(value.ProcedureName) && string.IsNullOrWhiteSpace(value.QueryText))
        {
            problems.Add("QueryText must be set when ProcedureName is specified.");
        }

        if (!string.IsNullOrWhiteSpace(value.ModuleName) && string.IsNullOrWhiteSpace(value.QueryText))
        {
            problems.Add("QueryText must be set when ModuleName is specified.");
        }

        if (!string.IsNullOrWhiteSpace(value.ApplicationName) && string.IsNullOrWhiteSpace(value.QueryText))
        {
            problems.Add("QueryText must be set when ApplicationName is specified.");
        }

        if (!string.IsNullOrWhiteSpace(value.DatabaseName) && string.IsNullOrWhiteSpace(value.QueryText))
        {
            problems.Add("QueryText must be set when DatabaseName is specified.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="DatabaseQuery"/> is valid.
    /// </summary>
    /// <param name="value">The query to check.</param>
    /// <returns><see langword="true"/> if the query is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool IsValid(this DatabaseQuery value) => value.Validate().Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="DatabaseQuery"/> is valid, throwing an <see cref="ArgumentException"/> if it is not.
    /// </summary>
    /// <param name="value">The query to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when the query is invalid, containing a list of validation problems.</exception>
    public static void EnsureValid(this DatabaseQuery value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"DatabaseQuery is invalid. Problems:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}",
                nameof(value));
        }
    }
}