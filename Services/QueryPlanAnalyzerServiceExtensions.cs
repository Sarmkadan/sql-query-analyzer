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
    /// <exception cref="ArgumentNullException">Thrown when plan is null</exception>
    public static async Task<IReadOnlyList<PlanNode>> GetExpensiveOperationsAsync(
        this QueryPlanAnalyzerService service,
        QueryPlan plan,
        int topN = 5)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(plan);

        await Task.CompletedTask;
        return plan.GetExpensiveOperations(topN);
    }

    /// <summary>
    /// Gets all index operations (seeks, scans, lookups) from the execution plan
    /// </summary>
    /// <param name="service">The analyzer service instance</param>
    /// <param name="plan">The query plan to analyze</param>
    /// <returns>List of all index operations</returns>
    /// <exception cref="ArgumentNullException">Thrown when plan is null</exception>
    public static async Task<IReadOnlyList<PlanNode>> GetIndexOperationsAsync(
        this QueryPlanAnalyzerService service,
        QueryPlan plan)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(plan);

        await Task.CompletedTask;
        return plan.GetIndexOperations();
    }

    /// <summary>
    /// Gets a summary report of the execution plan analysis
    /// </summary>
    /// <param name="service">The analyzer service instance</param>
    /// <param name="plan">The query plan to analyze</param>
    /// <returns>Dictionary containing plan summary statistics</returns>
    /// <exception cref="ArgumentNullException">Thrown when plan is null</exception>
    public static async Task<Dictionary<string, object>> GetPlanSummaryAsync(
        this QueryPlanAnalyzerService service,
        QueryPlan plan)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(plan);

        await Task.CompletedTask;
        return plan.ToSummary();
    }

    /// <summary>
    /// Groups performance issues by their type for better analysis
    /// </summary>
    /// <param name="service">The analyzer service instance</param>
    /// <param name="issues">List of performance issues to group</param>
    /// <returns>Dictionary grouping issues by their <see cref="IssueType"/></returns>
    /// <exception cref="ArgumentNullException">Thrown when service or issues is null</exception>
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
    /// <exception cref="ArgumentNullException">Thrown when service or plan is null</exception>
    public static async Task<IReadOnlyList<PlanNode>> GetHighImpactTableScansAsync(
        this QueryPlanAnalyzerService service,
        QueryPlan plan,
        int minRowThreshold = 1000)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(plan);

        await Task.CompletedTask;
        return plan.GetTableScans()
            .Where(scan => scan.EstimatedRows >= minRowThreshold)
            .ToList();
    }

    /// <summary>
    /// Gets a performance score for the query plan (0-100, lower is better)
    /// </summary>
    /// <param name="service">The analyzer service instance</param>
    /// <param name="plan">The query plan to analyze</param>
    /// <returns>Performance score where 0 is optimal and 100 is worst</returns>
    /// <exception cref="ArgumentNullException">Thrown when service or plan is null</exception>
    public static async Task<int> GetPerformanceScoreAsync(
        this QueryPlanAnalyzerService service,
        QueryPlan plan)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(plan);

        await Task.CompletedTask;

        var score = 0;

        // Base score on total estimated cost
        score += (int)Math.Min(plan.TotalEstimatedCost * 2, 30);

        // Add penalty for table scans
        score += plan.GetTableScans().Count * 5;

        // Add penalty for expensive joins
        score += (int)Math.Min(plan.Joins.Sum(j => j.EstimatedCost) * 3, 20);

        // Add penalty for high row estimates
        if (plan.TotalEstimatedRows > 1000000)
        {
            score += 15;
        }
        else if (plan.TotalEstimatedRows > 100000)
        {
            score += 10;
        }
        else if (plan.TotalEstimatedRows > 10000)
        {
            score += 5;
        }

        // Add penalty for large result sets
        if (plan.TotalEstimatedRows > 10000)
        {
            score += 10;
        }

        // Cap at 100
        return Math.Min(score, 100);
    }

    /// <summary>
    /// Gets a detailed analysis report as a formatted string
    /// </summary>
    /// <param name="service">The analyzer service instance</param>
    /// <param name="plan">The query plan to analyze</param>
    /// <param name="culture">Culture for formatting numbers (default: InvariantCulture)</param>
    /// <returns>Formatted analysis report string</returns>
    /// <exception cref="ArgumentNullException">Thrown when service or plan is null</exception>
    public static async Task<string> GetAnalysisReportAsync(
        this QueryPlanAnalyzerService service,
        QueryPlan plan,
        CultureInfo? culture = null)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(plan);

        await Task.CompletedTask;
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
        report.AppendLine($"Total Estimated Rows: {plan.TotalEstimatedRows:N0}");
        report.AppendLine($"Total Logical Reads: {plan.TotalLogicalReads:N0}");
        report.AppendLine($"Total Physical Reads: {plan.TotalPhysicalReads:N0}");
        report.AppendLine($"Total Nodes: {plan.AllNodes.Count:N0}");
        report.AppendLine($"Table Accesses: {plan.TableAccesses.Count:N0}");
        report.AppendLine($"Joins: {plan.Joins.Count:N0}");
        report.AppendLine($"Table Scans: {plan.GetTableScans().Count:N0}");
        report.AppendLine();

        report.AppendLine("=== TOP 5 EXPENSIVE OPERATIONS ===");
        var expensiveOps = await service.GetExpensiveOperationsAsync(plan, 5);
        foreach (var (index, op) in expensiveOps.Select((op, i) => (i + 1, op)))
        {
            report.AppendLine($"{index}. [{op.NodeType}] {op.ObjectName}");
            report.AppendLine($"   Cost: {op.EstimatedCost:F2}, Rows: {op.EstimatedRows:N0}");
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
                report.AppendLine($"  - {scan.ObjectName}: {scan.EstimatedRows:N0} rows, Cost: {scan.EstimatedCost:F2}");
            }
            report.AppendLine();
        }

        var indexOps = await service.GetIndexOperationsAsync(plan);
        if (indexOps.Count > 0)
        {
            report.AppendLine($"✅ Index operations: {indexOps.Count} found");
        }

        return report.ToString();
    }
}