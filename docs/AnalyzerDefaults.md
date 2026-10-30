# AnalyzerDefaults

The `AnalyzerDefaults` static class (`Constants/AnalyzerDefaults.cs`, namespace `SqlQueryAnalyzer.Constants`) holds the default configuration values used by the SQL Query Analyzer. It groups thresholds for performance issues, index fragmentation, query complexity, issue scoring, index recommendations, statistics maintenance, timeouts, report generation, resource limits, error and success codes, and database version compatibility. All members are `public const` (except the nested `ScoringRanges`, `DetectionSensitivity`, `ErrorCodes`, and `SuccessCodes` types), so they are compile-time constants and can be used in attributes and switch labels.

> **Note:** As of this writing, no other code in the repository references `AnalyzerDefaults`. The purposes below are taken from each member's name and XML comment and describe the intended use.

## Performance thresholds

| Constant | Type | Value | Purpose |
|----------|------|-------|---------|
| `HighExecutionTimeThresholdMs` | `double` | `1000.0` | Execution time (ms) above which a query is considered high-cost. |
| `ModerateExecutionTimeThresholdMs` | `double` | `500.0` | Execution time (ms) above which a query is considered moderately slow. |
| `HighLogicalReadsThreshold` | `double` | `100000` | Logical read count above which a query is considered high-cost. |
| `ModerateLogicalReadsThreshold` | `double` | `10000` | Logical read count above which a query is considered moderately expensive. |
| `HighPhysicalReadsThreshold` | `double` | `10000` | Physical read count above which a query is considered high-cost. |

## Index fragmentation thresholds

Fragmentation values are percentages.

| Constant | Type | Value | Purpose |
|----------|------|-------|---------|
| `FragmentationRebuildThreshold` | `double` | `30.0` | Fragmentation (%) at or above which an index should be rebuilt. |
| `FragmentationReorganizeThreshold` | `double` | `10.0` | Fragmentation (%) at or above which an index should be reorganized. |
| `OptimalFragmentationThreshold` | `double` | `5.0` | Fragmentation (%) below which an index is considered healthy and needs no maintenance. |

## Query complexity thresholds

The line-count and table-count constants form four ascending levels: simple, low, medium, and high.

| Constant | Type | Value | Purpose |
|----------|------|-------|---------|
| `SimpleComplexityLineCount` | `int` | `5` | Maximum line count for a query classified as simple. |
| `LowComplexityLineCount` | `int` | `10` | Line count threshold for the low complexity level. |
| `MediumComplexityLineCount` | `int` | `20` | Line count threshold for the medium complexity level. |
| `HighComplexityLineCount` | `int` | `50` | Line count threshold for the high complexity level. |
| `SimpleComplexityTableCount` | `int` | `1` | Table count for a query classified as simple. |
| `LowComplexityTableCount` | `int` | `2` | Table count threshold for the low complexity level. |
| `MediumComplexityTableCount` | `int` | `3` | Table count threshold for the medium complexity level. |
| `HighComplexityTableCount` | `int` | `5` | Table count threshold for the high complexity level. |

## Issue severity scoring

| Constant | Type | Value | Purpose |
|----------|------|-------|---------|
| `CriticalIssuePenalty` | `double` | `10.0` | Score penalty applied for each critical issue. |
| `WarningIssuePenalty` | `double` | `5.0` | Score penalty applied for each warning. |
| `InfoIssuePenalty` | `double` | `2.0` | Score penalty applied for each informational issue. |
| `OptimizationPotentialBonus` | `double` | `0.1` | Bonus applied per unit of optimization potential. |

## Performance score weights

| Constant | Type | Value | Purpose |
|----------|------|-------|---------|
| `IssueWeightCritical` | `double` | `1.0` | Weight of critical issues in the performance score calculation. |
| `IssueWeightWarning` | `double` | `0.5` | Weight of warning issues in the performance score calculation. |
| `IssueWeightInfo` | `double` | `0.1` | Weight of informational issues in the performance score calculation. |
| `OptimizationWeight` | `double` | `0.8` | Weight of optimization potential in the performance score calculation. |

## Index recommendation thresholds

| Constant | Type | Value | Purpose |
|----------|------|-------|---------|
| `MinIndexPerformanceGain` | `double` | `5.0` | Minimum expected performance gain (%) before an index is recommended. |
| `MinIndexAffectedQueries` | `int` | `1` | Minimum number of queries an index must benefit before it is recommended. |
| `MaxIndexMaintenanceCostRatio` | `double` | `0.3` | Maximum ratio of maintenance cost to benefit; recommendations above it are rejected. |

## Statistics update frequency

| Constant | Type | Value | Purpose |
|----------|------|-------|---------|
| `StatisticsStalenessDays` | `int` | `7` | Age in days after which statistics are considered stale. |
| `StatisticsCheckIntervalDays` | `int` | `1` | Interval in days between statistics staleness checks. |

## Analysis timeouts

