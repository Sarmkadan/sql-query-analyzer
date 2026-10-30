## AnalysisEventPublisherTests

The `AnalysisEventPublisherTests` class contains unit tests for the `AnalysisEventPublisher` class. It verifies subscription management, event publishing, and error handling scenarios.

Here's an example of how to use it:
```csharp
var publisher = new AnalysisEventPublisher();
var subscriber1 = new Mock<IAnalysisEventSubscriber>().Object;
var subscriber2 = new Mock<IAnalysisEventSubscriber>().Object;
publisher.Subscribe(subscriber1);
publisher.Subscribe(subscriber2);
var @event = new AnalysisStartedEvent { QueryId = "test-query-123" };
await publisher.PublishAsync(@event);
subscriber1Mock.Verify(s => s.OnEventAsync(@event), Times.Once);
subscriber2Mock.Verify(s => s.OnEventAsync(@event), Times.Once);
```
## QueryComplexityScorerTests

The `QueryComplexityScorerTests` class contains unit tests for the `QueryComplexityScorer` static class. It verifies that the complexity score correctly combines the distinct table count with weighted contributions from table-scan, missing-index, N+1, and subquery issues, covering empty/whitespace queries, single- and multi-table selects, and combinations of all scoring factors.

Here's an example of how to use it:
```csharp
var result = new QueryAnalysisResult
{
    Query = "SELECT * FROM Users u WHERE u.Id IN (SELECT UserId FROM Orders)",
    Issues =
    [
        new PerformanceIssue { IssueType = IssueType.TableScan },
        new PerformanceIssue { IssueType = IssueType.MissingIndex }
    ]
};

var score = QueryComplexityScorer.ComputeScore(result);
// score = 1 (Users) + 5 (table scan) + 3 (missing index) + 2 (subquery) = 11
```
## CartesianJoinPluginTests

The `CartesianJoinPluginTests` class contains unit tests for the cartesian join detection plugin. It verifies that implicit cartesian products (comma-separated table lists) and explicit `CROSS JOIN` usage — including lower-case spellings and occurrences inside single-line comments — cause the corresponding issue to be added to the analysis result, while proper `INNER`/`LEFT` joins, single-table queries, multi-line comments, and queries without a `FROM` clause do not. It also covers plugin lifecycle and shutdown, metadata correctness, disabled-plugin behavior, multiple issues for combined implicit and explicit cross joins, and safe handling of null or empty queries.

Here's an example of how to use it:
```csharp
var tests = new CartesianJoinPluginTests();

await tests.ProcessAsync_QueryWithCommaSeparatedTables_AddsIssue();
await tests.ProcessAsync_QueryWithExplicitCrossJoin_AddsIssue();
await tests.ProcessAsync_QueryWithCrossJoinInComment_StillDetectsIssue();
await tests.ProcessAsync_QueryWithProperInnerJoin_DoesNotAddIssue();
await tests.ProcessAsync_QueryWithBothImplicitAndExplicitCrossJoin_AddsTwoIssues();
await tests.InitializeAsync_And_ShutdownAsync_ShouldNotThrow();
tests.PluginMetadata_IsCorrect();
```
## ErrorHandlingMiddlewareTests

The `ErrorHandlingMiddlewareTests` class contains unit tests for the error handling middleware. It verifies retry semantics around transient analysis exceptions (including fallback to default values when a connection is supplied), immediate propagation of non-transient failures, logging and rethrowing of generic exceptions, classification of transient error types, construction of complete and recoverable error reports with type-specific suggestions, error report formatting, and graceful degradation when primary operations fail.

Here's an example of how to use it:
```csharp
var tests = new ErrorHandlingMiddlewareTests();

var result = await tests.ExecuteWithErrorHandlingAsync_SuccessfulOperation_ReturnsResult();
await tests.ExecuteWithErrorHandlingAsync_TransientAnalysisException_RetriesThenSucceeds();
await tests.ExecuteWithErrorHandlingAsync_NonTransientAnalysisException_ThrowsImmediately();

tests.IsTransientError_TimeoutError_ReturnsTrue();
tests.CreateErrorReport_WithFileNotFoundException_HasCorrectSuggestion();

await tests.DegradationStrategy_ExecuteWithDegradationAsync_PrimaryFailsDegradedSucceeds();
```
## QueryNormalizerTests

The `QueryNormalizerTests` class contains unit tests for the `QueryNormalizer` utility class. It verifies that SQL normalization lowercases keywords while preserving string-literal case and escaped quotes, strips line and block comments, and collapses whitespace, and that table/column extraction and keyword detection behave correctly across `FROM`/`JOIN`/`INTO` clauses, `SELECT` columns, and the `*` wildcard.

Here's an example of how to use it:
```csharp
var tests = new QueryNormalizerTests();

tests.Normalize_StringLiteralInQuery_LiteralCaseIsPreserved();
tests.ExtractTableNames_QueryWithFromAndJoin_ReturnsBothTableNames();
tests.ExtractColumnNames_QueryWithSelectColumns_ReturnsColumnNames();
tests.IsSqlKeyword_RecognizesKeywords();
tests.IsSqlKeyword_NotAKeyword_ReturnsFalse();
```

