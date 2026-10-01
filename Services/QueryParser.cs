#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using SqlQueryAnalyzer.Models;

namespace SqlQueryAnalyzer.Services;

/// <summary>
/// Parses raw SQL text into structured query components.
/// Extracts statement type, referenced tables, columns, joins, and subqueries.
/// </summary>
public sealed class QueryParser
{
    private readonly ILogger<QueryParser> _logger;

    private static readonly Regex TablePattern = new(
        @"\b(?:FROM|JOIN|INTO|UPDATE)\s+([[\w]+\.]*[\w]+)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex AliasPattern = new(
        @"\b(?:FROM|JOIN)\s+([\w.]+)\s+(?:AS\s+)?([\w]+)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>
    /// Initializes a new instance of <see cref="QueryParser"/>.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    public QueryParser(ILogger<QueryParser> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Determines the SQL statement type (SELECT, INSERT, UPDATE, DELETE, etc.).
    /// </summary>
    /// <param name="sql">The SQL query to classify.</param>
    /// <returns>The detected statement type.</returns>
    public string DetectStatementType(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
            return "UNKNOWN";

        var trimmed = sql.TrimStart();
        var firstWord = trimmed.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.ToUpperInvariant();

        return firstWord switch
        {
            "SELECT" => "SELECT",
            "INSERT" => "INSERT",
            "UPDATE" => "UPDATE",
            "DELETE" => "DELETE",
            "CREATE" => "DDL",
            "ALTER" => "DDL",
            "DROP" => "DDL",
            "WITH" => "SELECT", // CTE
            "EXEC" or "EXECUTE" => "PROCEDURE_CALL",
            "MERGE" => "MERGE",
            _ => "UNKNOWN"
        };
    }

    /// <summary>
    /// Extracts all table names referenced in the query.
    /// </summary>
    /// <param name="sql">The SQL query to analyze.</param>
    /// <returns>Distinct list of table names.</returns>
    public List<string> ExtractTableNames(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
            return new List<string>();

        _logger.LogDebug("Extracting table names from query");

        var matches = TablePattern.Matches(sql);
        var tables = matches
            .Select(m => m.Groups[1].Value.Trim('[', ']'))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        _logger.LogDebug("Found {Count} table(s): {Tables}", tables.Count, string.Join(", ", tables));
        return tables;
    }

    /// <summary>
    /// Extracts table aliases from the query.
    /// </summary>
    /// <param name="sql">The SQL query.</param>
    /// <returns>Dictionary mapping alias to table name.</returns>
    public Dictionary<string, string> ExtractAliases(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
            return new Dictionary<string, string>();

        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var matches = AliasPattern.Matches(sql);

        foreach (Match m in matches)
        {
            var table = m.Groups[1].Value;
            var alias = m.Groups[2].Value;

            if (!string.Equals(alias, "ON", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(alias, "WHERE", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(alias, "SET", StringComparison.OrdinalIgnoreCase))
            {
                result.TryAdd(alias, table);
            }
        }

        return result;
    }

    /// <summary>
    /// Counts the number of JOIN clauses in the query.
    /// </summary>
    /// <param name="sql">The SQL query.</param>
    /// <returns>Total number of joins.</returns>
    public int CountJoins(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
            return 0;

        var count = Regex.Matches(sql, @"\bJOIN\b", RegexOptions.IgnoreCase).Count;
        _logger.LogDebug("Detected {Count} JOIN clause(s)", count);
        return count;
    }

    /// <summary>
    /// Checks whether the query contains subqueries (nested SELECT statements).
    /// </summary>
    /// <param name="sql">The SQL query.</param>
    /// <returns>True if subqueries are present.</returns>
    public bool HasSubqueries(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
            return false;

        // A subquery is a SELECT inside parentheses, not the first SELECT
        var match = Regex.IsMatch(sql, @"\(\s*SELECT\b", RegexOptions.IgnoreCase);
        return match;
    }
}
