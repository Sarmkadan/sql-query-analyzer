#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SqlQueryAnalyzer.Exceptions;

namespace SqlQueryAnalyzer.Models;

/// <summary>
/// Represents a database query with metadata and lineage.
/// </summary>
public sealed class DatabaseQuery
{
    /// <summary>
    /// Maximum allowed query length in characters (100 KB).
    /// </summary>
    private const int MaxQueryLength = 100 * 1024;

    /// <summary>
    /// Gets or sets the unique identifier for the query.
    /// </summary>
    public string QueryId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the raw SQL query text.
    /// </summary>
    public string QueryText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the stored procedure, if applicable.
    /// </summary>
    public string? ProcedureName { get; set; }

    /// <summary>
    /// Gets or sets the module name.
    /// </summary>
    public string? ModuleName { get; set; }

    /// <summary>
    /// Gets or sets the application name.
    /// </summary>
    public string? ApplicationName { get; set; }

    /// <summary>
    /// Gets or sets the database name.
    /// </summary>
    public string? DatabaseName { get; set; }

    // Classification
    /// <summary>
    /// Gets or sets the query type.
    /// </summary>
    public QueryType QueryType { get; set; } = QueryType.Unknown;
    /// <summary>
    /// Gets or sets the database type.
    /// </summary>
    public DatabaseType DatabaseType { get; set; } = DatabaseType.SqlServer;
    /// <summary>
    /// Gets or sets the schema name.
    /// </summary>
    public string SchemaName { get; set; } = "dbo";

    // Metadata
    /// <summary>
    /// Gets or sets the created by.
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the created date.
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    /// <summary>
    /// Gets or sets the modified by.
    /// </summary>
    public string? ModifiedBy { get; set; }
    /// <summary>
    /// Gets or sets the modified date.
    /// </summary>
    public DateTime? ModifiedDate { get; set; }

    // Query analysis
    /// <summary>
    /// Gets or sets the referenced tables.
    /// </summary>
    public List<string> ReferencedTables { get; set; } = [];
    /// <summary>
    /// Gets or sets the referenced columns.
    /// </summary>
    public List<string> ReferencedColumns { get; set; } = [];
    /// <summary>
    /// Gets or sets the join conditions.
    /// </summary>
    public List<string> JoinConditions { get; set; } = [];
    /// <summary>
    /// Gets or sets the where conditions.
    /// </summary>
    public List<string> WhereConditions { get; set; } = [];

    // Parameters and variables
    /// <summary>
    /// Gets or sets the parameters.
    /// </summary>
    public Dictionary<string, ParameterInfo> Parameters { get; set; } = [];
    /// <summary>
    /// Gets or sets the variable declarations.
    /// </summary>
    public Dictionary<string, string> VariableDeclarations { get; set; } = [];

    // Query complexity
    /// <summary>
    /// Gets or sets the line count.
    /// </summary>
    public int LineCount { get; set; }
    /// <summary>
    /// Gets or sets the statement count.
    /// </summary>
    public int StatementCount { get; set; }
    /// <summary>
    /// Gets or sets the cyclomatic complexity.
    /// </summary>
    public double CyclomaticComplexity { get; set; }

    // Execution context
    /// <summary>
    /// Gets or sets the source file.
    /// </summary>
    public string? SourceFile { get; set; }
    /// <summary>
    /// Gets or sets the source line number.
    /// </summary>
    public int? SourceLineNumber { get; set; }
    /// <summary>
    /// Gets or sets the calling method.
    /// </summary>
    public string? CallingMethod { get; set; }
    /// <summary>
    /// Gets or sets the environment.
    /// </summary>
    public string? Environment { get; set; } // Development, Staging, Production

    // Hash for deduplication
    /// <summary>
    /// Gets or sets the query hash.
    /// </summary>
    public string QueryHash { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the normalized query.
    /// </summary>
    public string NormalizedQuery { get; set; } = string.Empty;

    // Validate query
    /// <summary>
    /// Is valid.
    /// </summary>
    public bool IsValid() =>
        !string.IsNullOrWhiteSpace(QueryText) &&
        QueryType != QueryType.Unknown &&
        ReferencedTables.Count > 0;

    // Parse query text and extract basic information
    /// <summary>
    /// Parses the query text and extracts metadata including query type, referenced tables,
    /// join conditions, and WHERE clauses.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if QueryText is null or empty.</exception>
    /// <exception cref="QueryTooLargeException">Thrown if QueryText exceeds the maximum allowed length of 100 KB.</exception>
    public void Parse()
    {
        ArgumentException.ThrowIfNullOrEmpty(QueryText);

        // Validate query length to prevent resource exhaustion from excessive regex processing
        if (QueryText.Length > MaxQueryLength)
        {
            throw new QueryTooLargeException(
                $"Query text exceeds maximum allowed length of {MaxQueryLength} characters.",
                QueryText);
        }

        // Normalize for analysis - remove comments and mask string literals before regex matching
        NormalizedQuery = NormalizeQuery(QueryText);

        // Count statements
        StatementCount = QueryText.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries).Length;

        // Count lines
        LineCount = QueryText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).Length;

