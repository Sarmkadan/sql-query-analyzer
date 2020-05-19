#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text;

namespace SqlQueryAnalyzer.Models;

/// <summary>
/// Fully rendered textual representation of a <see cref="QueryPlan"/>.
/// Produced by the execution plan visualizer and embedded in <see cref="QueryProfilerReport"/>.
/// </summary>
public sealed class PlanVisualization
{
    /// <summary>ASCII tree rendering of all plan nodes with cost annotations.</summary>
    public string TextTree { get; set; } = string.Empty;

    /// <summary>Horizontal bar chart showing relative cost per table access operation.</summary>
    public string CostDistribution { get; set; } = string.Empty;

    /// <summary>Annotated bottleneck nodes ordered by estimated cost descending.</summary>
    public List<BottleneckAnnotation> Bottlenecks { get; set; } = [];

    /// <summary>Scalar summary statistics extracted from the rendered plan.</summary>
    public Dictionary<string, object> Stats { get; set; } = [];

    /// <summary>UTC timestamp when this visualization was produced.</summary>
    public DateTime RenderedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Returns <c>true</c> when at least one bottleneck was identified.</summary>
    public bool HasBottlenecks => Bottlenecks.Count > 0;

    /// <summary>
    /// Combines the tree, bottleneck list, and cost distribution into a single text block
    /// suitable for embedding in log output or CI reports.
    /// </summary>
    public string ToCompactReport()
    {
        var sb = new StringBuilder();
        sb.AppendLine(TextTree);

        if (Bottlenecks.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine($"Bottlenecks ({Bottlenecks.Count}):");
            foreach (var b in Bottlenecks)
                sb.AppendLine($"  • [{b.NodeType}] {b.ObjectName} — {b.Recommendation}");
        }

        sb.AppendLine();
        sb.Append(CostDistribution);
        return sb.ToString();
    }
}

/// <summary>
/// Identifies a high-cost operator in the execution plan and provides a targeted recommendation.
/// </summary>
public sealed class BottleneckAnnotation
{
    /// <summary>Unique node identifier from the plan tree.</summary>
    public string NodeId { get; set; } = string.Empty;

    /// <summary>Operator type (e.g., Table Scan, Hash Match, Sort).</summary>
    public string NodeType { get; set; } = string.Empty;

    /// <summary>Object (table or index) the operator acts upon, if applicable.</summary>
    public string ObjectName { get; set; } = string.Empty;

    /// <summary>Estimated cost assigned by the query optimizer to this operator.</summary>
    public double EstimatedCost { get; set; }

    /// <summary>Depth of this node within the plan tree (root = 0).</summary>
    public int Depth { get; set; }

    /// <summary>Specific, actionable recommendation for eliminating this bottleneck.</summary>
    public string Recommendation { get; set; } = string.Empty;

    /// <inheritdoc/>
    public override string ToString() =>
        $"[{NodeType}] depth={Depth} cost={EstimatedCost:F4} → {Recommendation}";
}
