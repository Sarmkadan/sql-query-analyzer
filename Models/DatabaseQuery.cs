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
    public QueryType QueryType { get; set; } = QueryType.Unknown;
    public DatabaseType DatabaseType { get; set; } = DatabaseType.SqlServer;
    public string SchemaName { get; set; } = "dbo";

    // Metadata
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }

    // Query analysis
    public List<string> ReferencedTables { get; set; } = [];
    public List<string> ReferencedColumns { get; set; } = [];
    public List<string> JoinConditions { get; set; } = [];
    public List<string> WhereConditions { get; set; } = [];

    // Parameters and variables
    public Dictionary<string, ParameterInfo> Parameters { get; set; } = [];
    public Dictionary<string, string> VariableDeclarations { get; set; } = [];

    // Query complexity
    public int LineCount { get; set; }
    public int StatementCount { get; set; }
    public double CyclomaticComplexity { get; set; }

    // Execution context
    public string? SourceFile { get; set; }
    public int? SourceLineNumber { get; set; }
    public string? CallingMethod { get; set; }
    public string? Environment { get; set; } // Development, Staging, Production

    // Hash for deduplication
    public string QueryHash { get; set; } = string.Empty;
    public string NormalizedQuery { get; set; } = string.Empty;

    // Validate query
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
    public string GetSummary() =>
        $"{QueryType} | Tables: {string.Join(", ", ReferencedTables)} | " +
        $"Lines: {LineCount} | Complexity: {CyclomaticComplexity:F2}";

    // Generate query hash for deduplication
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
    public string ParameterName { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public string? DefaultValue { get; set; }
    public bool IsOutput { get; set; }
    public int? MaxLength { get; set; }
}

/// <summary>
/// Query type classification
/// </summary>
public enum QueryType
{
    Unknown = 0,
    Select = 1,
    Insert = 2,
    Update = 3,
    Delete = 4,
    Create = 5,
    Drop = 6,
    Procedure = 7,
    Function = 8
}

/// <summary>
/// Supported database types
/// </summary>
public enum DatabaseType
{
    Unknown = 0,
    SqlServer = 1,
    PostgreSQL = 2,
    MySQL = 3,
    Oracle = 4,
    SQLite = 5
}
