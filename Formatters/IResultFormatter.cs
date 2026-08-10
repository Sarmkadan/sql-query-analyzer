#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using SqlQueryAnalyzer.Models;

namespace SqlQueryAnalyzer.Formatters;

/// <summary>
/// Base interface for formatting analysis results in various output formats.
/// Implementations provide format-specific serialization logic.
/// </summary>
public interface IResultFormatter
{
    /// <summary>
    /// Formats a query analysis result into string representation.
    /// </summary>
    string Format(QueryAnalysisResult result);

    /// <summary>
    /// Formats multiple results into string representation.
    /// Useful for batch analysis output.
    /// </summary>
    string FormatBatch(IEnumerable<QueryAnalysisResult> results);

    /// <summary>
    /// Returns the format type identifier (json, csv, xml, etc).
    /// </summary>
    string GetFormatType();
}

/// <summary>
/// JSON formatter for analysis results.
/// Produces compact or pretty-printed JSON output.
/// </summary>
public class JsonResultFormatter : IResultFormatter
{
    private readonly bool _prettyPrint;

    public JsonResultFormatter(bool prettyPrint = true)
    {
        _prettyPrint = prettyPrint;
    }

    public string Format(QueryAnalysisResult result)
    {
        ArgumentNullException.ThrowIfNull(nameof(result));
        var dict = result.ToJsonDictionary();
        dict["issues"] = result.Issues.Select(i => new
        {
            i.IssueType,
            i.Severity,
            i.Description,
            i.EstimatedPerformanceImpact
        }).ToList();

        dict["suggestions"] = result.IndexSuggestions.Select(s => new
        {
            s.TableName,
            s.ColumnName,
            s.EstimatedPerformanceGain
        }).ToList();

        return SerializeToJson(dict);
    }

    public string FormatBatch(IEnumerable<QueryAnalysisResult> results)
    {
        ArgumentNullException.ThrowIfNull(nameof(results));
        var list = results.Select(r => r.ToJsonDictionary()).ToList();
        return SerializeToJson(new { results = list });
    }

    public string GetFormatType() => "json";