        // Detect query type
        DetectQueryType();

        // Extract tables
        ExtractTables();

        // Extract joins
        ExtractJoins();

        // Extract WHERE conditions
        ExtractWhere();
    }

    private void DetectQueryType()
    {
        var upperQuery = NormalizedQuery.ToUpperInvariant();

        if (upperQuery.StartsWith("SELECT"))
            QueryType = QueryType.Select;
        else if (upperQuery.StartsWith("INSERT"))
            QueryType = QueryType.Insert;
        else if (upperQuery.StartsWith("UPDATE"))
            QueryType = QueryType.Update;
        else if (upperQuery.StartsWith("DELETE"))
            QueryType = QueryType.Delete;
        else if (upperQuery.StartsWith("CREATE"))
            QueryType = QueryType.Create;
        else if (upperQuery.StartsWith("DROP"))
            QueryType = QueryType.Drop;
        else if (upperQuery.StartsWith("DECLARE") || upperQuery.StartsWith("EXEC"))
            QueryType = QueryType.Procedure;
    }

    private void ExtractWhere()
    {
        // Use timeout to prevent catastrophic backtracking
        // Note: RegexOptions.NonBacktracking cannot be used with lookaheads, so we use timeout only
        var wherePattern = @"WHERE\s+(.+?)(?=GROUP\s+BY|ORDER\s+BY|UNION\s+(ALL\s+)?|LIMIT|OFFSET|;|$)";
        try
        {
            var whereRegex = new Regex(wherePattern, RegexOptions.IgnoreCase | RegexOptions.Singleline, TimeSpan.FromSeconds(1));
            var whereMatch = whereRegex.Match(NormalizedQuery);

            if (whereMatch.Success && whereMatch.Groups.Count > 1)
            {
                var whereClause = whereMatch.Groups[1].Value.Trim();
                if (!string.IsNullOrWhiteSpace(whereClause))
                {
                    WhereConditions.Add(whereClause);
                }
            }
        }
        catch (RegexMatchTimeoutException)
        {
            // Query is too complex to analyze - skip WHERE extraction
        }
        catch (ArgumentOutOfRangeException)
        {
            // Invalid regex pattern or other error - skip WHERE extraction
        }
    }

    private void ExtractTables()
    {
        // Extract CTE alias names first — they are virtual and must not be counted
        // as physical table references, which would cause false-positive N+1 detection.
        var ctePattern = @"\bWITH\s+(\w+)\s+AS\s*\(";
        try
        {
            var cteRegex = new Regex(ctePattern, RegexOptions.IgnoreCase | RegexOptions.NonBacktracking, TimeSpan.FromSeconds(1));
            var cteNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (Match cteMatch in cteRegex.Matches(NormalizedQuery))
            {
                cteNames.Add(cteMatch.Groups[1].Value);
            }

            // Simple extraction - in real scenario would use proper SQL parser
            var pattern = @"FROM\s+(\w+)|JOIN\s+(\w+)|INTO\s+(\w+)|UPDATE\s+(\w+)";
            var regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.NonBacktracking, TimeSpan.FromSeconds(1));

            var matches = regex.Matches(NormalizedQuery);
            var seenTables = new HashSet<string>();

            foreach (Match match in matches)
            {
                // Group.Value is never null (it is "" for a non-participating group),
                // so a ?? chain would always stop at Groups[1] and silently drop
                // tables captured by the JOIN/INTO/UPDATE alternatives. Pick the
                // group that actually matched instead.
                var table = match.Groups[1].Success ? match.Groups[1].Value
                    : match.Groups[2].Success ? match.Groups[2].Value
                    : match.Groups[3].Success ? match.Groups[3].Value
                    : match.Groups[4].Value;
                if (!string.IsNullOrWhiteSpace(table) && !cteNames.Contains(table) && seenTables.Add(table))
                    ReferencedTables.Add(table);
            }
        }
        catch (RegexMatchTimeoutException)
        {
            // Query is too complex to analyze - skip table extraction
        }
        catch (ArgumentOutOfRangeException)
        {
            // Invalid regex pattern or other error - skip table extraction
        }
    }

    private void ExtractJoins()
    {
        var pattern = @"(INNER\s+|LEFT\s+|RIGHT\s+|FULL\s+)?\s*JOIN\s+(.+?)\s+ON\s+(.+?)(?=WHERE|GROUP|ORDER|JOIN|$)";
        try
        {
            // Timeout used to prevent catastrophic backtracking; lookahead prevents NonBacktracking
            var regex = new Regex(pattern, RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));

            var matches = regex.Matches(NormalizedQuery);
            foreach (Match match in matches)
            {
                JoinConditions.Add(match.Groups[3].Value.Trim());
            }
        }
        catch (RegexMatchTimeoutException)
        {
            // Query is too complex to analyze - skip join extraction
        }
        catch (ArgumentOutOfRangeException)
        {
            // Invalid regex pattern or other error - skip join extraction
        }
    }

    private string NormalizeQuery(string query)
    {
        // Remove comments
        // Use NonBacktracking and timeout
        var withoutComments = Regex.Replace(query,
            @"--[^\n]*|/\*[\s\S]*?\*/",
            " ",
            RegexOptions.Multiline | RegexOptions.NonBacktracking,
            TimeSpan.FromSeconds(1));

        // Remove extra whitespace
        // Simple regex, but adding timeout for safety
        var normalized = Regex.Replace(withoutComments,
            @"\s+",
            " ",
            RegexOptions.None,
            TimeSpan.FromSeconds(1));

        return normalized.Trim();
    }

    // Get query summary
    /// <summary>
    /// Get summary.
    /// </summary>
    public string GetSummary() =>
        $"{QueryType} | Tables: {string.Join(", ", ReferencedTables)} | " +
        $"Lines: {LineCount} | Complexity: {CyclomaticComplexity:F2}";

    /// <summary>
    /// Returns a concise, human-readable summary of the query's key properties.
    /// </summary>
    public override string ToString() =>
        $"DatabaseQuery {QueryId} | {DatabaseType} {QueryType} | " +
        $"Database: {DatabaseName ?? "-"} | Tables: {ReferencedTables.Count} | Lines: {LineCount}";

    // Generate query hash for deduplication
    /// <summary>
    /// Generate hash.
    /// </summary>
    public string GenerateHash()
    {
        using (var sha = System.Security.Cryptography.SHA256.Create())
        {
            var hash = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(NormalizedQuery));
            QueryHash = Convert.ToBase64String(hash);
        }
        return QueryHash;
    }
}

