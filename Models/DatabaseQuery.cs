#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

namespace SqlQueryAnalyzer.Models;

/// <summary>
/// Represents a database query with metadata and lineage.
/// </summary>
public sealed class DatabaseQuery
{
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
    public void Parse()
    {
        // Normalize for analysis
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

    private void ExtractTables()
    {
        // Extract CTE alias names first — they are virtual and must not be counted
        // as physical table references, which would cause false-positive N+1 detection.
        var ctePattern = @"\bWITH\s+(\w+)\s+AS\s*\(";
        var cteRegex = new System.Text.RegularExpressions.Regex(ctePattern,
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        var cteNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (System.Text.RegularExpressions.Match cteMatch in cteRegex.Matches(NormalizedQuery))
            cteNames.Add(cteMatch.Groups[1].Value);

        // Simple extraction - in real scenario would use proper SQL parser
        var pattern = @"FROM\s+(\w+)|JOIN\s+(\w+)|INTO\s+(\w+)|UPDATE\s+(\w+)";
        var regex = new System.Text.RegularExpressions.Regex(pattern,
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        var matches = regex.Matches(NormalizedQuery);
        var seenTables = new HashSet<string>();

        foreach (System.Text.RegularExpressions.Match match in matches)
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

    private void ExtractJoins()
    {
        var pattern = @"(INNER\s+|LEFT\s+|RIGHT\s+|FULL\s+)?\s*JOIN\s+(.+?)\s+ON\s+(.+?)(?=WHERE|GROUP|ORDER|JOIN|$)";
        var regex = new System.Text.RegularExpressions.Regex(pattern,
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        var matches = regex.Matches(NormalizedQuery);
        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            JoinConditions.Add(match.Groups[3].Value.Trim());
        }
    }

    private string NormalizeQuery(string query)
    {
        // Remove comments
        var withoutComments = System.Text.RegularExpressions.Regex.Replace(query,
            @"--[^\n]*|/\*[\s\S]*?\*/", " ");

        // Remove extra whitespace
        var normalized = System.Text.RegularExpressions.Regex.Replace(withoutComments,
            @"\s+", " ");

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
            QueryHash = System.Convert.ToBase64String(hash);
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
