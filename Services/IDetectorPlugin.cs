#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Generic;
using SqlQueryAnalyzer.Models;

namespace SqlQueryAnalyzer.Services;

/// <summary>
/// Unifies the built-in performance rules and the third-party plugin layer under a single
/// contract: given a parsed <see cref="DatabaseQuery"/>, produce zero or more
/// <see cref="PerformanceIssue"/> findings for one specific rule.
/// Replaces the former two-tier model where built-in checks lived as private methods on
/// <see cref="PerformanceIssueDetectorService"/> while external rules implemented a separate
/// plugin abstraction. Implementations are stateless, individually testable, and can be
/// toggled or reordered by changing the registered collection - no changes to the
/// orchestrator are required to add, remove, or disable a rule.
/// </summary>
public interface IDetectorPlugin
{
    /// <summary>
    /// Gets the stable, unique identifier of the rule implemented by this plugin
    /// (e.g. "select-star", "cartesian-join"). Used for toggling rules, correlating
    /// findings, and generating the rule catalog documentation.
    /// </summary>
    string RuleId { get; }

    /// <summary>
    /// Analyzes the supplied query and returns any performance issues the rule detects.
    /// Implementations must be side-effect free and safe to call concurrently for
    /// different queries.
    /// </summary>
    /// <param name="query">The parsed query to analyze.</param>
    /// <returns>A sequence of detected issues; empty when the rule does not apply.</returns>
    IEnumerable<PerformanceIssue> Analyze(DatabaseQuery query);
}
