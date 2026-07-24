#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SqlQueryAnalyzer.Configuration;
using SqlQueryAnalyzer.Constants;
using SqlQueryAnalyzer.Models;

namespace SqlQueryAnalyzer.Services;

/// <summary>
/// Thin orchestrator that runs the registered set of <see cref="IDetectorPlugin"/> rules
/// against a query and merges their findings. Every built-in rule (SELECT *, missing
/// WHERE/LIMIT, cartesian join, non-sargable predicates, implicit conversions) is a
/// standalone, individually testable <see cref="IDetectorPlugin"/> - this class no longer
/// contains detection logic itself, only sequencing, N+1 cross-query analysis (which
/// operates over a batch rather than a single query and therefore does not fit the
/// single-query plugin contract), and ordering of the combined results.
/// </summary>
public class PerformanceIssueDetectorService : IPerformanceIssueDetectorService
{
    /// <summary>
    /// Default execution budget granted to a single rule plugin before it is treated as
    /// timed out and its findings for the current query are skipped.
    /// </summary>
    public static readonly TimeSpan DefaultDetectorTimeout = TimeSpan.FromSeconds(2);

    private readonly ILogger<PerformanceIssueDetectorService> _logger;
    private readonly IndexSeverityThresholds _indexSeverity;
    private readonly IReadOnlyList<IDetectorPlugin> _plugins;

    /// <summary>
    /// Gets or sets the maximum time a single rule plugin is allowed to run before it is
    /// treated as timed out and recorded as a diagnostic instead of aborting the whole
    /// detection run. Defaults to <see cref="DefaultDetectorTimeout"/>.
    /// </summary>
    public TimeSpan DetectorTimeout { get; set; } = DefaultDetectorTimeout;

    /// <summary>
    /// Creates the service with the default set of built-in rule plugins.
    /// </summary>
    /// <param name="logger">Logger used for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="logger"/> is null.</exception>
    public PerformanceIssueDetectorService(ILogger<PerformanceIssueDetectorService> logger)
        : this(logger, settings: null, plugins: null) { }

    /// <summary>
    /// Creates the service with the default set of built-in rule plugins, configured
    /// with the supplied analyzer settings.
    /// </summary>
    /// <param name="logger">Logger used for diagnostic output.</param>
    /// <param name="settings">Optional analyzer settings controlling severity thresholds.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="logger"/> is null.</exception>
    public PerformanceIssueDetectorService(
        ILogger<PerformanceIssueDetectorService> logger,
        AnalyzerSettings? settings)
        : this(logger, settings, plugins: null) { }

    /// <summary>
    /// Creates the service with an explicit set of rule plugins, enabling callers (and
    /// dependency injection) to register custom or reduced rule sets without touching
    /// this orchestrator.
    /// </summary>
    /// <param name="logger">Logger used for diagnostic output.</param>
    /// <param name="settings">Optional analyzer settings controlling severity thresholds.</param>
    /// <param name="plugins">
    /// Rule plugins to run, in order. When null or empty, the default built-in rule set
    /// is used instead.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="logger"/> is null.</exception>
    public PerformanceIssueDetectorService(
        ILogger<PerformanceIssueDetectorService> logger,
        AnalyzerSettings? settings,
        IEnumerable<IDetectorPlugin>? plugins)
    {
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;
        _indexSeverity = settings?.Analysis?.IndexSeverity ?? new IndexSeverityThresholds();

        var resolved = plugins?.ToList();
        _plugins = resolved is { Count: > 0 }
            ? resolved
            : CreateDefaultPlugins(_indexSeverity);
    }

    /// <summary>
    /// Builds the default set of built-in rule plugins.
    /// </summary>
    /// <param name="indexSeverity">Severity thresholds shared by index-related rules.</param>
    private static IReadOnlyList<IDetectorPlugin> CreateDefaultPlugins(IndexSeverityThresholds indexSeverity) =>
    [
        new SelectStarDetectorPlugin(),
        new MissingWhereOrLimitDetectorPlugin(),
        new CartesianJoinDetectorPlugin(),
        new JoinColumnTypeDetectorPlugin(),
        new NonSargablePredicateDetectorPlugin(indexSeverity),
        new ImplicitConversionDetectorPlugin()
    ];

    /// <summary>
    /// Gets the rule plugins currently registered with this orchestrator, exposed so the
    /// rule catalog (e.g. the QUICKSTART documentation) can be generated from
    /// <see cref="IDetectorPlugin.RuleId"/> metadata instead of being maintained by hand.
    /// </summary>
    public IReadOnlyList<IDetectorPlugin> Plugins => _plugins;

    /// <summary>
    /// Runs every registered rule plugin against the query and returns the merged,
    /// severity-ordered set of findings.
    /// </summary>
    /// <param name="query">The query to analyze.</param>
    /// <returns>The detected performance issues, ordered by severity then impact.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="query"/> is null.</exception>
    public async Task<List<PerformanceIssue>> DetectIssuesAsync(DatabaseQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        var (issues, _) = await DetectIssuesWithDiagnosticsAsync(query).ConfigureAwait(false);
        return issues;
    }

