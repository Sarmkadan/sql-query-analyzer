#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;

namespace SqlQueryAnalyzer.Exceptions;

/// <summary>
/// Exception for SQL queries that exceed the maximum allowed length.
/// </summary>
public class QueryTooLargeException : AnalysisException
{
    /// <summary>
    /// Gets the query that exceeded the maximum allowed length.
    /// </summary>
    public string Query { get; } = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryTooLargeException"/> class with a specified error message.
    /// </param>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public QueryTooLargeException(string message) : base(message, "QUERY_TOO_LARGE") { }

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryTooLargeException"/> class with a specified error message and the query that caused the exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="query">The query string that exceeded the maximum allowed length.</param>
    public QueryTooLargeException(string message, string query) : base(message, "QUERY_TOO_LARGE")
    {
        Query = query;
    }
}