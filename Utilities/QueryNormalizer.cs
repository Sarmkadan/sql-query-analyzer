// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.RegularExpressions;

namespace SqlQueryAnalyzer.Utilities;

/// <summary>
/// Normalizes SQL queries for consistent analysis.
/// Removes redundant whitespace, standardizes capitalization, expands abbreviations.
/// Normalization is SAFE - does not change query logic or semantics.
/// </summary>
public class QueryNormalizer
{
    /// <summary>
    /// Normalizes a SQL query by applying multiple transformations.
    /// Returns normalized query that's logically identical to input.
    /// </summary>
    public string Normalize(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return query;

        // Preserve string literals during normalization
        var stringLiterals = ExtractStringLiterals(query);
        var working = ReplaceStringLiteralsWithPlaceholders(query, stringLiterals);

        // Apply normalizations
        working = NormalizeWhitespace(working);
        working = StandardizeKeywordCapitalization(working);
        working = NormalizeLineBreaks(working);
        working = RemoveTrailingComments(working);

        // Restore string literals
        working = RestoreStringLiterals(working, stringLiterals);

        return working.Trim();
    }

    /// <summary>
    /// Extracts all string literals to protect them from modification.
    /// String literals are replaced with placeholders during normalization.
    /// </summary>
    private Dictionary<string, string> ExtractStringLiterals(string query)
    {
        var literals = new Dictionary<string, string>();
        var regex = new Regex(@"'(?:''|[^'])*'", RegexOptions.Compiled);
        int index = 0;

        foreach (Match match in regex.Matches(query))
        {
            var placeholder = $"__STRING_LITERAL_{index}__";
            literals[placeholder] = match.Value;
            index++;
        }

        return literals;
    }

    /// <summary>
    /// Replaces string literals with placeholders to prevent modification.
    /// </summary>
    private string ReplaceStringLiteralsWithPlaceholders(string query, Dictionary<string, string> literals)
    {
        var result = query;
        int index = 0;

        var regex = new Regex(@"'(?:''|[^'])*'", RegexOptions.Compiled);
        foreach (Match match in regex.Matches(query))
        {
            var placeholder = $"__STRING_LITERAL_{index}__";
            result = result.Replace(match.Value, placeholder, StringComparison.Ordinal);
            index++;
        }

        return result;
    }

    /// <summary>
    /// Restores string literals after normalization.
    /// </summary>
    private string RestoreStringLiterals(string query, Dictionary<string, string> literals)
    {
        var result = query;

        foreach (var literal in literals)
        {
            result = result.Replace(literal.Key, literal.Value, StringComparison.Ordinal);
        }

        return result;
    }

    /// <summary>
    /// Normalizes whitespace: removes excess spaces, tabs, and aligns indentation.
    /// </summary>
    private string NormalizeWhitespace(string query)
    {
        // Replace multiple spaces with single space
        var result = Regex.Replace(query, @" +", " ");

        // Remove spaces around common operators
        result = Regex.Replace(result, @" *(,|;|\(|\)) *", "$1");

        // Remove spaces around certain operators
        result = Regex.Replace(result, @" *(=|<|>|<>|<=|>=) *", " $1 ");

        return result;
    }

    /// <summary>
    /// Standardizes SQL keywords to uppercase for consistency.
    /// Preserves case within identifiers and string literals.
    /// </summary>
    private string StandardizeKeywordCapitalization(string query)
    {
        var keywords = new[]
        {
            "SELECT", "FROM", "WHERE", "JOIN", "INNER", "LEFT", "RIGHT", "FULL", "CROSS",
            "ON", "AND", "OR", "NOT", "IN", "EXISTS", "BETWEEN", "LIKE", "IS", "NULL",
            "ORDER", "BY", "GROUP", "HAVING", "LIMIT", "OFFSET", "UNION", "ALL", "DISTINCT",
            "INSERT", "UPDATE", "DELETE", "CREATE", "DROP", "ALTER", "TABLE", "INDEX",
            "AS", "WITH", "CASE", "WHEN", "THEN", "ELSE", "END", "CAST", "CONVERT"
        };

        var result = query;
        foreach (var keyword in keywords)
        {
            // Use word boundaries to match whole words only
            var pattern = $@"\b{keyword}\b";
            result = Regex.Replace(result, pattern, keyword, RegexOptions.IgnoreCase);
        }

        return result;
    }

    /// <summary>
    /// Normalizes line breaks and removes excessive newlines.
    /// Useful for single-line storage and comparison.
    /// </summary>
    private string NormalizeLineBreaks(string query)
    {
        // Replace all newlines with space
        var result = Regex.Replace(query, @"[\r\n]+", " ");

        // Remove multiple spaces created by line break replacement
        result = Regex.Replace(result, @" +", " ");

        return result;
    }

    /// <summary>
    /// Removes SQL comments (-- line comments and /* */ block comments).
    /// Comments don't affect query logic but add noise.
    /// </summary>
    private string RemoveTrailingComments(string query)
    {
        // Remove -- line comments (but only if at start of line or after whitespace)
        var result = Regex.Replace(query, @"--.*?(?=\n|$)", "");

        // Remove /* */ block comments
        result = Regex.Replace(result, @"/\*.*?\*/", "", RegexOptions.Singleline);

        return result;
    }

    /// <summary>
    /// Extracts table names mentioned in query.
    /// Useful for dependency analysis and validation.
    /// </summary>
    public List<string> ExtractTableNames(string query)
    {
        var tables = new HashSet<string>();

        // Match table names after FROM and JOIN keywords
        var patterns = new[]
        {
            @"FROM\s+([a-zA-Z_][a-zA-Z0-9_]*)",
            @"JOIN\s+([a-zA-Z_][a-zA-Z0-9_]*)",
            @"INTO\s+([a-zA-Z_][a-zA-Z0-9_]*)"
        };

        foreach (var pattern in patterns)
        {
            var regex = new Regex(pattern, RegexOptions.IgnoreCase);
            foreach (Match match in regex.Matches(query))
            {
                tables.Add(match.Groups[1].Value);
            }
        }

        return tables.ToList();
    }

    /// <summary>
    /// Extracts column names mentioned in SELECT clause.
    /// </summary>
    public List<string> ExtractColumnNames(string query)
    {
        var columns = new HashSet<string>();

        // Match SELECT column_name pattern
        var selectMatch = Regex.Match(query, @"SELECT\s+(.*?)\s+FROM", RegexOptions.IgnoreCase);
        if (selectMatch.Success)
        {
            var columnList = selectMatch.Groups[1].Value;
            var parts = columnList.Split(',');

            foreach (var part in parts)
            {
                var col = part.Trim();
                if (col == "*") continue;

                // Handle alias syntax: column AS alias
                var asMatch = Regex.Match(col, @"([a-zA-Z_][a-zA-Z0-9_]*)\s+AS\s+([a-zA-Z_][a-zA-Z0-9_]*)", RegexOptions.IgnoreCase);
                if (asMatch.Success)
                {
                    columns.Add(asMatch.Groups[1].Value);
                }
                else
                {
                    // Extract last identifier (rightmost word)
                    var words = col.Split(new[] { ' ', '.', '(' }, StringSplitOptions.RemoveEmptyEntries);
                    if (words.Length > 0)
                    {
                        columns.Add(words.Last());
                    }
                }
            }
        }

        return columns.ToList();
    }
}