    /// <summary>
    /// Runs every registered rule plugin against the query, isolating each plugin behind its
    /// own timeout (<see cref="DetectorTimeout"/>) and exception boundary so that one faulty
    /// or slow plugin (e.g. a third-party plugin such as a distinct-abuse detector) cannot
    /// abort detection for the remaining rules. Failures and timeouts are recorded as
    /// diagnostics instead of being silently dropped.
    /// </summary>
    /// <param name="query">The query to analyze.</param>
    /// <param name="cancellationToken">Token used to cancel the whole detection run.</param>
    /// <returns>The detected issues, severity-ordered, alongside diagnostics for any plugin that could not complete.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="query"/> is null.</exception>
    public async Task<(List<PerformanceIssue> Issues, List<AnalysisDiagnostic> Diagnostics)> DetectIssuesWithDiagnosticsAsync(
        DatabaseQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        _logger.LogInformation($"Detecting issues in query: {query.QueryId}");

        var issues = new List<PerformanceIssue>();
        var diagnostics = new List<AnalysisDiagnostic>();

        foreach (var plugin in _plugins)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var pluginIssues = await Task.Run(() => plugin.Analyze(query).ToList(), cancellationToken)
                    .WaitAsync(DetectorTimeout, cancellationToken)
                    .ConfigureAwait(false);
                issues.AddRange(pluginIssues);
            }
            catch (TimeoutException)
            {
                var message = $"Detector '{plugin.RuleId}' timed out after {DetectorTimeout.TotalMilliseconds:F0}ms";
                _logger.LogError($"Rule plugin '{plugin.RuleId}' timed out - skipping its findings");
                diagnostics.Add(new AnalysisDiagnostic { RuleId = plugin.RuleId, Message = message, TimedOut = true });
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // The caller's own cancellation token fired: this is a genuine abort request, not a fault to isolate.
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Rule plugin '{plugin.RuleId}' failed - skipping its findings");
                diagnostics.Add(new AnalysisDiagnostic { RuleId = plugin.RuleId, Message = ex.Message, TimedOut = false });
            }
        }

        issues = issues.OrderByDescending(i => i.Severity)
                       .ThenByDescending(i => i.EstimatedPerformanceImpact)
                       .ToList();

        _logger.LogInformation($"Found {issues.Count} performance issues ({diagnostics.Count} detector(s) failed or timed out)");

        return (issues, diagnostics);
    }

    /// <summary>
    /// Detects potential N+1 query patterns across a batch of queries: many separate
    /// queries repeatedly hitting the same table are a strong signal of a loop-based
    /// access pattern that should instead be a single JOIN or a batched query. This
    /// analysis is inherently cross-query, so it is not expressed as an
    /// <see cref="IDetectorPlugin"/> (which analyzes one query at a time).
    /// </summary>
    /// <param name="queries">The batch of queries to inspect.</param>
    /// <returns>Detected N+1 findings, one per offending table group.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="queries"/> is null.</exception>
    public ValueTask<List<PerformanceIssue>> DetectNPlusOneAsync(List<DatabaseQuery> queries)
    {
        ArgumentNullException.ThrowIfNull(queries);

        _logger.LogInformation($"Detecting N+1 query patterns in {queries.Count} queries");

        var issues = new List<PerformanceIssue>();

        var tableGroups = queries
            .Where(q => q.ReferencedTables.Count > 0)
            .GroupBy(q => q.ReferencedTables.FirstOrDefault() ?? string.Empty)
            .ToList();

        foreach (var group in tableGroups)
        {
            if (group.Count() > 10)
            {
                issues.Add(new PerformanceIssue
                {
                    IssueType = IssueType.NPlusOne,
                    Severity = IssueSeverity.Critical,
                    Description = $"Potential N+1 pattern: {group.Count()} queries accessing {group.Key}",
                    EstimatedPerformanceImpact = 50.0,
                    RecommendedFix = "Use JOIN or batch queries instead of loop-based queries",
                    Priority = 1
                });
            }
        }

        return ValueTask.FromResult(issues);
    }

    /// <summary>
    /// Runs the join-related rule plugins (<see cref="CartesianJoinDetectorPlugin"/> and
    /// <see cref="JoinColumnTypeDetectorPlugin"/>) against a single query. Retained as a
    /// dedicated entry point for callers that only need join diagnostics.
    /// </summary>
    /// <param name="query">The query to analyze.</param>
    /// <returns>Detected join-related issues.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="query"/> is null.</exception>
    public ValueTask<List<PerformanceIssue>> DetectJoinIssuesAsync(DatabaseQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        _logger.LogInformation("Detecting join-related issues");

        var issues = _plugins
            .Where(p => p is CartesianJoinDetectorPlugin or JoinColumnTypeDetectorPlugin)
            .SelectMany(p => p.Analyze(query))
            .ToList();

        return ValueTask.FromResult(issues);
    }

    /// <summary>
    /// Runs the non-sargable-predicate rule plugin against a single query. Retained as a
    /// dedicated entry point for callers that only need index-opportunity diagnostics.
    /// </summary>
    /// <param name="query">The query to analyze.</param>
    /// <returns>Detected index-opportunity issues (OR conditions, leading wildcards).</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="query"/> is null.</exception>
    public ValueTask<List<PerformanceIssue>> DetectIndexOpportunitiesAsync(DatabaseQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        _logger.LogInformation("Analyzing index opportunities");

        var issues = _plugins
            .OfType<NonSargablePredicateDetectorPlugin>()
            .SelectMany(p => p.Analyze(query))
            .Where(i => i.IssueType is IssueType.OrCondition or IssueType.LeadingWildcard)
            .ToList();

        return ValueTask.FromResult(issues);
    }
}
