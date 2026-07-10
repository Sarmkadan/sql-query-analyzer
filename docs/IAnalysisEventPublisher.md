# IAnalysisEventPublisher

The `IAnalysisEventPublisher` interface provides a mechanism for asynchronously broadcasting analysis events generated during the evaluation of SQL queries within the `sql-query-analyzer` system. It enables decoupling between the analysis engine and downstream consumers, such as loggers, telemetry collectors, or user interface components that require real-time updates on query performance, identified issues, or execution failures.

## API

### Methods

*   **`public AnalysisEventPublisher()`**
    Initializes a new instance of the analysis event publisher.

*   **`public void Subscribe(Action<AnalysisEvent> handler)`**
    Registers a delegate to be invoked when an event is published.
    *   `handler`: The action to perform when an event is received.

*   **`public void Unsubscribe(Action<AnalysisEvent> handler)`**
    Removes a previously registered delegate from the subscription list.
    *   `handler`: The action delegate to remove.

*   **`public async Task PublishAsync(AnalysisEvent event)`**
    Asynchronously publishes an event to all registered subscribers.
    *   `event`: The event object to be published.
    *   Throws: `ArgumentNullException` if the event is null.

### Properties

*   **`public DateTime Timestamp`**
    Gets the UTC timestamp when the event was generated.

*   **`public string CorrelationId`**
    Gets a unique identifier used for tracing and correlating events across different services or operations.

*   **`public Dictionary<string, object> Metadata`**
    Gets a dictionary containing additional contextual information associated with the event.

*   **`public string QueryId`**
    Gets the identifier of the query that generated the event. This identifier is used across performance, issue, and error event types.

*   **`public string Query`**
    Gets the raw SQL query string associated with the analysis.

*   **`public double PerformanceScore`**
    Gets the quantified performance metric assigned to the query during analysis.

*   **`public int IssuesFound`**
    Gets the total count of issues identified within the query.

*   **`public TimeSpan AnalysisDuration`**
    Gets the time taken to complete the analysis of the query.

*   **`public string IssueType`**
    Gets the classification or type of the specific issue identified.

*   **`public string Description`**
    Gets a detailed human-readable description of the event or identified issue.

*   **`public double ImpactPercentage`**
    Gets the estimated impact of the identified issue as a percentage of overall query performance.

*   **`public string ErrorMessage`**
    Gets the message string associated with a failure event.

*   **`public string ExceptionType`**
    Gets the type of the exception thrown during the analysis process, if applicable.

## Usage

### Example 1: Basic Subscription and Publishing
```csharp
var publisher = new AnalysisEventPublisher();

// Subscribe to events
publisher.Subscribe(evt => {
    Console.WriteLine($"Event generated at {evt.Timestamp}: {evt.Description}");
});

// Publish an event
await publisher.PublishAsync(new AnalysisEvent {
    Description = "Analysis started",
    Timestamp = DateTime.UtcNow
});
```

### Example 2: Handling Performance Events
```csharp
publisher.Subscribe(evt => {
    if (!string.IsNullOrEmpty(evt.QueryId) && evt.PerformanceScore > 0)
    {
        Console.WriteLine($"Query {evt.QueryId} analyzed in {evt.AnalysisDuration.TotalMilliseconds}ms with score {evt.PerformanceScore}.");
    }
});
```

## Notes

*   **Thread Safety**: Implementations of `IAnalysisEventPublisher` must ensure that subscription management (`Subscribe`/`Unsubscribe`) and event dispatching (`PublishAsync`) are thread-safe. Concurrent access to the subscriber list is expected.
*   **Subscriber Robustness**: Registered handlers should implement appropriate error handling to ensure that an exception within one subscriber does not prevent other subscribers from receiving the event or cause the `PublishAsync` task to fault prematurely.
*   **Event Structure**: While all properties are defined on the publisher type, certain properties may be null or default-valued depending on the specific event type (e.g., `ErrorMessage` will be populated for error events, but null for performance events). Consumers should check for relevant properties based on the event context.