/// <summary>
/// Represents a query parameter
/// </summary>
public sealed class ParameterInfo
{
    /// <summary>
    /// Gets or sets the parameter name.
    /// </summary>
    public string ParameterName { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the data type.
    /// </summary>
    public string DataType { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the default value.
    /// </summary>
    public string? DefaultValue { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether is output.
    /// </summary>
    public bool IsOutput { get; set; }
    /// <summary>
    /// Gets or sets the max length.
    /// </summary>
    public int? MaxLength { get; set; }
}

/// <summary>
/// Query type classification
/// </summary>
public enum QueryType
{
    /// <summary>
    /// The unknown.
    /// </summary>
    Unknown = 0,
    /// <summary>
    /// The select.
    /// </summary>
    Select = 1,
    /// <summary>
    /// The insert.
    /// </summary>
    Insert = 2,
    /// <summary>
    /// The update.
    /// </summary>
    Update = 3,
    /// <summary>
    /// The delete.
    /// </summary>
    Delete = 4,
    /// <summary>
    /// The create.
    /// </summary>
    Create = 5,
    /// <summary>
    /// The drop.
    /// </summary>
    Drop = 6,
    /// <summary>
    /// The procedure.
    /// </summary>
    Procedure = 7,
    /// <summary>
    /// The function.
    /// </summary>
    Function = 8
}

/// <summary>
/// Supported database types
/// </summary>
public enum DatabaseType
{
    /// <summary>
    /// The unknown.
    /// </summary>
    Unknown = 0,
    /// <summary>
    /// The SQL server.
    /// </summary>
    SqlServer = 1,
    /// <summary>
    /// The postgre SQL.
    /// </summary>
    PostgreSQL = 2,
    /// <summary>
    /// The my SQL.
    /// </summary>
    MySQL = 3,
    /// <summary>
    /// The oracle.
    /// </summary>
    Oracle = 4,
    /// <summary>
    /// The SQ lite.
    /// </summary>
    SQLite = 5
}
