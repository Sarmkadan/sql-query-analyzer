#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlQueryAnalyzer.Models;

/// <summary>
/// Provides extension methods for the <see cref="DatabaseQuery"/> class.
/// </summary>
public static class DatabaseQueryExtensions
{
    /// <summary>
    /// Determines whether the specified table is referenced by the query.
    /// </summary>
    /// <param name="query">The <see cref="DatabaseQuery"/> instance.</param>
    /// <param name="tableName">The name of the table to check.</param>
    /// <returns>True if the table is referenced, otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="query"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="tableName"/> is null or empty.</exception>
    public static bool IsTableReferenced(this DatabaseQuery query, string tableName)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentException.ThrowIfNullOrEmpty(tableName);

        return query.ReferencedTables.Contains(tableName, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Retrieves a read-only list of parameter names used in the query.
    /// </summary>
    /// <param name="query">The <see cref="DatabaseQuery"/> instance.</param>
    /// <returns>An <see cref="IReadOnlyList{T}"/> containing parameter names.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="query"/> is null.</exception>
    public static IReadOnlyList<string> GetParameterNames(this DatabaseQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        return query.Parameters.Keys.ToList().AsReadOnly();
    }

    /// <summary>
    /// Determines whether the query is a DDL operation (CREATE or DROP).
    /// </summary>
    /// <param name="query">The <see cref="DatabaseQuery"/> instance.</param>
    /// <returns>True if the query is a DDL operation, otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="query"/> is null.</exception>
    public static bool IsDdl(this DatabaseQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        return query.QueryType is QueryType.Create or QueryType.Drop;
    }
}
