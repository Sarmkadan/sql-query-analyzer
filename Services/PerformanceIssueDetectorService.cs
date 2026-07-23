#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
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
    private readonly ILogger<PerformanceIssueDetectorService> _logger;
    private readonly IndexSeverityThresholds _indexSeverity;
    private readonly IReadOnlyList<IDetectorPlugin> _plugins;

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
    public Task<List<PerformanceIssue>> DetectIssuesAsync(DatabaseQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        _logger.LogInformation($"Detecting issues in query: {query.QueryId}");

        var issues = new List<PerformanceIssue>();

        foreach (var plugin in _plugins)
        {
            try
            {
                issues.AddRange(plugin.Analyze(query));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Rule plugin '{plugin.RuleId}' failed - skipping its findings");
            }
        }

        issues = issues.OrderByDescending(i => i.Severity)
                       .ThenByDescending(i => i.EstimatedPerformanceImpact)
                       .ToList();

        _logger.LogInformation($"Found {issues.Count} performance issues");

        return Task.FromResult(issues);
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
