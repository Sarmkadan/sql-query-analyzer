#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace SqlQueryAnalyzer.Formatters;

/// <summary>
/// Formats raw SQL strings into readable, consistently indented output.
/// Supports SELECT, INSERT, UPDATE, DELETE, and CTE statements.
/// </summary>
public sealed class SqlFormatter
{
    private readonly ILogger<SqlFormatter> _logger;
    private readonly int _indentSize;

    private static readonly HashSet<string> MajorKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "SELECT", "FROM", "WHERE", "JOIN", "INNER JOIN", "LEFT JOIN", "RIGHT JOIN",
        "CROSS JOIN", "FULL OUTER JOIN", "ON", "GROUP BY", "ORDER BY", "HAVING",
        "UNION", "UNION ALL", "EXCEPT", "INTERSECT", "INSERT INTO", "VALUES",
        "UPDATE", "SET", "DELETE FROM", "WITH", "LIMIT", "OFFSET", "FETCH"
    };

    /// <summary>
    /// Initializes a new instance of <see cref="SqlFormatter"/>.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    /// <param name="indentSize">Number of spaces per indent level. Defaults to 4.</param>
    public SqlFormatter(ILogger<SqlFormatter> logger, int indentSize = 4)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _indentSize = indentSize > 0 ? indentSize : 4;
    }

    /// <summary>
    /// Formats a SQL query string with proper indentation and line breaks.
    /// </summary>
    /// <param name="sql">The raw SQL string to format.</param>
    /// <returns>A formatted SQL string.</returns>
    public string Format(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
            return string.Empty;

        _logger.LogDebug("Formatting SQL query of length {Length}", sql.Length);

        var normalized = NormalizeWhitespace(sql);
        var tokens = Tokenize(normalized);
        var result = BuildFormatted(tokens);

        _logger.LogDebug("Formatted SQL output length: {Length}", result.Length);
        return result;
    }

    /// <summary>
    /// Removes excessive whitespace and normalizes line endings.
    /// </summary>
    /// <param name="sql">Raw SQL input.</param>
    /// <returns>Whitespace-normalized SQL.</returns>
    public string NormalizeWhitespace(string sql)
    {
        var result = Regex.Replace(sql, @"\s+", " ").Trim();
        return result;
    }

    /// <summary>
    /// Splits a SQL string into logical tokens respecting string literals.
    /// </summary>
    /// <param name="sql">Normalized SQL string.</param>
    /// <returns>List of token strings.</returns>
    public List<string> Tokenize(string sql)
    {
        var tokens = new List<string>();
        var current = new StringBuilder();
        bool inString = false;
        char stringChar = '\'';

        for (int i = 0; i < sql.Length; i++)
        {
            char c = sql[i];

            if (inString)
            {
                current.Append(c);
                if (c == stringChar && (i + 1 >= sql.Length || sql[i + 1] != stringChar))
                    inString = false;
                else if (c == stringChar)
                    i++; // skip escaped quote
                continue;
            }

            if (c == '\'' || c == '"')
            {
                inString = true;
                stringChar = c;
                current.Append(c);
                continue;
            }

            if (c == '(' || c == ')' || c == ',')
            {
                if (current.Length > 0) { tokens.Add(current.ToString().Trim()); current.Clear(); }
                tokens.Add(c.ToString());
                continue;
            }

            if (c == ' ')
            {
                if (current.Length > 0) { tokens.Add(current.ToString().Trim()); current.Clear(); }
                continue;
            }

            current.Append(c);
        }

        if (current.Length > 0)
            tokens.Add(current.ToString().Trim());

        return tokens.Where(t => !string.IsNullOrEmpty(t)).ToList();
    }

    /// <summary>
    /// Determines whether a given word is a major SQL keyword that triggers a new line.
    /// </summary>
    /// <param name="word">The word to check.</param>
    /// <returns>True if the word is a major keyword.</returns>
    public bool IsMajorKeyword(string word)
    {
        return MajorKeywords.Contains(word);
    }

    private string BuildFormatted(List<string> tokens)
    {
        var sb = new StringBuilder();
        int depth = 0;
        string indent() => new(' ', depth * _indentSize);

        for (int i = 0; i < tokens.Count; i++)
        {
            var token = tokens[i];
            var twoWord = i + 1 < tokens.Count ? $"{token} {tokens[i + 1]}" : null;

            if (twoWord != null && MajorKeywords.Contains(twoWord))
            {
                sb.AppendLine();
                sb.Append(indent());
                sb.Append(twoWord.ToUpperInvariant());
                i++;
            }
            else if (MajorKeywords.Contains(token))
            {
                sb.AppendLine();
                sb.Append(indent());
                sb.Append(token.ToUpperInvariant());
            }
            else if (token == "(")
            {
                sb.Append(" (");
                depth++;
            }
            else if (token == ")")
            {
                depth = Math.Max(0, depth - 1);
                sb.AppendLine();
                sb.Append(indent());
                sb.Append(')');
            }
            else if (token == ",")
            {
                sb.Append(',');
                sb.AppendLine();
                sb.Append(indent());
                sb.Append("    ");
            }
            else
            {
                sb.Append(' ');
                sb.Append(token);
            }
        }

        return sb.ToString().TrimStart();
    }
}
