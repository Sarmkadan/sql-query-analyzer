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
