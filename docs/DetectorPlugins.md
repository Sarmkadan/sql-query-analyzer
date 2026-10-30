# DetectorPlugins

`DetectorPlugins.cs` contains the six built-in rule implementations of [`IDetectorPlugin`](IDetectorPlugin.md). Each class checks one performance rule against a `DatabaseQuery` and yields zero or more `PerformanceIssue` findings. The classes are stateless (apart from the severity thresholds passed to `NonSargablePredicateDetectorPlugin`), so a single instance can be shared and called concurrently for different queries.

The plugins are registered by `PerformanceIssueDetectorService`, which runs them in this order and merges their findings:

1. `SelectStarDetectorPlugin`
2. `MissingWhereOrLimitDetectorPlugin`
3. `CartesianJoinDetectorPlugin`
4. `JoinColumnTypeDetectorPlugin`
5. `NonSargablePredicateDetectorPlugin`
6. `ImplicitConversionDetectorPlugin`

Every plugin in this file:

- Returns a lazily evaluated sequence (`yield return`), so findings are produced only as the caller enumerates it.
- Throws `ArgumentNullException` when `query` is null. Because `Analyze` is an iterator, the exception is thrown on the first enumeration, not on the call itself.
- Uses the `RuleId` value as the stable rule identifier for toggling and correlation.

## API

### `SelectStarDetectorPlugin`
`public sealed class SelectStarDetectorPlugin : IDetectorPlugin`

Flags `SELECT *` usage, which can retrieve unneeded columns and add I/O, memory, and network cost.

- **`RuleId`**: `"select-star"`
- **`Analyze(DatabaseQuery query)`**: Yields one issue when `QueryText` contains the literal text `SELECT *` (case-insensitive). The issue has `IssueType.SelectStar`, `IssueSeverity.Info`, `Priority` 3, and an estimated impact of 10.0. The check is a plain substring match, so `SELECT t.*` is not detected.

### `MissingWhereOrLimitDetectorPlugin`
`public sealed class MissingWhereOrLimitDetectorPlugin : IDetectorPlugin`

Detects two related problems:

- A statement beginning with `DELETE` or `UPDATE` that has no `WHERE` text. This yields a single `Critical` issue (`IssueType.TableScan`, `Priority` 1, impact 100.0) and stops further checks.
- A statement beginning with `SELECT` that has no bound on the result set. A bound is any of: `WHERE`, `TOP n`, `LIMIT n`, or `FETCH NEXT n ROWS ONLY`. This yields a `Warning` issue (`IssueType.TableScan`, `Priority` 2, impact 40.0).

The statement-type checks are anchored to the start of the text (`^\s*`), and the `WHERE` check is a case-insensitive substring match.

- **`RuleId`**: `"missing-where-limit"`
- **`Analyze(DatabaseQuery query)`**: Yields at most one issue, as described above.

### `CartesianJoinDetectorPlugin`
`public sealed class CartesianJoinDetectorPlugin : IDetectorPlugin`

Detects a likely Cartesian product by comparing the number of join conditions with the number of referenced tables. Fewer than `max(1, ReferencedTables.Count - 1)` join conditions produces a `Critical` issue (`IssueType.CrossJoin`, `Priority` 1, impact 90.0).

- **`RuleId`**: `"cartesian-join"`
- **`Analyze(DatabaseQuery query)`**: Uses `JoinConditions` and `ReferencedTables` only; `QueryText` is not inspected.

### `JoinColumnTypeDetectorPlugin`
`public sealed partial class JoinColumnTypeDetectorPlugin : IDetectorPlugin`

Finds column-to-column comparisons in join conditions and asks for a manual data-type compatibility check. The comparisons are located with a source-generated regex, `(\w+)\s*=\s*(\w+)`. This is a heuristic: the plugin does not know the column types.

- **`RuleId`**: `"join-column-type"`
- **`Analyze(DatabaseQuery query)`**: Yields one `Warning` issue (`IssueType.IneffectiveJoin`, `Priority` 2, impact 15.0) when `JoinConditions` contains at least one match. The description reports the number of matches.

### `NonSargablePredicateDetectorPlugin`
`public sealed partial class NonSargablePredicateDetectorPlugin : IDetectorPlugin`

Detects predicates that typically prevent index seeks. It checks three patterns:

1. A function call on a value in `QueryText` (`UPPER`, `LOWER`, `CONVERT`, `CAST`, `DATEPART`, `YEAR`, `MONTH`, `DAY`, case-insensitive). This yields a `Warning` issue (`IssueType.FunctionOnColumn`, impact 25.0). The regex matches any such call, so it can flag a function applied to a literal rather than a column.
2. An ` OR ` in the `WHERE` conditions. Severity is resolved from `IndexSeverityThresholds` with an estimated cost of 20.0 (`IssueType.OrCondition`).
3. `LIKE '%` (leading wildcard) in the `WHERE` conditions. Severity is resolved from `IndexSeverityThresholds` with an estimated cost of 30.0 (`IssueType.LeadingWildcard`).

Checks 2 and 3 run only when `WhereConditions` is non-empty.

- **`RuleId`**: `"non-sargable-predicate"`
- **`Analyze(DatabaseQuery query)`**: Yields zero to three issues, as described above.

#### `NonSargablePredicateDetectorPlugin(IndexSeverityThresholds indexSeverity)`
`public NonSargablePredicateDetectorPlugin(IndexSeverityThresholds indexSeverity)`

Creates the plugin with the thresholds used to grade OR and leading-wildcard findings.

- **Parameters**: `indexSeverity` - the thresholds (see `IndexSeverityThresholds` in `Configuration/AnalyzerSettings.cs`).
- **Exceptions**: `ArgumentNullException` when `indexSeverity` is null.

This is the only plugin in the file that takes a constructor argument. The other five have an implicit parameterless constructor.

### `ImplicitConversionDetectorPlugin`
`public sealed class ImplicitConversionDetectorPlugin : IDetectorPlugin`

A broad, low-cost hint that an equality comparison may involve an implicit type conversion. It fires whenever `QueryText` contains any `=` character, so it is informational rather than precise.

- **`RuleId`**: `"implicit-conversion"`
- **`Analyze(DatabaseQuery query)`**: Yields one `Info` issue (`IssueType.ImplicitConversion`, `Priority` 3, impact 5.0) when `=` is present.

## Adding or changing a rule

- Implement `IDetectorPlugin` in a new type, keep `Analyze` free of side effects, and give it a unique `RuleId`.
- Register it in the plugin collection used by `PerformanceIssueDetectorService`. No change to the orchestrator's merge logic is needed.
- Add a test class named after the type (for example `SelectStarDetectorPluginTests`) under `tests/sql-query-analyzer.Tests/`.
