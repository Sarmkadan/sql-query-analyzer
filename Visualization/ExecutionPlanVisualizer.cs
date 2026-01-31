// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SqlQueryAnalyzer.Configuration;
using SqlQueryAnalyzer.Models;

namespace SqlQueryAnalyzer.Visualization;

/// <summary>
/// Converts a <see cref="QueryPlan"/> into human-readable text representations.
/// Provides ASCII tree rendering with cost bars, bottleneck annotations,
/// and a horizontal cost-distribution chart per table access.
/// </summary>
public interface IExecutionPlanVisualizer
{
    /// <summary>
    /// Renders the execution plan as a <see cref="PlanVisualization"/> containing an ASCII tree,
    /// a cost-distribution chart, bottleneck annotations, and summary statistics.
    /// </summary>
    /// <param name="plan">The parsed execution plan to render. Must not be null.</param>
    /// <returns>A fully populated <see cref="PlanVisualization"/>.</returns>
    PlanVisualization Render(QueryPlan plan);

    /// <summary>
    /// Renders only the ASCII node tree. Useful for concise log output.
    /// </summary>
    /// <param name="plan">The parsed execution plan to render. Must not be null.</param>
    /// <returns>A multi-line string representation of the plan tree.</returns>
    string RenderTree(QueryPlan plan);

    /// <summary>
    /// Renders a horizontal bar chart showing relative cost per table access operation.
    /// </summary>
    /// <param name="plan">The parsed execution plan. Must not be null.</param>
    /// <returns>A multi-line string containing the cost-distribution chart.</returns>
    string RenderCostDistribution(QueryPlan plan);
}

/// <summary>
/// Default implementation of <see cref="IExecutionPlanVisualizer"/>.
/// Produces fixed-width text output compatible with terminals, log files,
/// CI console output, and HTML <c>&lt;pre&gt;</c> elements.
/// <para>
/// Rendering depth and node count limits, bottleneck thresholds, and cost-bar width
/// are all governed by <see cref="VisualizationSettings"/> at construction time.
/// </para>
/// </summary>
public sealed class ExecutionPlanVisualizer : IExecutionPlanVisualizer
{
    private readonly VisualizationSettings _settings;

    /// <summary>
    /// Initializes a new <see cref="ExecutionPlanVisualizer"/> using the visualization
    /// sub-section of the supplied <paramref name="profilerSettings"/>.
    /// </summary>
    public ExecutionPlanVisualizer(ProfilerSettings profilerSettings)
    {
        ArgumentNullException.ThrowIfNull(profilerSettings);
        _settings = profilerSettings.Visualization;
    }

    /// <inheritdoc/>
    public PlanVisualization Render(QueryPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        return new PlanVisualization
        {
            TextTree = RenderTree(plan),
            CostDistribution = RenderCostDistribution(plan),
            Bottlenecks = IdentifyBottlenecks(plan),
            Stats = BuildStats(plan),
            RenderedAt = DateTime.UtcNow
        };
    }

    /// <inheritdoc/>
    public string RenderTree(QueryPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        if (plan.RootNode == null)
            return "(empty plan — no root node available)";

        var sb = new StringBuilder();
        var separator = new string('─', 72);

        sb.AppendLine($"Execution Plan [{plan.Format}]");
        sb.AppendLine($"  Optimizer cost : {plan.TotalEstimatedCost:F4} units");
        sb.AppendLine($"  Estimated rows : {plan.TotalEstimatedRows:N0}");
        sb.AppendLine($"  Total nodes    : {plan.AllNodes.Count}");
        sb.AppendLine($"  Captured at    : {plan.CapturedAt:u}");
        sb.AppendLine(separator);

        var nodesRendered = 0;
        AppendNode(sb, plan.RootNode,
            prefix: "",
            isLast: true,
            depth: 0,
            maxCost: plan.TotalEstimatedCost > 0 ? plan.TotalEstimatedCost : 1.0,
            nodesRendered: ref nodesRendered);

        var omitted = plan.AllNodes.Count - nodesRendered;
        if (omitted > 0)
            sb.AppendLine($"  ... ({omitted} additional node(s) not shown — increase MaxNodes or MaxDepth)");

        sb.AppendLine(separator);
        return sb.ToString();
    }

    /// <inheritdoc/>
    public string RenderCostDistribution(QueryPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        if (plan.TableAccesses.Count == 0)
            return "(no table access operations recorded in this plan)";

        var sb = new StringBuilder();
        sb.AppendLine("Cost Distribution by Table Access:");
        sb.AppendLine(new string('─', 72));

        var maxCost = plan.TableAccesses.Max(t => t.EstimatedCost);
        if (maxCost <= 0) maxCost = 1;

        const int labelWidth = 42;

        foreach (var access in plan.TableAccesses.OrderByDescending(t => t.EstimatedCost))
        {
            var barLen = (int)Math.Round(access.EstimatedCost / maxCost * _settings.CostBarWidth);
            barLen = Math.Clamp(barLen, 0, _settings.CostBarWidth);

            var bar = new string('█', barLen).PadRight(_settings.CostBarWidth, '·');
            var label = $"{access.TableName} [{access.AccessMethod}]".PadRight(labelWidth);

            sb.AppendLine($"  {label} {bar}  {access.EstimatedCost:F4}");

            if (_settings.ShowRowCounts)
                sb.AppendLine($"  {"".PadRight(labelWidth)} {"".PadRight(_settings.CostBarWidth)}  ~{access.EstimatedRows:N0} rows");
        }

        return sb.ToString();
    }

    // ── Private rendering helpers ─────────────────────────────────────────────

