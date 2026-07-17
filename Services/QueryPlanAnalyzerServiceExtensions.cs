#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SqlQueryAnalyzer.Constants;
using SqlQueryAnalyzer.Models;

namespace SqlQueryAnalyzer.Services;

/// <summary>
/// Extension methods for <see cref="QueryPlanAnalyzerService"/> that provide additional analysis capabilities
/// </summary>
public static class QueryPlanAnalyzerServiceExtensions
{
    /// <summary>
    /// Gets the top N most expensive operations in the execution plan
    /// </summary>
    /// <param name="service">The analyzer service instance</param>
    /// <param name="plan">The query plan to analyze</param>
    /// <param name="topN">Number of top operations to return (default: 5)</param>
    /// <returns>List of expensive operations sorted by estimated cost</returns>
    /// <exception cref="ArgumentNullException">Thrown when service is null</exception>
    /// <exception cref="ArgumentNullException">Thrown when plan is null</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when topN is less than 1</exception>
    public static Task<IReadOnlyList<PlanNode>> GetExpensiveOperationsAsync(
        this QueryPlanAnalyzerService service,
        QueryPlan plan,
        int topN = 5)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentOutOfRangeException.ThrowIfLessThan(topN, 1);

        var result = plan.GetExpensiveOperations(topN);
        return Task.FromResult<IReadOnlyList<PlanNode>>(result);
    }

    /// <summary>
    /// Gets all index operations (seeks, scans, lookups) from the execution plan
    /// </summary>
    /// <param name="service">The analyzer service instance</param>
    /// <param name="plan">The query plan to analyze</param>
    /// <returns>List of all index operations</returns>
    /// <exception cref="ArgumentNullException">Thrown when service is null</exception>
    /// <exception cref="ArgumentNullException">Thrown when plan is null</exception>
    public static Task<IReadOnlyList<PlanNode>> GetIndexOperationsAsync(
        this QueryPlanAnalyzerService service,
        QueryPlan plan)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(plan);

        var result = plan.GetIndexOperations();
        return Task.FromResult<IReadOnlyList<PlanNode>>(result);
    }

    /// <summary>
    /// Gets a summary report of the execution plan analysis
    /// </summary>
    /// <param name="service">The analyzer service instance</param>
    /// <param name="plan">The query plan to analyze</param>
    /// <returns>Dictionary containing plan summary statistics</returns>
    /// <exception cref="ArgumentNullException">Thrown when service is null</exception>
    /// <exception cref="ArgumentNullException">Thrown when plan is null</exception>
    public static Task<Dictionary<string, object>> GetPlanSummaryAsync(
        this QueryPlanAnalyzerService service,
        QueryPlan plan)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(plan);

        return Task.FromResult(plan.ToSummary());
    }

    /// <summary>
    /// Groups performance issues by their type for better analysis
    /// </summary>
    /// <param name="service">The analyzer service instance</param>
    /// <param name="issues">List of performance issues to group</param>
    /// <returns>Dictionary grouping issues by their <see cref="IssueType"/></returns>
    /// <exception cref="ArgumentNullException">Thrown when service is null</exception>
    /// <exception cref="ArgumentNullException">Thrown when issues is null</exception>
    public static IReadOnlyDictionary<IssueType, IReadOnlyList<PerformanceIssue>> GroupByIssueType(
        this QueryPlanAnalyzerService service,
        IEnumerable<PerformanceIssue> issues)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(issues);

        return issues
            .Where(i => i.IsValid())
            .GroupBy(i => i.IssueType)
            .OrderByDescending(g => g.Count())
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<PerformanceIssue>)g.ToList().AsReadOnly()
            );
    }

    /// <summary>
    /// Gets all table scans with high estimated row counts (potential performance issues)
    /// </summary>
    /// <param name="service">The analyzer service instance</param>
    /// <param name="plan">The query plan to analyze</param>
    /// <param name="minRowThreshold">Minimum row count threshold for high-impact scans (default: 1000)</param>
    /// <returns>List of high-impact table scans</returns>
    /// <exception cref="ArgumentNullException">Thrown when service is null</exception>
    /// <exception cref="ArgumentNullException">Thrown when plan is null</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when minRowThreshold is less than 0</exception>
    public static Task<IReadOnlyList<PlanNode>> GetHighImpactTableScansAsync(
        this QueryPlanAnalyzerService service,
        QueryPlan plan,
        int minRowThreshold = 1000)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentOutOfRangeException.ThrowIfNegative(minRowThreshold);

        var result = plan.GetTableScans()
            .Where(scan => scan.EstimatedRows >= minRowThreshold)
            .ToList();
        return Task.FromResult<IReadOnlyList<PlanNode>>(result);
    }

    /// <summary>
    /// Gets a performance score for the query plan (0-100, lower is better)
    /// </summary>
    /// <param name="service">The analyzer service instance</param>
    /// <param name="plan">The query plan to analyze</param>
    /// <returns>Performance score where 0 is optimal and 100 is worst</returns>
    /// <exception cref="ArgumentNullException">Thrown when service is null</exception>
    /// <exception cref="ArgumentNullException">Thrown when plan is null</exception>
    public static Task<int> GetPerformanceScoreAsync(
        this QueryPlanAnalyzerService service,
        QueryPlan plan)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(plan);

        var score = 0;

        // Base score on total estimated cost
        score += (int)Math.Min(plan.TotalEstimatedCost * 2, 30);

        // Add penalty for table scans
        score += plan.GetTableScans().Count * 5;

        // Add penalty for expensive joins
        score += (int)Math.Min(plan.Joins.Sum(j => j.EstimatedCost) * 3, 20);

        // Add penalty for high row estimates
        if (plan.TotalEstimatedRows > 1_000_000)
        {
            score += 15;
        }
        else if (plan.TotalEstimatedRows > 100_000)
        {
            score += 10;
        }
        else if (plan.TotalEstimatedRows > 10_000)
        {
            score += 5;
        }

        // Add penalty for large result sets
        if (plan.TotalEstimatedRows > 10_000)
        {
            score += 10;
        }

        // Cap at 100
        return Task.FromResult(Math.Min(score, 100));
    }

    /// <summary>
    /// Gets a detailed analysis report as a formatted string
    /// </summary>
    /// <param name="service">The analyzer service instance</param>
    /// <param name="plan">The query plan to analyze</param>
    /// <param name="culture">Culture for formatting numbers (default: InvariantCulture)</param>
    /// <returns>Formatted analysis report string</returns>
    /// <exception cref="ArgumentNullException">Thrown when service is null</exception>
    /// <exception cref="ArgumentNullException">Thrown when plan is null</exception>
    public static async Task<string> GetAnalysisReportAsync(
        this QueryPlanAnalyzerService service,
        QueryPlan plan,
        CultureInfo? culture = null)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(plan);

        culture ??= CultureInfo.InvariantCulture;

        var report = new System.Text.StringBuilder();
        report.AppendLine("=== SQL QUERY PLAN ANALYSIS REPORT ===");
        report.AppendLine($"Database: {plan.DatabaseName}");
        report.AppendLine($"Captured: {plan.CapturedAt:yyyy-MM-dd HH:mm:ss UTC}");
        report.AppendLine($"Plan Format: {plan.Format}");
        report.AppendLine($"Estimated: {plan.IsEstimated}");
        report.AppendLine();

        report.AppendLine("=== PLAN SUMMARY ===");
        report.AppendLine($"Total Estimated Cost: {plan.TotalEstimatedCost:F2}");
        report.AppendLine(string.Format(culture, "Total Estimated Rows: {0:N0}", plan.TotalEstimatedRows));
        report.AppendLine(string.Format(culture, "Total Logical Reads: {0:N0}", plan.TotalLogicalReads));
        report.AppendLine(string.Format(culture, "Total Physical Reads: {0:N0}", plan.TotalPhysicalReads));
        report.AppendLine(string.Format(culture, "Total Nodes: {0:N0}", plan.AllNodes.Count));
        report.AppendLine(string.Format(culture, "Table Accesses: {0:N0}", plan.TableAccesses.Count));
        report.AppendLine(string.Format(culture, "Joins: {0:N0}", plan.Joins.Count));
        report.AppendLine(string.Format(culture, "Table Scans: {0:N0}", plan.GetTableScans().Count));
        report.AppendLine();

        report.AppendLine("=== TOP 5 EXPENSIVE OPERATIONS ===");
        var expensiveOps = await service.GetExpensiveOperationsAsync(plan, 5);
        foreach (var (index, op) in expensiveOps.Select((op, i) => (i + 1, op)))
        {
            report.AppendLine($"{index}. [{op.NodeType}] {op.ObjectName}");
            report.AppendLine(string.Format(culture, " Cost: {0:F2}, Rows: {1:N0}", op.EstimatedCost, op.EstimatedRows));
            report.AppendLine();
        }

        report.AppendLine("=== PERFORMANCE SCORE ===");
        var score = await service.GetPerformanceScoreAsync(plan);
        var scoreColor = score switch
        {
            <= 20 => "🟢 EXCELLENT",
            <= 40 => "🟢 GOOD",
            <= 60 => "🟡 FAIR",
            <= 80 => "🟠 POOR",
            _ => "🔴 CRITICAL"
        };
        report.AppendLine($"Performance Score: {scoreColor} ({score}/100)");
        report.AppendLine();

        report.AppendLine("=== RECOMMENDATIONS ===");
        var tableScans = await service.GetHighImpactTableScansAsync(plan);
        if (tableScans.Count > 0)
        {
            report.AppendLine("🔍 High-impact table scans detected:");
            foreach (var scan in tableScans)
            {
                report.AppendLine(string.Format(culture, " - {0}: {1:N0} rows, Cost: {2:F2}", scan.ObjectName, scan.EstimatedRows, scan.EstimatedCost));
            }
            report.AppendLine();
        }

        var indexOps = await service.GetIndexOperationsAsync(plan);
        if (indexOps.Count > 0)
        {
            report.AppendLine(string.Format(culture, "✅ Index operations: {0} found", indexOps.Count));
        }

        return report.ToString();
    }
}