#nullable enable

using System.Globalization;

namespace SqlQueryAnalyzer.Models;

/// <summary>
/// Provides extension methods for <see cref="PlanVisualization"/> to enable common operations
/// and queries on execution plan visualizations without modifying the original class.
/// </summary>
public static class PlanVisualizationExtensions
{
    /// <summary>
    /// Gets the total estimated cost across all bottlenecks in the plan.
    /// </summary>
    /// <param name="plan">The plan visualization instance.</param>
    /// <returns>The sum of all bottleneck estimated costs, or 0 if no bottlenecks exist.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="plan"/> is null.</exception>
    public static double GetTotalBottleneckCost(this PlanVisualization plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        return plan.Bottlenecks.Sum(b => b.EstimatedCost);
    }

    /// <summary>
    /// Gets the highest cost bottleneck in the plan.
    /// </summary>
    /// <param name="plan">The plan visualization instance.</param>
    /// <returns>The bottleneck annotation with the highest estimated cost, or null if no bottlenecks exist.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="plan"/> is null.</exception>
    public static BottleneckAnnotation? GetHighestCostBottleneck(this PlanVisualization plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        return plan.Bottlenecks.MaxBy(b => b.EstimatedCost);
    }

    /// <summary>
    /// Gets the average depth of all nodes in the plan.
    /// </summary>
    /// <param name="plan">The plan visualization instance.</param>
    /// <returns>The average depth as a double, or 0 if no bottlenecks exist.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="plan"/> is null.</exception>
    public static double GetAverageBottleneckDepth(this PlanVisualization plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        return plan.Bottlenecks.Count > 0
            ? plan.Bottlenecks.Average(b => b.Depth)
            : 0.0;
    }

    /// <summary>
    /// Gets the percentage of total cost represented by bottlenecks.
    /// </summary>
    /// <param name="plan">The plan visualization instance.</param>
    /// <returns>A value between 0 and 100 representing the percentage, or 0 if no bottlenecks exist.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="plan"/> is null.</exception>
    public static double GetBottleneckCostPercentage(this PlanVisualization plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        if (plan.Bottlenecks.Count == 0)
        {
            return 0.0;
        }

        double totalBottleneckCost = plan.GetTotalBottleneckCost();
        double totalPlanCost = plan.Stats.TryGetValue("TotalCost", out var totalCostObj) && totalCostObj is double totalCost
            ? totalCost
            : 1.0; // Default to 1.0 to avoid division by zero

        return (totalBottleneckCost / totalPlanCost) * 100.0;
    }

    /// <summary>
    /// Gets the most common node type among bottlenecks.
    /// </summary>
    /// <param name="plan">The plan visualization instance.</param>
    /// <returns>The most frequent node type, or an empty string if no bottlenecks exist.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="plan"/> is null.</exception>
    public static string GetMostCommonBottleneckNodeType(this PlanVisualization plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        return plan.Bottlenecks
            .GroupBy(b => b.NodeType)
            .MaxBy(g => g.Count())?.Key ?? string.Empty;
    }

    /// <summary>
    /// Gets the maximum depth found in any bottleneck.
    /// </summary>
    /// <param name="plan">The plan visualization instance.</param>
    /// <returns>The maximum depth value, or 0 if no bottlenecks exist.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="plan"/> is null.</exception>
    public static int GetMaxBottleneckDepth(this PlanVisualization plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        return plan.Bottlenecks.Max(b => b.Depth);
    }

    /// <summary>
    /// Gets the node types and their counts from all bottlenecks.
    /// </summary>
    /// <param name="plan">The plan visualization instance.</param>
    /// <returns>A dictionary mapping node types to their occurrence counts.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="plan"/> is null.</exception>
    public static IReadOnlyDictionary<string, int> GetBottleneckNodeTypeDistribution(this PlanVisualization plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        return plan.Bottlenecks
            .GroupBy(b => b.NodeType)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    /// <summary>
    /// Gets bottlenecks filtered by a specific node type.
    /// </summary>
    /// <param name="plan">The plan visualization instance.</param>
    /// <param name="nodeType">The node type to filter by (e.g., "Table Scan", "Hash Match").</param>
    /// <returns>An enumerable of matching bottlenecks.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="plan"/> is null.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="nodeType"/> is null.</exception>
    public static IEnumerable<BottleneckAnnotation> GetBottlenecksByNodeType(this PlanVisualization plan, string nodeType)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(nodeType);

        return plan.Bottlenecks.Where(b => string.Equals(b.NodeType, nodeType, StringComparison.Ordinal));
    }

    /// <summary>
    /// Gets bottlenecks with estimated cost above a specified threshold.
    /// </summary>
    /// <param name="plan">The plan visualization instance.</param>
    /// <param name="minCost">The minimum cost threshold (inclusive).</param>
    /// <returns>An enumerable of bottlenecks meeting the cost threshold.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="plan"/> is null.</exception>
    public static IEnumerable<BottleneckAnnotation> GetHighCostBottlenecks(this PlanVisualization plan, double minCost)
    {
        ArgumentNullException.ThrowIfNull(plan);

        return plan.Bottlenecks.Where(b => b.EstimatedCost >= minCost);
    }

    /// <summary>
    /// Gets bottlenecks at or below a specified depth.
    /// </summary>
    /// <param name="plan">The plan visualization instance.</param>
    /// <param name="maxDepth">The maximum depth threshold (inclusive).</param>
    /// <returns>An enumerable of bottlenecks at or below the specified depth.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="plan"/> is null.</exception>
    public static IEnumerable<BottleneckAnnotation> GetBottlenecksAtDepth(this PlanVisualization plan, int maxDepth)
    {
        ArgumentNullException.ThrowIfNull(plan);

        return plan.Bottlenecks.Where(b => b.Depth <= maxDepth);
    }

    /// <summary>
    /// Gets a formatted summary string for the plan visualization.
    /// </summary>
    /// <param name="plan">The plan visualization instance.</param>
    /// <returns>A formatted summary including render time, bottleneck count, and cost statistics.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="plan"/> is null.</exception>
    public static string ToSummaryString(this PlanVisualization plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Plan Visualization Summary (Rendered: {plan.RenderedAt:yyyy-MM-dd HH:mm:ss UTC})");
        sb.AppendLine($"Bottlenecks: {plan.Bottlenecks.Count}");
        sb.AppendLine($"Total Bottleneck Cost: {plan.GetTotalBottleneckCost():F4}");
        sb.AppendLine($"Bottleneck Cost Percentage: {plan.GetBottleneckCostPercentage():F2}%");
        sb.AppendLine($"Average Depth: {plan.GetAverageBottleneckDepth():F2}");
        sb.AppendLine($"Max Depth: {plan.GetMaxBottleneckDepth()}");
        sb.AppendLine($"Most Common Node Type: {plan.GetMostCommonBottleneckNodeType()}");

        if (plan.Bottlenecks.Count > 0)
        {
            var highest = plan.GetHighestCostBottleneck();
            sb.AppendLine($"Highest Cost Bottleneck: [{highest?.NodeType}] {highest?.ObjectName} (Cost: {highest?.EstimatedCost:F4}, Depth: {highest?.Depth})");
        }

        return sb.ToString();
    }
}