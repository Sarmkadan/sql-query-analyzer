#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using SqlQueryAnalyzer.Models;

namespace SqlQueryAnalyzer.Services;

/// <summary>
/// Parses vendor-specific slow-query logs into structured entries.
/// </summary>
public interface ISlowQueryLogParser
{
    /// <summary>Parses a MySQL slow-query log.</summary>
    Task<List<SlowQueryEntry>> ParseMySqlLogAsync(string logContent);

    /// <summary>Parses a PostgreSQL slow-query log.</summary>
    Task<List<SlowQueryEntry>> ParsePostgreSqlLogAsync(string logContent);

    /// <summary>Parses a SQL Server tab-separated slow-query export.</summary>
    Task<List<SlowQueryEntry>> ParseSqlServerLogAsync(string logContent);

    /// <summary>Returns the slowest entries after optional filtering.</summary>
    List<SlowQueryEntry> GetTopSlowQueries(List<SlowQueryEntry> entries, int topN = 10, TimeSpan? minDuration = null);

    /// <summary>Aggregates entries that have the same normalized query text.</summary>
    List<SlowQueryEntry> GetAggregatedByNormalizedQuery(List<SlowQueryEntry> entries);
}

/// <summary>
/// Default implementation of <see cref="ISlowQueryLogParser"/>.
/// </summary>
public sealed partial class SlowQueryLogParser : ISlowQueryLogParser
{
    private readonly ILogger<SlowQueryLogParser> _logger;

    /// <summary>
    /// Initializes a new parser instance.
    /// </summary>
    public SlowQueryLogParser(ILogger<SlowQueryLogParser> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;
    }

