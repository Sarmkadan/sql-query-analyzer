// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace SqlQueryAnalyzer.Utilities;

/// <summary>
/// Detects potential SQL injection vulnerabilities in queries.
/// Identifies unsafe patterns like string concatenation, parameterization issues.
/// This is a static analyzer - patterns detected may be false positives in safe contexts.
/// </summary>
public class SqlInjectionDetector
{
    private readonly ILogger<SqlInjectionDetector> _logger;

    public SqlInjectionDetector(ILogger<SqlInjectionDetector> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Analyzes a query for SQL injection risks.
    /// Returns list of detected vulnerabilities with severity.
    /// </summary>
    public List<SqlInjectionIssue> DetectVulnerabilities(string query)
    {
        var issues = new List<SqlInjectionIssue>();

        if (string.IsNullOrEmpty(query))
            return issues;

        // Run all detection patterns
        issues.AddRange(DetectStringConcatenation(query));
        issues.AddRange(DetectDynamicWhereClause(query));
        issues.AddRange(DetectCommentInjection(query));
        issues.AddRange(DetectUnionBasedInjection(query));
        issues.AddRange(DetectTimeBasedInjection(query));
        issues.AddRange(DetectBooleanBlindInjection(query));

        _logger.LogDebug($"SQL injection analysis complete. Issues found: {issues.Count}");

        return issues;
    }

    /// <summary>
    /// Detects string concatenation patterns that enable SQL injection.
    /// Pattern: ... + variable + '...' where variable could contain SQL
    /// </summary>
    private List<SqlInjectionIssue> DetectStringConcatenation(string query)
    {
        var issues = new List<SqlInjectionIssue>();

        // Look for patterns like: ' + @variable + ' or "' + variable + '"
        var patterns = new[]
        {
            @"'\s*\+\s*[a-zA-Z_@]\w*\s*\+\s*'",
            @"""\s*\+\s*[a-zA-Z_@]\w*\s*\+\s*""",
            @"'\s*\+\s*\[\w+\]\s*\+\s*'"
        };

        foreach (var pattern in patterns)
        {
            var regex = new Regex(pattern, RegexOptions.IgnoreCase);
            foreach (Match match in regex.Matches(query))
            {
                issues.Add(new SqlInjectionIssue
                {
                    Type = "String Concatenation",
                    Severity = "High",
                    Location = match.Index,
                    Pattern = match.Value,
                    Description = "String concatenation detected. Use parameterized queries instead."
                });
            }
        }

        return issues;
    }

    /// <summary>
    /// Detects dynamic WHERE clause construction.
    /// Problematic when WHERE conditions built from untrusted input.
    /// </summary>
    private List<SqlInjectionIssue> DetectDynamicWhereClause(string query)
    {
        var issues = new List<SqlInjectionIssue>();

        // Look for WHERE with variable references that could be concatenated
        var pattern = @"WHERE\s+.*?(\+|CONCAT|CONCATENATE).*?(?=\s+(AND|OR|ORDER|GROUP|LIMIT|;|$))";
        var regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);

        foreach (Match match in regex.Matches(query))
        {
            if (match.Value.Contains("+") || match.Value.ToUpper().Contains("CONCAT"))
            {
                issues.Add(new SqlInjectionIssue
                {
                    Type = "Dynamic WHERE Clause",
                    Severity = "High",
                    Location = match.Index,
                    Pattern = match.Value,
                    Description = "Dynamic WHERE clause detected. Concatenated conditions may enable injection."
                });
            }
        }

        return issues;
    }

