# IssueType

The `IssueType` enum classifies the performance issues that the SQL Query Analyzer can detect. It is declared in `Constants/IssueTypes.cs` under the `SqlQueryAnalyzer.Constants` namespace. Each detected `PerformanceIssue` is tagged with one of these members so that results can be filtered, grouped, and scored consistently across analyzers, plugins, and output formats.

## Declaration

```csharp
namespace SqlQueryAnalyzer.Constants;

public enum IssueType
{
    TableScan = 1,
    NPlusOne = 2,
    // ... see the table below for all members
    ClusteredIndexIssue = 20
}
```

The underlying type is `int` (the C# default). Values start at `1`; `0` is intentionally not a valid member, so an unset `IssueType` is never mistaken for a real issue category.

## Members

| Name | Value | Description |
|------|-------|-------------|
| `TableScan` | 1 | Tables being scanned without proper indexes |
| `NPlusOne` | 2 | N+1 query problem detected |
| `MissingIndex` | 3 | Missing index opportunity |
| `UnusedIndex` | 4 | Unused or redundant indexes |
| `ImplicitConversion` | 5 | Implicit type conversion in WHERE clause |
| `NonSargable` | 6 | Non-SARGable predicate |
| `IneffectiveJoin` | 7 | Inefficient join condition |
| `CrossJoin` | 8 | Cartesian product / cross join |
| `OrCondition` | 9 | OR condition that prevents index usage |
| `SubqueryOptimization` | 10 | Subquery that could be optimized |
| `IndexFragmentation` | 11 | Index fragmentation |
| `MissingStatistics` | 12 | Missing statistics |
| `OutdatedStatistics` | 13 | Outdated statistics |
| `FunctionOnColumn` | 14 | Function on indexed column |
| `LeadingWildcard` | 15 | LIKE with leading wildcard |
| `SelectStar` | 16 | SELECT * inefficiency |
| `LargeResultSet` | 17 | Large result set without pagination |
| `JoinOnDifferentTypes` | 18 | Join on different data types |
| `InefficientUnion` | 19 | Inefficient UNION instead of UNION ALL |
| `ClusteredIndexIssue` | 20 | Clustered index on non-unique column |

## Notes

*   **Stable values:** Numeric values are part of the public contract. Persisted or serialized results that store `IssueType` as an integer depend on them, so existing values must not be renumbered. New members should be appended with the next unused value.
*   **Related enums:** Severity is modeled separately by `IssueSeverity` (`Info`, `Warning`, `Critical`) in the same file, and is not a property of `IssueType`.
*   **Detection:** Individual analyzers and detector plugins emit the members that correspond to their checks. For example, `SelectStarPlugin` emits `SelectStar` and `CartesianJoinPlugin` emits `CrossJoin`. As of this writing, `UnusedIndex`, `NonSargable`, `IndexFragmentation`, `MissingStatistics`, `OutdatedStatistics`, `JoinOnDifferentTypes`, `InefficientUnion`, and `ClusteredIndexIssue` are not referenced by any non-test code.