    /// <inheritdoc/>
    public Task<List<SlowQueryEntry>> ParseMySqlLogAsync(string logContent)
    {
        ArgumentNullException.ThrowIfNull(logContent);
        
        var entries = new List<SlowQueryEntry>();
        foreach (var block in SplitMySqlBlocks(logContent))
        {
            try
            {
                var timeMatch = MySqlTimeRegex().Match(block);
                var statsMatch = MySqlStatsRegex().Match(block);
                if (!timeMatch.Success || !statsMatch.Success)
                {
                    _logger.LogWarning("Skipping malformed MySQL slow-log block");
                    continue;
                }

                var database = string.Empty;
                var queryLines = new List<string>();
                foreach (var rawLine in block.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    if (rawLine.StartsWith("#", StringComparison.Ordinal))
                        continue;
                    if (rawLine.StartsWith("use ", StringComparison.OrdinalIgnoreCase))
                    {
                        database = rawLine[4..].TrimEnd(';').Trim();
                        continue;
                    }

                    queryLines.Add(rawLine.TrimEnd(';'));
                }

                var entry = new SlowQueryEntry
                {
                    Timestamp = DateTime.TryParse(timeMatch.Groups["time"].Value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var timestamp) ? timestamp : DateTime.UtcNow,
                    UserHost = MySqlUserHostRegex().Match(block).Groups["userhost"].Value.Trim(),
                    Duration = TimeSpan.FromSeconds(ParseDouble(statsMatch.Groups["queryTime"].Value)),
                    LockTime = TimeSpan.FromSeconds(ParseDouble(statsMatch.Groups["lockTime"].Value)),
                    RowsSent = ParseLong(statsMatch.Groups["rowsSent"].Value),
                    RowsExamined = ParseLong(statsMatch.Groups["rowsExamined"].Value),
                    Database = database,
                    QueryText = string.Join(Environment.NewLine, queryLines).Trim(),
                    LogSource = "MySQL"
                };

                if (!string.IsNullOrWhiteSpace(entry.QueryText))
                    entries.Add(entry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing MySQL slow-log block");
                // Do not rethrow so that parsing can continue for other blocks
            }
        }

        return Task.FromResult(entries);
    }

    /// <inheritdoc/>
    public Task<List<SlowQueryEntry>> ParsePostgreSqlLogAsync(string logContent)
    {
        ArgumentNullException.ThrowIfNull(logContent);

        var entries = new List<SlowQueryEntry>();
        foreach (Match match in PostgreSqlLogRegex().Matches(logContent))
        {
            try
            {
                entries.Add(new SlowQueryEntry
                {
                    Timestamp = DateTime.TryParse(match.Groups["timestamp"].Value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var timestamp) ? timestamp : DateTime.UtcNow,
                    UserHost = match.Groups["user"].Value,
                    Database = match.Groups["database"].Value,
                    Duration = TimeSpan.FromMilliseconds(ParseDouble(match.Groups["duration"].Value)),
                    QueryText = match.Groups["statement"].Value.Trim(),
                    LogSource = "PostgreSQL"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing PostgreSQL slow-log entry");
            }
        }

        return Task.FromResult(entries);
    }

    /// <inheritdoc/>
    public Task<List<SlowQueryEntry>> ParseSqlServerLogAsync(string logContent)
    {
        ArgumentNullException.ThrowIfNull(logContent);

        var entries = new List<SlowQueryEntry>();
        var lines = logContent.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length <= 1)
            return Task.FromResult(entries);

        foreach (var line in lines.Skip(1))
        {
            var parts = line.Split('\t');
            if (parts.Length < 5)
            {
                _logger.LogWarning("Skipping malformed SQL Server slow-log row");
                continue;
            }

            try
            {
                entries.Add(new SlowQueryEntry
                {
                    Timestamp = DateTime.TryParse(parts[0], CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var timestamp) ? timestamp : DateTime.UtcNow,
                    QueryText = parts[1],
                    Duration = TimeSpan.FromMilliseconds(ParseDouble(parts[2])),
                    RowsSent = ParseLong(parts[4]),
                    RowsExamined = ParseLong(parts[4]),
                    LogSource = "SqlServer",
                    Metadata = new Dictionary<string, string>
                    {
                        ["execution_count"] = parts[3],
                        ["avg_rows"] = parts[4]
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing SQL Server slow-log row");
            }
        }

        return Task.FromResult(entries);
    }

    /// <inheritdoc/>
    public List<SlowQueryEntry> GetTopSlowQueries(List<SlowQueryEntry> entries, int topN = 10, TimeSpan? minDuration = null)
    {
        ArgumentNullException.ThrowIfNull(entries);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(topN);

        var threshold = minDuration ?? TimeSpan.Zero;
        return entries
            .Where(e => e.Duration >= threshold)
            .OrderByDescending(e => e.Duration)
            .Take(topN)
            .ToList();
    }

    /// <inheritdoc/>
    public List<SlowQueryEntry> GetAggregatedByNormalizedQuery(List<SlowQueryEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        return entries
            .GroupBy(entry => NormalizeQuery(entry.QueryText), StringComparer.Ordinal)
            .Select(group =>
            {
                var representative = group.First();
                var durations = group.Select(entry => entry.Duration).ToList();
                var metadata = new Dictionary<string, string>(representative.Metadata, StringComparer.OrdinalIgnoreCase)
                {
                    ["occurrence_count"] = durations.Count.ToString(CultureInfo.InvariantCulture),
                    ["max_duration_ms"] = durations.Max().TotalMilliseconds.ToString(CultureInfo.InvariantCulture)
                };

                return new SlowQueryEntry
                {
                    EntryId = representative.EntryId,
                    QueryText = representative.QueryText,
                    Duration = TimeSpan.FromTicks(durations.Sum(duration => duration.Ticks)),
                    LockTime = TimeSpan.FromTicks(group.Sum(entry => entry.LockTime.Ticks)),
                    RowsExamined = group.Sum(entry => entry.RowsExamined),
                    RowsSent = group.Sum(entry => entry.RowsSent),
                    Timestamp = representative.Timestamp,
                    UserHost = representative.UserHost,
                    Database = representative.Database,
                    LogSource = representative.LogSource,
                    Metadata = metadata
                };
            })
            .OrderByDescending(entry => entry.Duration)
            .ToList();
    }

    private static string NormalizeQuery(string queryText)
    {
        var normalized = QuotedLiteralRegex().Replace(queryText.ToLowerInvariant(), "?");
        normalized = NumericLiteralRegex().Replace(normalized, "?");
        return WhitespaceRegex().Replace(normalized, " ").Trim();
    }

    private static IEnumerable<string> SplitMySqlBlocks(string logContent)
    {
        var normalized = logContent.Replace("\r\n", "\n", StringComparison.Ordinal);
        var blocks = normalized.Split("# Time:", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var block in blocks)
            yield return $"# Time: {block.Trim()}";
    }

    private static double ParseDouble(string value) =>
        double.TryParse(value, CultureInfo.InvariantCulture, out var parsed) ? parsed : 0;

    private static long ParseLong(string value) =>
        long.TryParse(value, CultureInfo.InvariantCulture, out var parsed) ? parsed : 0;

    [GeneratedRegex(@"^# Time:\s*(?<time>.+)$", RegexOptions.Multiline)]
    private static partial Regex MySqlTimeRegex();

    [GeneratedRegex(@"^# User@Host:\s*(?<userhost>.+)$", RegexOptions.Multiline)]
    private static partial Regex MySqlUserHostRegex();

    [GeneratedRegex(@"^# Query_time:\s*(?<queryTime>[\d\.]+)\s+Lock_time:\s*(?<lockTime>[\d\.]+)\s+Rows_sent:\s*(?<rowsSent>\d+)\s+Rows_examined:\s*(?<rowsExamined>\d+)", RegexOptions.Multiline)]
    private static partial Regex MySqlStatsRegex();

    [GeneratedRegex(@"^(?<timestamp>\d{4}-\d{2}-\d{2}\s+\d{2}:\d{2}:\d{2}\.\d+\s+\w+)\s+\[\d+\]\s+(?<user>[^@\s]+)@(?<database>[^\s]+)\s+LOG:\s+duration:\s+(?<duration>[\d\.]+)\s+ms\s+statement:\s+(?<statement>.+)$", RegexOptions.Multiline)]
    private static partial Regex PostgreSqlLogRegex();

    [GeneratedRegex("'(?:''|[^'])*'|\"(?:\"\"|[^\"])*\"", RegexOptions.Compiled)]
    private static partial Regex QuotedLiteralRegex();

    [GeneratedRegex(@"(?<![\w$])[-+]?(?:\d+(?:\.\d+)?|\.\d+)(?:[eE][-+]?\d+)?(?![\w$])", RegexOptions.Compiled)]
    private static partial Regex NumericLiteralRegex();

    [GeneratedRegex(@"\s+", RegexOptions.Compiled)]
    private static partial Regex WhitespaceRegex();
}