    /// <summary>
    /// Detects comment injection attempts.
    /// Pattern: ... ' ; -- or ... ' ; /* */
    /// </summary>
    private List<SqlInjectionIssue> DetectCommentInjection(string query)
    {
        var issues = new List<SqlInjectionIssue>();

        // Comments might be used to hide rest of query
        var patterns = new[] { @"'.*;?--", @"'.*;?/\*", @"'.*?--", @"'.*?/\*" };

        foreach (var pattern in patterns)
        {
            var regex = new Regex(pattern, RegexOptions.IgnoreCase);
            foreach (Match match in regex.Matches(query))
            {
                issues.Add(new SqlInjectionIssue
                {
                    Type = "Comment Injection",
                    Severity = "Medium",
                    Location = match.Index,
                    Pattern = match.Value,
                    Description = "Comment sequence detected that could hide injection payload."
                });
            }
        }

        return issues;
    }

    /// <summary>
    /// Detects UNION-based injection attempts.
    /// Pattern: ... UNION SELECT ...
    /// </summary>
    private List<SqlInjectionIssue> DetectUnionBasedInjection(string query)
    {
        var issues = new List<SqlInjectionIssue>();

        // UNION-based injection is common attack vector
        var pattern = @"UNION\s+(ALL\s+)?SELECT";
        var regex = new Regex(pattern, RegexOptions.IgnoreCase);

        foreach (Match match in regex.Matches(query))
        {
            // Only flag if UNION appears to be added dynamically (after WHERE)
            var beforeUnion = query.Substring(0, match.Index).ToUpper();
            if (beforeUnion.Contains("WHERE") && beforeUnion.Contains("+"))
            {
                issues.Add(new SqlInjectionIssue
                {
                    Type = "UNION-based Injection",
                    Severity = "Critical",
                    Location = match.Index,
                    Pattern = match.Value,
                    Description = "UNION SELECT found in dynamic query. High risk of injection attack."
                });
            }
        }

        return issues;
    }

    /// <summary>
    /// Detects time-based blind injection attempts.
    /// Pattern: WAITFOR, SLEEP, DELAY functions
    /// </summary>
    private List<SqlInjectionIssue> DetectTimeBasedInjection(string query)
    {
        var issues = new List<SqlInjectionIssue>();

        var patterns = new[] { @"WAITFOR\s+DELAY", @"SLEEP\s*\(", @"BENCHMARK\s*\(" };

        foreach (var pattern in patterns)
        {
            var regex = new Regex(pattern, RegexOptions.IgnoreCase);
            if (regex.IsMatch(query))
            {
                issues.Add(new SqlInjectionIssue
                {
                    Type = "Time-based Injection",
                    Severity = "Critical",
                    Location = regex.Match(query).Index,
                    Pattern = regex.Match(query).Value,
                    Description = "Time-based injection function detected. Could be used for data exfiltration."
                });
            }
        }

        return issues;
    }

    /// <summary>
    /// Detects boolean-based blind injection attempts.
    /// Pattern: OR 1=1, AND 1=2, etc.
    /// </summary>
    private List<SqlInjectionIssue> DetectBooleanBlindInjection(string query)
    {
        var issues = new List<SqlInjectionIssue>();

        var patterns = new[] { @"OR\s+1\s*=\s*1", @"AND\s+1\s*=\s*2", @"OR\s+''\s*=\s*'" };

        foreach (var pattern in patterns)
        {
            var regex = new Regex(pattern, RegexOptions.IgnoreCase);
            foreach (Match match in regex.Matches(query))
            {
                issues.Add(new SqlInjectionIssue
                {
                    Type = "Boolean-based Injection",
                    Severity = "High",
                    Location = match.Index,
                    Pattern = match.Value,
                    Description = "Boolean-based injection pattern detected."
                });
            }
        }

        return issues;
    }
}

/// <summary>
/// Represents a detected SQL injection vulnerability.
/// </summary>
public class SqlInjectionIssue
{
    public string Type { get; set; } = string.Empty;
    public string Severity { get; set; } = "Low";
    public int Location { get; set; }
    public string Pattern { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public override string ToString() =>
        $"[{Severity}] {Type} at {Location}: {Description} (Pattern: {Pattern})";
}