## QueryNormalizerParameterizationTests

The `QueryNormalizerParameterizationTests` class contains unit tests for the `ToParameterizedQuery` method of `QueryNormalizer`. It verifies that numeric literals, string literals, decimal literals, and other literal values are replaced with `?` placeholders while preserving SQL structure, handling comments, whitespace normalization, and keyword case normalization.

Here's an example of how to use it:
```csharp
var tests = new QueryNormalizerParameterizationTests();

tests.ToParameterizedQuery_SimpleQueryWithNumericLiteral_ReplacesWithPlaceholder();
tests.ToParameterizedQuery_QueryWithStringLiteral_ReplacesWithPlaceholder();
tests.ToParameterizedQuery_QueryWithMixedLiterals_ReplacesAll();
tests.ToParameterizedQuery_QueryWithBlockComments_CommentsRemoved();
tests.ToParameterizedQuery_QueryWithKeywordsInMixedCase_NormalizesToLowercase();
tests.ToParameterizedQueryAndTrim_ExtensionMethod_TrimsResult();
```

## QueryNormalizer

The `QueryNormalizer` utility class normalizes SQL queries for consistent analysis: it removes redundant whitespace, lowercases keywords, strips comments, and collapses line breaks without changing query logic. It also exposes helpers for extracting table and column names, detecting SQL keywords, and producing parameterized query representations for comparison and caching.

Here's an example of how to use it:
```csharp
using SqlQueryAnalyzer.Utilities;

var normalizer = new QueryNormalizer();

var query = "SELECT Id, Name FROM Users WHERE Age > 21 AND Status = 'active'";

var normalized = normalizer.Normalize(query);
// "select id, name from users where age > 21 and status = 'active'"

var tables = normalizer.ExtractTableNames(query);
// ["Users"]

var columns = normalizer.ExtractColumnNames(query);
// ["Id", "Name"]

var isKeyword = QueryNormalizer.IsSqlKeyword("SELECT");
// true

var parameterized = normalizer.ToParameterizedQuery(query);
// "select id, name from users where age > ? and status = ?"
```
## AnalysisPipelineTests

The `AnalysisPipelineTests` class contains unit tests for the `AnalysisPipeline` class. It verifies the registration and execution of middlewares, including order, early termination, exception handling, logging, and integration with the analyzer service.

Here's an example of how to use it:
```csharp
var tests = new AnalysisPipelineTests();
tests.Constructor_RegistersDefaultMiddlewaresInCorrectOrder();
await tests.ExecuteAsync_ExecutesMiddlewaresInRegistrationOrder();
await tests.ExecuteAsync_StopsExecutionWhenShouldContinueIsFalse();
```

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Issue Types

The `IssueType` enum (`Constants/IssueTypes.cs`) lists every performance issue the analyzer can detect.

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

## Analysis Plugins

Detector plugins in `Plugins/` contribute `PerformanceIssue` entries to each analysis run.

- **`CartesianJoinPlugin`**: Flags Cartesian products: comma-separated tables in the FROM clause without join conditions, and explicit `CROSS JOIN`s.
- **`DistinctAbusePlugin`**: Flags `SELECT DISTINCT` over multiple joined tables as a possible join-fanout smell, and suggests checking that join keys are properly constrained.
- **`SelectStarPlugin`**: Flags `SELECT *` (case-insensitive) and recommends an explicit column list. `COUNT(*)` and stars inside comments or string literals are ignored.
- **`UnboundedOrderByPlugin`**: Flags `ORDER BY` without `TOP`, `LIMIT`, or `OFFSET ... FETCH`, which may indicate sorting the full result set.

## Default Configuration Values

The defaults below are defined in `Constants/AnalyzerDefaults.cs`. Each row lists the constant's group and what it controls.

