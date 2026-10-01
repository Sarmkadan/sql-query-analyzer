#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using SqlQueryAnalyzer.Models;

namespace SqlQueryAnalyzer.Services;

/// <summary>
/// Reads and parses execution plan output from various database engines.
/// Supports JSON (PostgreSQL), XML (SQL Server), and text-based EXPLAIN output.
/// </summary>
public sealed class ExecutionPlanReader
{
    private readonly ILogger<ExecutionPlanReader> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="ExecutionPlanReader"/>.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    public ExecutionPlanReader(ILogger<ExecutionPlanReader> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Detects the format of the execution plan text (JSON, XML, or plain text).
    /// </summary>
    /// <param name="planText">Raw execution plan output.</param>
    /// <returns>The detected format as a string: "json", "xml", or "text".</returns>
    public string DetectFormat(string planText)
    {
        if (string.IsNullOrWhiteSpace(planText))
            return "text";

        var trimmed = planText.TrimStart();

        if (trimmed.StartsWith('[') || trimmed.StartsWith('{'))
            return "json";

        if (trimmed.StartsWith('<'))
            return "xml";

        return "text";
    }

    /// <summary>
    /// Parses a JSON execution plan (PostgreSQL EXPLAIN (FORMAT JSON) output)
    /// and extracts node types with their estimated costs.
    /// </summary>
    /// <param name="jsonPlan">JSON string from EXPLAIN.</param>
    /// <returns>List of plan node descriptions with cost info.</returns>
    public List<PlanNodeInfo> ParseJsonPlan(string jsonPlan)
    {
        var nodes = new List<PlanNodeInfo>();

        if (string.IsNullOrWhiteSpace(jsonPlan))
            return nodes;

        _logger.LogDebug("Parsing JSON execution plan");

        try
        {
            using var doc = JsonDocument.Parse(jsonPlan);
            var root = doc.RootElement;

            // PostgreSQL wraps in an array with a Plan object
            if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0)
            {
                var firstElement = root[0];
                if (firstElement.TryGetProperty("Plan", out var plan))
                {
                    TraverseJsonNode(plan, nodes, 0);
                }
            }
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse JSON execution plan");
        }

        return nodes;
    }

    /// <summary>
    /// Parses an XML execution plan (SQL Server SET SHOWPLAN_XML ON output).
    /// </summary>
    /// <param name="xmlPlan">XML string from SQL Server.</param>
    /// <returns>List of plan node descriptions.</returns>
    public List<PlanNodeInfo> ParseXmlPlan(string xmlPlan)
    {
        var nodes = new List<PlanNodeInfo>();

        if (string.IsNullOrWhiteSpace(xmlPlan))
            return nodes;

        _logger.LogDebug("Parsing XML execution plan");

        try
        {
            var xdoc = XDocument.Parse(xmlPlan);
            var ns = xdoc.Root?.GetDefaultNamespace() ?? XNamespace.None;

            var relOps = xdoc.Descendants(ns + "RelOp");
            foreach (var op in relOps)
            {
                nodes.Add(new PlanNodeInfo
                {
                    NodeType = op.Attribute("PhysicalOp")?.Value ?? "Unknown",
                    LogicalOp = op.Attribute("LogicalOp")?.Value,
                    EstimatedCost = double.TryParse(op.Attribute("EstimatedTotalSubtreeCost")?.Value,
                        System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out var c) ? c : 0,
                    EstimatedRows = double.TryParse(op.Attribute("EstimateRows")?.Value,
                        System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out var r) ? r : 0,
                    Depth = 0
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse XML execution plan");
        }

        return nodes;
    }

    /// <summary>
    /// Extracts the total estimated cost from a list of plan nodes.
    /// </summary>
    /// <param name="nodes">Parsed plan nodes.</param>
    /// <returns>The maximum cost across root-level nodes, or 0 if empty.</returns>
    public double GetTotalEstimatedCost(List<PlanNodeInfo> nodes)
    {
        if (nodes == null || nodes.Count == 0)
            return 0;

        return nodes.Where(n => n.Depth == 0).Select(n => n.EstimatedCost).DefaultIfEmpty(0).Max();
    }

    /// <summary>
    /// Identifies plan nodes that use sequential/table scans (potential performance issues).
    /// </summary>
    /// <param name="nodes">Parsed plan nodes.</param>
    /// <returns>List of nodes performing full scans.</returns>
    public List<PlanNodeInfo> FindFullScans(List<PlanNodeInfo> nodes)
    {
        if (nodes == null)
            return new List<PlanNodeInfo>();

        var scanTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Seq Scan", "Table Scan", "Clustered Index Scan", "Index Scan"
        };

        return nodes.Where(n => scanTypes.Contains(n.NodeType)).ToList();
    }

    private void TraverseJsonNode(JsonElement node, List<PlanNodeInfo> nodes, int depth)
    {
        var info = new PlanNodeInfo
        {
            Depth = depth,
            NodeType = node.TryGetProperty("Node Type", out var nt) ? nt.GetString() ?? "Unknown" : "Unknown",
            EstimatedCost = node.TryGetProperty("Total Cost", out var tc) ? tc.GetDouble() : 0,
            EstimatedRows = node.TryGetProperty("Plan Rows", out var pr) ? pr.GetDouble() : 0,
            RelationName = node.TryGetProperty("Relation Name", out var rn) ? rn.GetString() : null
        };

        nodes.Add(info);

        if (node.TryGetProperty("Plans", out var plans) && plans.ValueKind == JsonValueKind.Array)
        {
            foreach (var child in plans.EnumerateArray())
            {
                TraverseJsonNode(child, nodes, depth + 1);
            }
        }
    }
}

/// <summary>
/// Represents a single node in a parsed execution plan tree.
/// </summary>
public sealed class PlanNodeInfo
{
    /// <summary>The physical operation type (e.g., "Seq Scan", "Hash Join").</summary>
    public string NodeType { get; set; } = "Unknown";

    /// <summary>The logical operation, if available (SQL Server).</summary>
    public string? LogicalOp { get; set; }

    /// <summary>Estimated total cost for this subtree.</summary>
    public double EstimatedCost { get; set; }

    /// <summary>Estimated number of rows.</summary>
    public double EstimatedRows { get; set; }

    /// <summary>Depth in the plan tree (0 = root).</summary>
    public int Depth { get; set; }

    /// <summary>Table/relation name, if applicable.</summary>
    public string? RelationName { get; set; }
}