    private string SerializeToJson(object obj)
    {
        // In a real implementation, use System.Text.Json or similar
        // For now, basic JSON generation
        if (obj is Dictionary<string, object> dict)
        {
            var items = new List<string>();
            foreach (var kvp in dict)
            {
                var value = FormatJsonValue(kvp.Value);
                items.Add($@"""{kvp.Key}"": {value}");
            }

            return _prettyPrint
                ? "{\n  " + string.Join(",\n  ", items) + "\n}"
                : "{" + string.Join(",", items) + "}";
        }

        return obj?.ToString() ?? "null";
    }

    private string FormatJsonValue(object? value) =>
        value switch
        {
            null => "null",
            bool b => b.ToString().ToLower(),
            string s => $@"""{EscapeJsonString(s)}""",
            double d => d.ToString("F2"),
            int i => i.ToString(),
            List<object> list => "[" + string.Join(",", list.Select(FormatJsonValue)) + "]",
            _ => $@"""{value}"""
        };

    private string EscapeJsonString(string str) =>
        str.Replace("\\", "\\\\")
           .Replace("\"", "\\\"")
           .Replace("\n", "\\n")
           .Replace("\r", "\\r")
           .Replace("\t", "\\t");
}

/// <summary>
/// CSV formatter for analysis results.
/// One row per issue, suitable for spreadsheet import.
/// </summary>
public class CsvResultFormatter : IResultFormatter
{
    public string Format(QueryAnalysisResult result)
    {
        ArgumentNullException.ThrowIfNull(nameof(result));
        var lines = new List<string>();

        // Header
        lines.Add("QueryId,IssueType,Severity,Description,ImpactPercentage");

        // Data rows - one per issue
        foreach (var issue in result.Issues)
        {
            var row = new[]
            {
                EscapeCsv(result.QueryId),
                EscapeCsv(issue.IssueType.ToString()),
                EscapeCsv(issue.Severity.ToString()),
                EscapeCsv(issue.Description),
                issue.EstimatedPerformanceImpact.ToString("F2")
            };

            lines.Add(string.Join(",", row));
        }

        return string.Join("\n", lines);
    }

    public string FormatBatch(IEnumerable<QueryAnalysisResult> results)
    {
        ArgumentNullException.ThrowIfNull(nameof(results));
        var lines = new List<string> { "QueryId,IssueType,Severity,Description,ImpactPercentage" };

        foreach (var result in results)
        {
            foreach (var issue in result.Issues)
            {
                var row = new[]
                {
                    EscapeCsv(result.QueryId),
                    EscapeCsv(issue.IssueType.ToString()),
                    EscapeCsv(issue.Severity.ToString()),
                    EscapeCsv(issue.Description),
                    issue.EstimatedPerformanceImpact.ToString("F2")
                };

                lines.Add(string.Join(",", row));
            }
        }

        return string.Join("\n", lines);
    }

    public string GetFormatType() => "csv";

    private string EscapeCsv(string value)
    {
        if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}

/// <summary>
/// XML formatter for analysis results.
/// Structured XML suitable for integration systems.
/// </summary>
public class XmlResultFormatter : IResultFormatter
{
    public string Format(QueryAnalysisResult result)
    {
        ArgumentNullException.ThrowIfNull(nameof(result));
        var lines = new List<string>
        {
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>",
            "<QueryAnalysis>",
            $"  <QueryId>{EscapeXml(result.QueryId)}</QueryId>",
            $"  <PerformanceScore>{result.PerformanceScore:F2}</PerformanceScore>",
            $"  <Complexity>{result.Complexity}</Complexity>",
            $"  <HasCriticalIssues>{result.HasCriticalIssues.ToString().ToLower()}</HasCriticalIssues>",
            "  <Issues>"
        };

        foreach (var issue in result.Issues)
        {
            lines.Add("    <Issue>");
            lines.Add($"      <Type>{EscapeXml(issue.IssueType.ToString())}</Type>");
            lines.Add($"      <Severity>{issue.Severity}</Severity>");
            lines.Add($"      <Description>{EscapeXml(issue.Description)}</Description>");
            lines.Add($"      <Impact>{issue.EstimatedPerformanceImpact:F2}</Impact>");
            lines.Add("    </Issue>");
        }

        lines.Add("  </Issues>");
        lines.Add("  <IndexSuggestions>");

        foreach (var suggestion in result.IndexSuggestions)
        {
            lines.Add("    <Suggestion>");
            lines.Add($"      <Table>{EscapeXml(suggestion.TableName)}</Table>");
            lines.Add($"      <Column>{EscapeXml(suggestion.ColumnName ?? "N/A")}</Column>");
            lines.Add($"      <GainPercentage>{suggestion.EstimatedPerformanceGain:F2}</GainPercentage>");
            lines.Add("    </Suggestion>");
        }

        lines.Add("  </IndexSuggestions>");
        lines.Add("</QueryAnalysis>");

        return string.Join("\n", lines);
    }

    public string FormatBatch(IEnumerable<QueryAnalysisResult> results)
    {
        var lines = new List<string>
        {
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>",
            "<QueryAnalysisBatch>"
        };

        foreach (var result in results)
        {
            lines.Add("  <Analysis>");
            lines.Add($"    <QueryId>{EscapeXml(result.QueryId)}</QueryId>");
            lines.Add($"    <Score>{result.PerformanceScore:F2}</Score>");
            lines.Add("  </Analysis>");
        }

        lines.Add("</QueryAnalysisBatch>");
        return string.Join("\n", lines);
    }

    public string GetFormatType() => "xml";

    private string EscapeXml(string value) =>
        value.Replace("&", "&amp;")
             .Replace("<", "&lt;")
             .Replace(">", "&gt;")
             .Replace("\"", "&quot;")
             .Replace("'", "&apos;");
}

/// <summary>
/// HTML formatter for creating detailed analysis reports.
/// </summary>
public class HtmlResultFormatter : IResultFormatter
{
    public string Format(QueryAnalysisResult result)
    {
        ArgumentNullException.ThrowIfNull(nameof(result));
        return $@"
<!DOCTYPE html>
<html>
<head>
    <title>SQL Analysis Report - {result.QueryId}</title>
    <style>
        body {{ font-family: Arial; margin: 20px; background: #f5f5f5; }}
        .container {{ background: white; padding: 20px; border-radius: 8px; max-width: 1000px; margin: 0 auto; }}
        .header {{ border-bottom: 3px solid #2196F3; padding-bottom: 15px; margin-bottom: 20px; }}
        .score {{ font-size: 32px; font-weight: bold; color: #2196F3; }}
        .section {{ margin: 20px 0; }}
        .section h2 {{ border-left: 4px solid #2196F3; padding-left: 10px; }}
        .issue {{ padding: 10px; margin: 5px 0; border-left: 4px solid #FF9800; background: #FFF3E0; }}
        .issue.critical {{ border-left-color: #F44336; background: #FFEBEE; }}
        table {{ width: 100%; border-collapse: collapse; }}
        th, td {{ text-align: left; padding: 8px; border-bottom: 1px solid #ddd; }}
        th {{ background: #f9f9f9; font-weight: bold; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>SQL Query Analysis Report</h1>
            <div class='score'>{result.PerformanceScore:F1}/100</div>
            <p>Analyzed: {result.AnalyzedAt:yyyy-MM-dd HH:mm:ss} UTC</p>
        </div>

        <div class='section'>
            <h2>Summary</h2>
            <table>
                <tr><td>Complexity:</td><td>{result.Complexity}</td></tr>
                <tr><td>Total Issues:</td><td>{result.Issues.Count}</td></tr>
                <tr><td>Suggestions:</td><td>{result.IndexSuggestions.Count}</td></tr>
                <tr><td>Optimization Potential:</td><td>{result.TotalOptimizationPotential:F1}%</td></tr>
            </table>
        </div>

        <div class='section'>
            <h2>Issues Found</h2>
            {string.Join("", result.Issues.Select(i => $@"
            <div class='issue {(i.Severity == Constants.IssueSeverity.Critical ? "critical" : "")}'>
                <strong>{i.IssueType}</strong> [{i.Severity}]
                <br/>{i.Description}
            </div>
            "))}
        </div>
    </div>
</body>
</html>";
    }

    public string FormatBatch(IEnumerable<QueryAnalysisResult> results)
    {
        ArgumentNullException.ThrowIfNull(nameof(results));
        var resultsList = results.ToList();
        var rows = string.Join("", resultsList.Select(r => $@"
        <tr>
            <td>{r.QueryId.Substring(0, 8)}...</td>
            <td>{r.PerformanceScore:F1}</td>
            <td>{r.Complexity}</td>
            <td>{r.Issues.Count}</td>
        </tr>
        "));

        return $@"
<!DOCTYPE html>
<html>
<head><title>SQL Analysis Batch Report</title>
<style>
    body {{ font-family: Arial; margin: 20px; }}
    table {{ width: 100%; border-collapse: collapse; }}
    th, td {{ text-align: left; padding: 8px; border-bottom: 1px solid #ddd; }}
    th {{ background: #f9f9f9; }}
</style>
</head>
<body>
    <h1>Batch Analysis Report</h1>
    <p>Analyzed {resultsList.Count} queries</p>
    <table>
        <tr><th>Query ID</th><th>Score</th><th>Complexity</th><th>Issues</th></tr>
        {rows}
    </table>
</body>
</html>";
    }

    public string GetFormatType() => "html";
}

/// <summary>
/// Text formatter for human-readable console output.
/// </summary>
public class TextResultFormatter : IResultFormatter
{
    public string Format(QueryAnalysisResult result)
    {
        ArgumentNullException.ThrowIfNull(nameof(result));
        var lines = new List<string>
        {
            "═══════════════════════════════════════════",
            "SQL QUERY ANALYSIS REPORT",
            "═══════════════════════════════════════════",
            $"Query ID: {result.QueryId}",
            $"Score: {result.PerformanceScore:F1}/100",
            $"Complexity: {result.Complexity}",
            $"Analyzed: {result.AnalyzedAt:yyyy-MM-dd HH:mm:ss} UTC",
            "",
            "ISSUES FOUND:",
            "───────────────────────────────────────────"
        };

        if (result.Issues.Count == 0)
        {
            lines.Add("No issues detected.");
        }
        else
        {
            foreach (var issue in result.Issues)
            {
                lines.Add($"[{issue.Severity}] {issue.IssueType}");
                lines.Add($"  └─ {issue.Description}");
            }
        }

        lines.Add("");
        lines.Add("OPTIMIZATION SUGGESTIONS:");
        lines.Add("───────────────────────────────────────────");

        if (result.IndexSuggestions.Count == 0)
        {
            lines.Add("No index suggestions.");
        }
        else
        {
            foreach (var suggestion in result.IndexSuggestions.Take(5))
            {
                lines.Add($"• {suggestion.TableName}.{suggestion.ColumnName}");
                lines.Add($"  Gain: {suggestion.EstimatedPerformanceGain:F1}%");
            }
        }

        return string.Join("\n", lines);
    }

    public string FormatBatch(IEnumerable<QueryAnalysisResult> results)
    {
        var lines = new List<string> { "BATCH ANALYSIS RESULTS", "=" + new string('=', 40) };

        foreach (var result in results)
        {
            lines.Add($"{result.QueryId}: {result.PerformanceScore:F1}/100 ({result.Issues.Count} issues)");
        }

        return string.Join("\n", lines);
    }

    public string GetFormatType() => "text";
}