| Constant | Type | Value | Purpose |
|----------|------|-------|---------|
| `DefaultAnalysisTimeoutSeconds` | `int` | `60` | Default timeout (seconds) for a single analysis. |
| `BatchAnalysisTimeoutSeconds` | `int` | `300` | Timeout (seconds) for a batch analysis. |
| `IndexAnalysisTimeoutSeconds` | `int` | `30` | Timeout (seconds) for index analysis. |

## Report generation

| Constant | Type | Value | Purpose |
|----------|------|-------|---------|
| `MaxIssuesInReport` | `int` | `50` | Maximum number of issues included in a report. |
| `MaxSuggestionsInReport` | `int` | `10` | Maximum number of suggestions included in a report. |
| `ReportSummaryLineLength` | `int` | `80` | Line length used for report summary output. |

## Database compatibility

| Constant | Type | Value | Purpose |
|----------|------|-------|---------|
| `SqlServerVersion` | `string` | `"2016+"` | Minimum supported SQL Server version. |
| `PostgreSqlVersion` | `string` | `"12+"` | Minimum supported PostgreSQL version. |
| `MySqlVersion` | `string` | `"5.7+"` | Minimum supported MySQL version. |

## Cache settings

| Constant | Type | Value | Purpose |
|----------|------|-------|---------|
| `AnalysisCacheExpirationMinutes` | `int` | `60` | Lifetime (minutes) of a cached analysis result. |
| `MaxCachedAnalyses` | `int` | `10000` | Maximum number of analyses held in the cache. |

## Batch processing

| Constant | Type | Value | Purpose |
|----------|------|-------|---------|
| `MaxBatchSize` | `int` | `100` | Maximum number of queries in one batch. |
| `DefaultBatchSize` | `int` | `10` | Batch size used when none is specified. |
| `MaxConcurrentAnalyses` | `int` | `4` | Maximum number of analyses running concurrently. |

## Resource limits

| Constant | Type | Value | Purpose |
|----------|------|-------|---------|
| `MaxQuerySizeBytes` | `long` | `1048576` (1 MB) | Maximum size of a query in bytes. |
| `MaxQueryStatements` | `int` | `1000` | Maximum number of statements in a query. |
| `MaxQueryParameters` | `int` | `500` | Maximum number of parameters in a query. |

## Default detection sensitivity

`DefaultSensitivity` is a `DetectionSensitivity` value and defaults to `DetectionSensitivity.High`.

| Constant | Type | Value | Purpose |
|----------|------|-------|---------|
| `DefaultSensitivity` | `DetectionSensitivity` | `High` | Detection sensitivity used when none is configured. |

### `DetectionSensitivity` enum

| Member | Value | Description |
|--------|-------|-------------|
| `Low` | `1` | Low sensitivity. |
| `Medium` | `2` | Medium sensitivity. |
| `High` | `3` | High sensitivity. |
| `VeryHigh` | `4` | Very high sensitivity. |

## Scoring ranges

`AnalyzerDefaults.ScoringRanges` holds score boundaries on a 0–100 scale. The values are in descending order.

| Constant | Type | Value | Purpose |
|----------|------|-------|---------|
| `ScoringRanges.Excellent` | `double` | `90.0` | Lower bound of the excellent score range. |
| `ScoringRanges.Good` | `double` | `75.0` | Lower bound of the good score range. |
| `ScoringRanges.Acceptable` | `double` | `60.0` | Lower bound of the acceptable score range. |
| `ScoringRanges.Poor` | `double` | `40.0` | Lower bound of the poor score range. |
| `ScoringRanges.Critical` | `double` | `20.0` | Lower bound of the critical score range. |

## Error codes

`AnalyzerDefaults.ErrorCodes` holds machine-readable error identifiers.

| Constant | Type | Value | Purpose |
|----------|------|-------|---------|
| `ErrorCodes.InvalidQuery` | `string` | `"ERR_INVALID_QUERY"` | The submitted query is invalid. |
| `ErrorCodes.AnalysisFailed` | `string` | `"ERR_ANALYSIS_FAILED"` | Analysis failed. |
| `ErrorCodes.ConnectionError` | `string` | `"ERR_CONNECTION"` | Connection to the database failed. |
| `ErrorCodes.Timeout` | `string` | `"ERR_TIMEOUT"` | The operation timed out. |
| `ErrorCodes.ConfigError` | `string` | `"ERR_CONFIG"` | Configuration is invalid. |
| `ErrorCodes.DatabaseError` | `string` | `"ERR_DATABASE"` | The database returned an error. |

## Success codes

`AnalyzerDefaults.SuccessCodes` holds machine-readable success identifiers.

| Constant | Type | Value | Purpose |
|----------|------|-------|---------|
| `SuccessCodes.AnalysisComplete` | `string` | `"OK_ANALYSIS_COMPLETE"` | Analysis completed successfully. |
| `SuccessCodes.NoIssuesFound` | `string` | `"OK_NO_ISSUES"` | Analysis completed and found no issues. |
| `SuccessCodes.SuggestionsProvided` | `string` | `"OK_SUGGESTIONS"` | Analysis completed and produced suggestions. |