| Name | Value | Meaning |
|---|---|---|
| `HighExecutionTimeThresholdMs` | `1000.0` | Performance thresholds: high execution time (ms) |
| `ModerateExecutionTimeThresholdMs` | `500.0` | Performance thresholds: moderate execution time (ms) |
| `HighLogicalReadsThreshold` | `100000` | Performance thresholds: high logical reads |
| `ModerateLogicalReadsThreshold` | `10000` | Performance thresholds: moderate logical reads |
| `HighPhysicalReadsThreshold` | `10000` | Performance thresholds: high physical reads |
| `FragmentationRebuildThreshold` | `30.0` | Index fragmentation: rebuild threshold |
| `FragmentationReorganizeThreshold` | `10.0` | Index fragmentation: reorganize threshold |
| `OptimalFragmentationThreshold` | `5.0` | Index fragmentation: optimal threshold |
| `SimpleComplexityLineCount` | `5` | Query complexity: line count for simple queries |
| `LowComplexityLineCount` | `10` | Query complexity: line count for low complexity |
| `MediumComplexityLineCount` | `20` | Query complexity: line count for medium complexity |
| `HighComplexityLineCount` | `50` | Query complexity: line count for high complexity |
| `SimpleComplexityTableCount` | `1` | Query complexity: table count for simple queries |
| `LowComplexityTableCount` | `2` | Query complexity: table count for low complexity |
| `MediumComplexityTableCount` | `3` | Query complexity: table count for medium complexity |
| `HighComplexityTableCount` | `5` | Query complexity: table count for high complexity |
| `CriticalIssuePenalty` | `10.0` | Issue severity scoring: penalty per critical issue |
| `WarningIssuePenalty` | `5.0` | Issue severity scoring: penalty per warning issue |
| `InfoIssuePenalty` | `2.0` | Issue severity scoring: penalty per info issue |
| `OptimizationPotentialBonus` | `0.1` | Issue severity scoring: bonus for optimization potential |
| `MinIndexPerformanceGain` | `5.0` | Index recommendations: minimum performance gain |
| `MinIndexAffectedQueries` | `1` | Index recommendations: minimum number of affected queries |
| `MaxIndexMaintenanceCostRatio` | `0.3` | Index recommendations: maximum maintenance cost ratio |
| `StatisticsStalenessDays` | `7` | Statistics update frequency: days until statistics are stale |
| `StatisticsCheckIntervalDays` | `1` | Statistics update frequency: days between checks |
| `DefaultAnalysisTimeoutSeconds` | `60` | Analysis timeouts: default analysis timeout (seconds) |
| `BatchAnalysisTimeoutSeconds` | `300` | Analysis timeouts: batch analysis timeout (seconds) |
| `IndexAnalysisTimeoutSeconds` | `30` | Analysis timeouts: index analysis timeout (seconds) |
| `IssueWeightCritical` | `1.0` | Performance score weights: weight for critical issues |
| `IssueWeightWarning` | `0.5` | Performance score weights: weight for warning issues |
| `IssueWeightInfo` | `0.1` | Performance score weights: weight for info issues |
| `OptimizationWeight` | `0.8` | Performance score weights: weight for optimization |
| `MaxIssuesInReport` | `50` | Report generation: maximum issues in a report |
| `MaxSuggestionsInReport` | `10` | Report generation: maximum suggestions in a report |
| `ReportSummaryLineLength` | `80` | Report generation: report summary line length |
| `SqlServerVersion` | `"2016+"` | Database compatibility: SQL Server version |
| `PostgreSqlVersion` | `"12+"` | Database compatibility: PostgreSQL version |
| `MySqlVersion` | `"5.7+"` | Database compatibility: MySQL version |
| `AnalysisCacheExpirationMinutes` | `60` | Cache settings: analysis cache expiration (minutes) |
| `MaxCachedAnalyses` | `10000` | Cache settings: maximum cached analyses |
| `MaxBatchSize` | `100` | Batch processing: maximum batch size |
| `DefaultBatchSize` | `10` | Batch processing: default batch size |
| `MaxConcurrentAnalyses` | `4` | Batch processing: maximum concurrent analyses |
| `MaxQuerySizeBytes` | `1048576` | Resource limits: maximum query size in bytes (1 MB) |
| `MaxQueryStatements` | `1000` | Resource limits: maximum statements per query |
| `MaxQueryParameters` | `500` | Resource limits: maximum parameters per query |
| `ScoringRanges.Excellent` | `90.0` | Default scoring ranges: excellent |
| `ScoringRanges.Good` | `75.0` | Default scoring ranges: good |
| `ScoringRanges.Acceptable` | `60.0` | Default scoring ranges: acceptable |
| `ScoringRanges.Poor` | `40.0` | Default scoring ranges: poor |
| `ScoringRanges.Critical` | `20.0` | Default scoring ranges: critical |
| `DetectionSensitivity.Low` | `1` | Detection sensitivity level: low |
| `DetectionSensitivity.Medium` | `2` | Detection sensitivity level: medium |
| `DetectionSensitivity.High` | `3` | Detection sensitivity level: high |
| `DetectionSensitivity.VeryHigh` | `4` | Detection sensitivity level: very high |
| `DefaultSensitivity` | `DetectionSensitivity.High` | Default detection sensitivity |
| `ErrorCodes.InvalidQuery` | `"ERR_INVALID_QUERY"` | Error code: invalid query |
| `ErrorCodes.AnalysisFailed` | `"ERR_ANALYSIS_FAILED"` | Error code: analysis failed |
| `ErrorCodes.ConnectionError` | `"ERR_CONNECTION"` | Error code: connection error |
| `ErrorCodes.Timeout` | `"ERR_TIMEOUT"` | Error code: timeout |
| `ErrorCodes.ConfigError` | `"ERR_CONFIG"` | Error code: configuration error |
| `ErrorCodes.DatabaseError` | `"ERR_DATABASE"` | Error code: database error |
| `SuccessCodes.AnalysisComplete` | `"OK_ANALYSIS_COMPLETE"` | Success code: analysis complete |
| `SuccessCodes.NoIssuesFound` | `"OK_NO_ISSUES"` | Success code: no issues found |
| `SuccessCodes.SuggestionsProvided` | `"OK_SUGGESTIONS"` | Success code: suggestions provided |
