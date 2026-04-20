#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using SqlQueryAnalyzer.Models;

namespace SqlQueryAnalyzer.Services;

/// <summary>
/// Core contract for the query profiler subsystem.
/// Implementations collect pipeline timings, execution plan statistics, resource usage,
/// and ranked optimization suggestions for one or more SQL queries.
/// </summary>
public interface IQueryProfilerService
{
    /// <summary>
    /// Profiles a raw SQL query string and returns a fully populated report.
    /// The query is parsed internally before analysis begins.
    /// </summary>
    /// <param name="queryText">SQL text to profile. Must not be null or whitespace.</param>
    /// <param name="options">
    /// Optional per-invocation overrides. When <c>null</c>, the service falls back to
    /// the values in <see cref="Configuration.ProfilerSettings"/>.
    /// </param>
    /// <returns>A <see cref="QueryProfilerReport"/> containing timings, metrics, and suggestions.</returns>
    Task<QueryProfilerReport> ProfileQueryAsync(string queryText, ProfilerOptions? options = null);

    /// <summary>
    /// Profiles a pre-parsed <see cref="DatabaseQuery"/> and returns a fully populated report.
    /// Use this overload when the caller has already built the query model (e.g., from a repository).
    /// </summary>
    /// <param name="query">The parsed query to profile. Must not be null.</param>
    /// <param name="options">
    /// Optional per-invocation overrides. When <c>null</c>, the service falls back to
    /// the values in <see cref="Configuration.ProfilerSettings"/>.
    /// </param>
    /// <returns>A <see cref="QueryProfilerReport"/> containing timings, metrics, and suggestions.</returns>
    Task<QueryProfilerReport> ProfileQueryAsync(DatabaseQuery query, ProfilerOptions? options = null);

    /// <summary>
    /// Profiles a collection of queries and returns one report per query in the same order.
    /// </summary>
    /// <param name="queries">Queries to profile. Must not be null.</param>
    /// <param name="options">
    /// Optional per-invocation overrides applied uniformly across the batch.
    /// </param>
    /// <returns>An ordered list of <see cref="QueryProfilerReport"/> instances.</returns>
    Task<List<QueryProfilerReport>> ProfileBatchAsync(
        IEnumerable<DatabaseQuery> queries,
        ProfilerOptions? options = null);

    /// <summary>
    /// Generates prioritized optimization suggestions from a completed profiler report.
    /// Can be called after the fact to regenerate suggestions with updated heuristics.
    /// </summary>
    /// <param name="report">A previously produced profiler report. Must not be null.</param>
    /// <returns>
    /// A list of <see cref="ProfilerSuggestion"/> items ordered by priority and estimated impact.
    /// </returns>
    Task<List<ProfilerSuggestion>> GenerateSuggestionsAsync(QueryProfilerReport report);

    /// <summary>
    /// Compares a baseline profile against a candidate profile (e.g., a rewritten query)
    /// and returns a structured <see cref="ProfileComparison"/> with regression detection.
    /// </summary>
    /// <param name="baseline">The original, unoptimized profiler report. Must not be null.</param>
    /// <param name="candidate">The optimized or rewritten profiler report. Must not be null.</param>
    /// <returns>
    /// A <see cref="ProfileComparison"/> containing score deltas, timing deltas,
    /// per-metric diffs, and a regression/improvement verdict.
    /// </returns>
    Task<ProfileComparison> CompareProfilesAsync(
        QueryProfilerReport baseline,
        QueryProfilerReport candidate);
}
