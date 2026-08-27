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