    private void AppendNode(
        StringBuilder sb,
        PlanNode node,
        string prefix,
        bool isLast,
        int depth,
        double maxCost,
        ref int nodesRendered)
    {
        if (depth > _settings.MaxDepth || nodesRendered >= _settings.MaxNodes)
            return;

        nodesRendered++;

        var connector = isLast ? "└── " : "├── ";
        var childPrefix = prefix + (isLast ? "    " : "│   ");

        var costBar = RenderCostBar(node.EstimatedCost, maxCost);
        var isBottleneck = _settings.AnnotateBottlenecks
            && node.EstimatedCost > _settings.BottleneckCostThreshold;

        var objectLabel = string.IsNullOrEmpty(node.ObjectName)
            ? string.Empty
            : $" ({node.ObjectName})";

        var costLabel = _settings.ShowDetailedCosts
            ? $" [IO={node.EstimatedIoCost:F4} CPU={node.EstimatedCpuCost:F4} total={node.EstimatedCost:F4}]"
            : $" cost={node.EstimatedCost:F4}";

        var rowLabel = _settings.ShowRowCounts
            ? $" rows~{node.EstimatedRows:N0}"
            : string.Empty;

        var bottleneckMarker = isBottleneck ? "  ◄ BOTTLENECK" : string.Empty;

        sb.AppendLine(
            $"{prefix}{connector}{node.NodeType}{objectLabel}{costLabel}{rowLabel} {costBar}{bottleneckMarker}");

        for (var i = 0; i < node.Children.Count; i++)
        {
            AppendNode(sb, node.Children[i],
                childPrefix,
                isLast: i == node.Children.Count - 1,
                depth: depth + 1,
                maxCost: maxCost,
                nodesRendered: ref nodesRendered);
        }
    }

    private string RenderCostBar(double cost, double maxCost)
    {
        if (maxCost <= 0) return string.Empty;
        var filled = (int)Math.Round(cost / maxCost * _settings.CostBarWidth);
        filled = Math.Clamp(filled, 0, _settings.CostBarWidth);
        return "[" + new string('■', filled) + new string('·', _settings.CostBarWidth - filled) + "]";
    }

    private List<BottleneckAnnotation> IdentifyBottlenecks(QueryPlan plan)
    {
        return plan.AllNodes
            .Where(n => n.EstimatedCost > _settings.BottleneckCostThreshold)
            .OrderByDescending(n => n.EstimatedCost)
            .Take(10)
            .Select(n => new BottleneckAnnotation
            {
                NodeId = n.NodeId,
                NodeType = n.NodeType,
                ObjectName = n.ObjectName,
                EstimatedCost = n.EstimatedCost,
                Depth = n.Depth,
                Recommendation = BuildRecommendation(n)
            })
            .ToList();
    }

    private static string BuildRecommendation(PlanNode node) =>
        node.NodeType switch
        {
            "Table Scan" =>
                $"Full scan on '{node.ObjectName}' — create a covering index on the most selective filtered columns.",
            "Index Scan" =>
                $"Index scan on '{node.ObjectName}' — a more selective index may convert this to an Index Seek.",
            "Hash Match" =>
                "Hash join detected — verify join columns are indexed, data types match, and statistics are current.",
            "Nested Loops" =>
                "Nested-loop join with high cost — consider adding an index on the inner-side join column.",
            "Merge Join" =>
                "Merge join present — ensure both input streams are sorted on the join key; add ordered indexes if not.",
            "Sort" =>
                "Explicit sort operation — add an index aligned with the ORDER BY or GROUP BY clause to eliminate it.",
            "Key Lookup" =>
                "Key/RID lookup detected — include the required columns in the non-clustered index to avoid the back-to-base lookup.",
            "Parallelism" =>
                "Parallelism (gather/exchange) operator — review MAXDOP settings; consider whether parallelism is beneficial here.",
            "Spool" =>
                "Spool (lazy or eager) detected — often caused by subquery or CTE re-evaluation; try materializing into a temp table.",
            _ =>
                $"High-cost '{node.NodeType}' operator — review query structure, available indexes, and optimizer statistics."
        };

    private static Dictionary<string, object> BuildStats(QueryPlan plan)
    {
        var tableScans = plan.GetTableScans();
        var indexSeeks = plan.AllNodes.Where(n => n.NodeType == "Index Seek").ToList();
        var indexScans = plan.AllNodes.Where(n => n.NodeType == "Index Scan").ToList();
        var sortOps = plan.AllNodes.Where(n => n.NodeType == "Sort").ToList();
        var hashJoins = plan.Joins.Where(j => j.JoinType.Contains("Hash", StringComparison.OrdinalIgnoreCase)).ToList();

        return new Dictionary<string, object>
        {
            { "format",           plan.Format.ToString() },
            { "capturedAt",       plan.CapturedAt.ToString("u") },
            { "isEstimated",      plan.IsEstimated },
            { "totalCost",        plan.TotalEstimatedCost },
            { "totalIoCost",      plan.TotalEstimatedIoCost },
            { "totalCpuCost",     plan.TotalEstimatedCpuCost },
            { "totalRows",        plan.TotalEstimatedRows },
            { "nodeCount",        plan.AllNodes.Count },
            { "tableAccessCount", plan.TableAccesses.Count },
            { "joinCount",        plan.Joins.Count },
            { "tableScanCount",   tableScans.Count },
            { "indexSeekCount",   indexSeeks.Count },
            { "indexScanCount",   indexScans.Count },
            { "sortCount",        sortOps.Count },
            { "hashJoinCount",    hashJoins.Count },
            { "seekToScanRatio",  plan.TableAccesses.Count > 0
                ? $"{indexSeeks.Count}/{indexScans.Count + tableScans.Count}"
                : "n/a" }
        };
    }
}
