#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SqlQueryAnalyzer.Constants;

/// <summary>
/// Types of performance issues that can be detected
/// </summary>
public enum IssueType
{
    /// <summary>Tables being scanned without proper indexes</summary>
    TableScan = 1,

    /// <summary>N+1 query problem detected</summary>
    NPlusOne = 2,

    /// <summary>Missing index opportunity</summary>
    MissingIndex = 3,

    /// <summary>Unused or redundant indexes</summary>
    UnusedIndex = 4,

    /// <summary>Implicit type conversion in WHERE clause</summary>
    ImplicitConversion = 5,

    /// <summary>Non-SARGable predicate</summary>
    NonSargable = 6,

    /// <summary>Inefficient join condition</summary>
    IneffectiveJoin = 7,

    /// <summary>Cartesian product / cross join</summary>
    CrossJoin = 8,

    /// <summary>OR condition that prevents index usage</summary>
    OrCondition = 9,

    /// <summary>Subquery that could be optimized</summary>
    SubqueryOptimization = 10,

    /// <summary>Index fragmentation</summary>
    IndexFragmentation = 11,

    /// <summary>Missing statistics</summary>
    MissingStatistics = 12,

    /// <summary>Outdated statistics</summary>
    OutdatedStatistics = 13,

    /// <summary>Function on indexed column</summary>
    FunctionOnColumn = 14,

    /// <summary>LIKE with leading wildcard</summary>
    LeadingWildcard = 15,

    /// <summary>SELECT * inefficiency</summary>
    SelectStar = 16,

    /// <summary>Large result set without pagination</summary>
    LargeResultSet = 17,

    /// <summary>Join on different data types</summary>
    JoinOnDifferentTypes = 18,

    /// <summary>Inefficient UNION instead of UNION ALL</summary>
    InefficientUnion = 19,

    /// <summary>Clustered index on non-unique column</summary>
    ClusteredIndexIssue = 20
}

/// <summary>
/// Severity levels for issues
/// </summary>
public enum IssueSeverity
{
    Info = 0,
    Warning = 1,
    Critical = 2
}

/// <summary>
/// Query complexity levels
/// </summary>
public enum QueryComplexity
{
    Simple = 0,
    Low = 1,
    Medium = 2,
    High = 3,
    VeryHigh = 4,
    Extreme = 5
}

/// <summary>
/// Analysis result status
/// </summary>
public enum AnalysisStatus
{
    Pending = 0,
    InProgress = 1,
    Completed = 2,
    Failed = 3,
    Cancelled = 4
}
