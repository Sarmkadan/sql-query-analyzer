#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;
using System.Net;
using System.Text;
using SqlQueryAnalyzer.Configuration;
using SqlQueryAnalyzer.Models;

namespace SqlQueryAnalyzer.Visualization;

/// <summary>
/// Renders query plans as self-contained HTML documents.
/// </summary>
public interface IHtmlPlanVisualizer
{
    /// <summary>
    /// Renders a full HTML page for the supplied execution plan.
    /// </summary>
    string RenderHtml(QueryPlan plan);

    /// <summary>
    /// Renders only the HTML fragment for embedding in larger reports.
    /// </summary>
    string RenderHtmlFragment(QueryPlan plan);
}

/// <summary>
/// Default HTML renderer for <see cref="QueryPlan"/> instances.
/// </summary>
public sealed class HtmlPlanVisualizer : IHtmlPlanVisualizer
{
    private readonly VisualizationSettings _settings;

    /// <summary>
    /// Initializes a new renderer using the visualization settings from the profiler configuration.
    /// </summary>
    public HtmlPlanVisualizer(ProfilerSettings profilerSettings)
    {
        ArgumentNullException.ThrowIfNull(profilerSettings);
        _settings = profilerSettings.Visualization;
    }

    /// <inheritdoc/>
    public string RenderHtml(QueryPlan plan)
    {
        var fragment = RenderHtmlFragment(plan);
        return $@"<!DOCTYPE html>
<html lang=""en"">
<head>
<meta charset=""utf-8"" />
<meta name=""viewport"" content=""width=device-width, initial-scale=1"" />
<title>Query Plan Visualization</title>
<style>
body {{ font-family: Arial, sans-serif; margin: 24px; color: #1f2937; }}
section {{ margin-bottom: 24px; }}
h1, h2 {{ margin-bottom: 12px; }}
.tree details {{ margin-left: 16px; padding-left: 12px; border-left: 1px solid #d1d5db; }}
.tree summary {{ cursor: pointer; list-style: none; }}
.node-line {{ display: flex; flex-wrap: wrap; gap: 12px; align-items: center; }}
.cost-bar {{ width: 220px; height: 12px; border-radius: 999px; background: #e5e7eb; overflow: hidden; display: inline-block; }}
.cost-fill {{ height: 100%; display: block; }}
.cost-low {{ background: #22c55e; }}
.cost-medium {{ background: #eab308; }}
.cost-high {{ background: #ef4444; }}
table {{ width: 100%; border-collapse: collapse; }}
th, td {{ border: 1px solid #d1d5db; padding: 8px; text-align: left; }}
th {{ background: #f3f4f6; }}
.stats {{ display: grid; grid-template-columns: repeat(auto-fit, minmax(180px, 1fr)); gap: 12px; }}
.stat-card {{ border: 1px solid #d1d5db; border-radius: 8px; padding: 12px; background: #f9fafb; }}
.empty-state {{ padding: 16px; border: 1px dashed #9ca3af; border-radius: 8px; color: #6b7280; }}
ul {{ padding-left: 20px; }}
</style>
</head>
<body>
{fragment}
</body>
</html>";
    }

    /// <inheritdoc/>
    public string RenderHtmlFragment(QueryPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        EnsureInitialized(plan);

        if (plan.RootNode == null)
        {
            return "<section><h1>Query Plan Visualization</h1><div class=\"empty-state\">No execution plan data available.</div></section>";
        }

        var maxCost = Math.Max(plan.TotalEstimatedCost, plan.AllNodes.Max(n => n.EstimatedCost));
        if (maxCost <= 0)
            maxCost = 1;

        var bottlenecks = plan.AllNodes
            .Where(n => n.EstimatedCost >= _settings.BottleneckCostThreshold)
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

        var sb = new StringBuilder();
        sb.AppendLine("<section><h1>Query Plan Visualization</h1></section>");
        sb.AppendLine("<section><h2>Summary Statistics</h2><div class=\"stats\">");
        AppendStat(sb, "Format", plan.Format.ToString());
        AppendStat(sb, "Total Cost", plan.TotalEstimatedCost.ToString("F2", CultureInfo.InvariantCulture));
        AppendStat(sb, "Estimated Rows", plan.TotalEstimatedRows.ToString("N0", CultureInfo.InvariantCulture));
        AppendStat(sb, "Plan Nodes", plan.AllNodes.Count.ToString(CultureInfo.InvariantCulture));
        AppendStat(sb, "Table Accesses", plan.TableAccesses.Count.ToString(CultureInfo.InvariantCulture));
        AppendStat(sb, "Captured At", WebUtility.HtmlEncode(plan.CapturedAt.ToString("u", CultureInfo.InvariantCulture)));
        sb.AppendLine("</div></section>");

        sb.AppendLine("<section><h2>Plan Tree</h2><div class=\"tree\">");
        AppendNode(sb, plan.RootNode, maxCost, isRoot: true);
        sb.AppendLine("</div></section>");

        sb.AppendLine("<section><h2>Table Accesses</h2><table><thead><tr><th>Table</th><th>Access Method</th><th>Cost</th><th>Rows</th></tr></thead><tbody>");
        if (plan.TableAccesses.Count == 0)
        {
            sb.AppendLine("<tr><td colspan=\"4\">No table access operations recorded.</td></tr>");
        }
        else
        {
            foreach (var access in plan.TableAccesses.OrderByDescending(t => t.EstimatedCost))
            {
                sb.AppendLine($"<tr><td>{Encode(access.TableName)}</td><td>{Encode(access.AccessMethod)}</td><td>{access.EstimatedCost.ToString("F2", CultureInfo.InvariantCulture)}</td><td>{access.EstimatedRows.ToString("N0", CultureInfo.InvariantCulture)}</td></tr>");
            }
        }

        sb.AppendLine("</tbody></table></section>");
        sb.AppendLine("<section><h2>Bottleneck Highlights</h2>");

        if (bottlenecks.Count == 0)
        {
            sb.AppendLine("<div class=\"empty-state\">No bottlenecks exceeded the configured threshold.</div>");
        }
        else
        {
            sb.AppendLine("<ul>");
            foreach (var bottleneck in bottlenecks)
            {
                sb.AppendLine($"<li><strong>{Encode(bottleneck.NodeType)}</strong> {Encode(bottleneck.ObjectName)} — cost {bottleneck.EstimatedCost.ToString("F2", CultureInfo.InvariantCulture)}. {Encode(bottleneck.Recommendation)}</li>");
            }
            sb.AppendLine("</ul>");
        }

        sb.AppendLine("</section>");
        return sb.ToString();
    }

    private static void EnsureInitialized(QueryPlan plan)
    {
        if (plan.RootNode != null && plan.AllNodes.Count == 0 && plan.TableAccesses.Count == 0 && plan.Joins.Count == 0)
            plan.Initialize();
    }

    private static void AppendStat(StringBuilder sb, string label, string value) =>
        sb.AppendLine($"<div class=\"stat-card\"><strong>{Encode(label)}</strong><div>{value}</div></div>");

    private void AppendNode(StringBuilder sb, PlanNode node, double maxCost, bool isRoot = false)
    {
        var ratio = maxCost <= 0 ? 0 : Math.Clamp(node.EstimatedCost / maxCost, 0, 1);
        var width = Math.Max(2, (int)Math.Round(ratio * 100));
        var cssClass = ratio >= 0.66 ? "cost-high" : ratio >= 0.33 ? "cost-medium" : "cost-low";
        var objectLabel = string.IsNullOrWhiteSpace(node.ObjectName) ? string.Empty : $" ({Encode(node.ObjectName)})";
        var depthLabel = isRoot ? "Root" : $"Depth {node.Depth}";

        sb.AppendLine(isRoot ? "<details open>" : "<details>");
        sb.AppendLine($"<summary><span class=\"node-line\"><strong>{Encode(node.NodeType)}</strong><span>{objectLabel}</span><span>{Encode(depthLabel)}</span><span>{node.EstimatedRows.ToString("N0", CultureInfo.InvariantCulture)} rows</span><span>{node.EstimatedCost.ToString("F2", CultureInfo.InvariantCulture)} cost</span><span class=\"cost-bar\"><span class=\"cost-fill {cssClass}\" style=\"width:{width}%\"></span></span></span></summary>");

        foreach (var child in node.Children.Take(_settings.MaxNodes))
            AppendNode(sb, child, maxCost);

        sb.AppendLine("</details>");
    }

    private static string BuildRecommendation(PlanNode node) =>
        node.NodeType switch
        {
            "Table Scan" => "Add a selective index on the filtered columns to avoid full scans.",
            "Index Scan" => "Consider a more selective composite index to encourage seek operations.",
            "Hash Match" => "Verify join columns are indexed and statistics are current.",
            "Sort" => "Create an index aligned with ORDER BY or GROUP BY to remove the sort.",
            _ => "Review this operator for indexing or query-shape improvements."
        };

    private static string Encode(string? value) => WebUtility.HtmlEncode(value ?? string.Empty);
}
