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
public class QueryPlan
{
    public string PlanId { get; set; } = Guid.NewGuid().ToString();
    public string DatabaseName { get; set; } = string.Empty;
    public DateTime CapturedAt { get; set; } = DateTime.UtcNow;
    public bool IsEstimated { get; set; } // true for EXPLAIN PLAN, false for actual execution
    public PlanFormat Format { get; set; } = PlanFormat.Unknown;

    // Plan tree structure
    public PlanNode? RootNode { get; set; }

    // Summary statistics
    public double TotalEstimatedCost { get; set; }
    public double TotalEstimatedIoCost { get; set; }
    public double TotalEstimatedCpuCost { get; set; }
    public int TotalEstimatedRows { get; set; }
    public TimeSpan TotalElapsedTime { get; set; }
    public long TotalLogicalReads { get; set; }
    public long TotalPhysicalReads { get; set; }

    // Tree analysis
    public List<PlanNode> AllNodes { get; set; } = [];
    public List<TableAccess> TableAccesses { get; set; } = [];
    public List<Join> Joins { get; set; } = [];

    // Initialize plan structure
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
    public List<PlanNode> GetExpensiveOperations(int topN = 5) =>
        AllNodes.OrderByDescending(n => n.EstimatedCost).Take(topN).ToList();

    // Get all table scans (potential issue)
    public List<PlanNode> GetTableScans() =>
        AllNodes.Where(n => n.NodeType == "Table Scan").ToList();

    // Get all index operations
    public List<PlanNode> GetIndexOperations() =>
        AllNodes.Where(n => n.NodeType.Contains("Index")).ToList();

    // Detect missing indexes based on plan
    public List<string> DetectMissingIndexes() =>
        TableScans.Where(ts => !string.IsNullOrEmpty(ts.ObjectName))
            .Select(ts => $"Consider adding index on {ts.ObjectName}")
            .ToList();

    // Export plan summary
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
public class PlanNode
{
    public string NodeId { get; set; } = Guid.NewGuid().ToString();
    public string NodeType { get; set; } = string.Empty; // Table Scan, Index Seek, etc.
    public string ObjectName { get; set; } = string.Empty; // Table or index name
    public int Depth { get; set; }
    public double EstimatedCost { get; set; }
    public double EstimatedIoCost { get; set; }
    public double EstimatedCpuCost { get; set; }
    public int EstimatedRows { get; set; }
    public double EstimatedRowSize { get; set; }
    public List<PlanNode> Children { get; set; } = [];
    public Dictionary<string, string> Properties { get; set; } = [];
}

/// <summary>
/// Represents table access information from a plan
/// </summary>
public class TableAccess
{
    public string TableName { get; set; } = string.Empty;
    public string AccessMethod { get; set; } = string.Empty;
    public int EstimatedRows { get; set; }
    public double EstimatedCost { get; set; }
}

/// <summary>
/// Represents join information from a plan
/// </summary>
public class Join
{
    public string JoinType { get; set; } = string.Empty;
    public int EstimatedRows { get; set; }
    public double EstimatedCost { get; set; }
}

/// <summary>
/// Format of the execution plan
/// </summary>
public enum PlanFormat
{
    Unknown = 0,
    SqlServer = 1,
    PostgreSQL = 2,
    MySql = 3,
    Oracle = 4,
    Json = 5
}
