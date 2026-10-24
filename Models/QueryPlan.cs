#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Generic;
using System.Linq;

namespace SqlQueryAnalyzer.Models;

/// <summary>
/// Represents a parsed SQL execution plan
/// </summary>
public sealed class QueryPlan
{
    /// <summary>
    /// Gets or sets the plan ID.
    /// </summary>
    public string PlanId { get; set; } = Guid.NewGuid().ToString();
    /// <summary>
    /// Gets or sets the database name.
    /// </summary>
    public string DatabaseName { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the captured at.
    /// </summary>
    public DateTime CapturedAt { get; set; } = DateTime.UtcNow;
    /// <summary>
    /// Gets or sets a value indicating whether is estimated.
    /// </summary>
    public bool IsEstimated { get; set; } // true for EXPLAIN PLAN, false for actual execution
    /// <summary>
    /// Gets or sets the format.
    /// </summary>
    public PlanFormat Format { get; set; } = PlanFormat.Unknown;

    // Plan tree structure
    /// <summary>
    /// Gets or sets the root node.
    /// </summary>
    public PlanNode? RootNode { get; set; }

    // Summary statistics
    /// <summary>
    /// Gets or sets the total estimated cost.
    /// </summary>
    public double TotalEstimatedCost { get; set; }
    /// <summary>
    /// Gets or sets the total estimated IO cost.
    /// </summary>
    public double TotalEstimatedIoCost { get; set; }
    /// <summary>
    /// Gets or sets the total estimated CPU cost.
    /// </summary>
    public double TotalEstimatedCpuCost { get; set; }
    /// <summary>
    /// Gets or sets the total estimated rows.
    /// </summary>
    public int TotalEstimatedRows { get; set; }
    /// <summary>
    /// Gets or sets the total elapsed time.
    /// </summary>
    public TimeSpan TotalElapsedTime { get; set; }
    /// <summary>
    /// Gets or sets the total logical reads.
    /// </summary>
    public long TotalLogicalReads { get; set; }
    /// <summary>
    /// Gets or sets the total physical reads.
    /// </summary>
    public long TotalPhysicalReads { get; set; }

    // Tree analysis
    /// <summary>
    /// Gets or sets the all nodes.
    /// </summary>
    public List<PlanNode> AllNodes { get; set; } = [];
    /// <summary>
    /// Gets or sets the table accesses.
    /// </summary>
    public List<TableAccess> TableAccesses { get; set; } = [];
    /// <summary>
    /// Gets or sets the joins.
    /// </summary>
    public List<Join> Joins { get; set; } = [];

    // Initialize plan structure
    /// <summary>
    /// Initialize.
    /// </summary>
    public void Initialize()
    {
        AllNodes = [];
        TableAccesses = [];
        Joins = [];

        if (RootNode != null)
        {
            TraverseTree(RootNode);
        }
    }

    // Traverse the plan tree and collect information
    private void TraverseTree(PlanNode node)
    {
        AllNodes.Add(node);

        if (node.NodeType == "Table Scan" || node.NodeType == "Index Scan" || node.NodeType == "Index Seek")
        {
            TableAccesses.Add(new TableAccess
            {
                TableName = node.ObjectName,
                AccessMethod = node.NodeType,
                EstimatedRows = node.EstimatedRows,
                EstimatedCost = node.EstimatedCost
            });
        }

        if (node.NodeType.Contains("Join"))
        {
            Joins.Add(new Join
            {
                JoinType = node.NodeType,
                EstimatedRows = node.EstimatedRows,
                EstimatedCost = node.EstimatedCost
            });
        }

        foreach (var child in node.Children)
        {
            TraverseTree(child);
        }
    }

    // Get most expensive operations
    /// <summary>
    /// Get expensive operations.
    /// </summary>
    public List<PlanNode> GetExpensiveOperations(int topN = 5) =>
        AllNodes.OrderByDescending(n => n.EstimatedCost).Take(topN).ToList();

    // Get all table scans (potential issue)
    /// <summary>
    /// Get table scans.
    /// </summary>
    public List<PlanNode> GetTableScans() =>
        AllNodes.Where(n => n.NodeType == "Table Scan").ToList();

    // Get all index operations
    /// <summary>
    /// Get index operations.
    /// </summary>
    public List<PlanNode> GetIndexOperations() =>
        AllNodes.Where(n => n.NodeType.Contains("Index")).ToList();

    // Detect missing indexes based on plan
    /// <summary>
    /// Detect missing indexes.
    /// </summary>
    public List<string> DetectMissingIndexes() =>
        GetTableScans().Where(ts => !string.IsNullOrEmpty(ts.ObjectName))
            .Select(ts => $"Consider adding index on {ts.ObjectName}")
            .ToList();

    // Export plan summary
    /// <summary>
    /// To summary.
    /// </summary>
    public Dictionary<string, object> ToSummary() =>
        new()
        {
            { "format", Format.ToString() },
            { "totalCost", TotalEstimatedCost },
            { "estimatedRows", TotalEstimatedRows },
            { "nodeCount", AllNodes.Count },
            { "tableAccessCount", TableAccesses.Count },
            { "joinCount", Joins.Count },
            { "tableScans", GetTableScans().Count }
        };
}

/// <summary>
/// Represents a single node in the execution plan tree
/// </summary>
public sealed class PlanNode
{
    /// <summary>
    /// Gets or sets the node ID.
    /// </summary>
    public string NodeId { get; set; } = Guid.NewGuid().ToString();
    /// <summary>
    /// Gets or sets the node type.
    /// </summary>
    public string NodeType { get; set; } = string.Empty; // Table Scan, Index Seek, etc.
    /// <summary>
    /// Gets or sets the object name.
    /// </summary>
    public string ObjectName { get; set; } = string.Empty; // Table or index name
    /// <summary>
    /// Gets or sets the depth.
    /// </summary>
    public int Depth { get; set; }
    /// <summary>
    /// Gets or sets the estimated cost.
    /// </summary>
    public double EstimatedCost { get; set; }
    /// <summary>
    /// Gets or sets the estimated IO cost.
    /// </summary>
    public double EstimatedIoCost { get; set; }
    /// <summary>
    /// Gets or sets the estimated CPU cost.
    /// </summary>
    public double EstimatedCpuCost { get; set; }
    /// <summary>
    /// Gets or sets the estimated rows.
    /// </summary>
    public int EstimatedRows { get; set; }
    /// <summary>
    /// Gets or sets the estimated row size.
    /// </summary>
    public double EstimatedRowSize { get; set; }
    /// <summary>
    /// Gets or sets the children.
    /// </summary>
    public List<PlanNode> Children { get; set; } = [];
    /// <summary>
    /// Gets or sets the properties.
    /// </summary>
    public Dictionary<string, string> Properties { get; set; } = [];
}

/// <summary>
/// Represents table access information from a plan
/// </summary>
public sealed class TableAccess
{
    /// <summary>
    /// Gets or sets the table name.
    /// </summary>
    public string TableName { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the access method.
    /// </summary>
    public string AccessMethod { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the estimated rows.
    /// </summary>
    public int EstimatedRows { get; set; }
    /// <summary>
    /// Gets or sets the estimated cost.
    /// </summary>
    public double EstimatedCost { get; set; }
}

/// <summary>
/// Represents join information from a plan
/// </summary>
public sealed class Join
{
    /// <summary>
    /// Gets or sets the join type.
    /// </summary>
    public string JoinType { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the estimated rows.
    /// </summary>
    public int EstimatedRows { get; set; }
    /// <summary>
    /// Gets or sets the estimated cost.
    /// </summary>
    public double EstimatedCost { get; set; }
}

/// <summary>
/// Format of the execution plan
/// </summary>
public enum PlanFormat
{
    /// <summary>
    /// The unknown.
    /// </summary>
    Unknown = 0,
    /// <summary>
    /// The SQL server.
    /// </summary>
    SqlServer = 1,
    /// <summary>
    /// The postgre SQL.
    /// </summary>
    PostgreSQL = 2,
    /// <summary>
    /// The my SQL.
    /// </summary>
    MySql = 3,
    /// <summary>
    /// The oracle.
    /// </summary>
    Oracle = 4,
    /// <summary>
    /// The JSON.
    /// </summary>
    Json = 5
}
