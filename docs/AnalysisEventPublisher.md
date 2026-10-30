# AnalysisEventPublisher

The `AnalysisEventPublisher` class is the default implementation of `IAnalysisEventPublisher`. It keeps an ordered list of `IAnalysisEventSubscriber` instances and delivers each published `AnalysisEvent` to them one at a time, in subscription order. A subscriber that throws does not stop delivery to the others, and a subscriber that keeps failing is removed automatically.

The class lives in the `SqlQueryAnalyzer.Events` namespace, alongside the event types (`AnalysisStartedEvent`, `AnalysisCompletedEvent`, `CriticalIssueDetectedEvent`, `AnalysisFailedEvent`) and the subscriber contract `IAnalysisEventSubscriber`.

## API

### Constructor

*   **`public AnalysisEventPublisher(ILogger<AnalysisEventPublisher>? logger = null)`**
    Creates a publisher with an empty subscriber list.
    *   `logger`: Optional logger for subscription changes, publish activity, and subscriber failures. When `null`, the publisher does not log.

### Methods

*   **`public void Subscribe(IAnalysisEventSubscriber subscriber)`**
    Adds a subscriber to the end of the dispatch list.
    *   `subscriber`: The subscriber to register. Must not be `null`.
    *   Subscribing the same instance twice is a no-op; it keeps its original position and is not notified twice per event.
    *   Throws: `ArgumentNullException` if `subscriber` is `null`.

*   **`public void Unsubscribe(IAnalysisEventSubscriber subscriber)`**
    Removes a subscriber from the dispatch list and clears its failure count.
    *   `subscriber`: The subscriber to remove. Must not be `null`.
    *   Unsubscribing an instance that is not registered does nothing.
    *   Throws: `ArgumentNullException` if `subscriber` is `null`.

*   **`public Task PublishAsync(AnalysisEvent @event, int maxConsecutiveFailures = 3)`**
    Delivers an event to every subscriber registered at the moment the call starts.
    *   `event`: The event to publish. Must not be `null`.
    *   `maxConsecutiveFailures`: The number of consecutive failures after which a subscriber is automatically unsubscribed. Defaults to `3`.
    *   Returns: A task that completes after every subscriber has been notified (or has failed).
    *   Throws: `ArgumentNullException` (delivered through the returned task, because the method is `async`) if `event` is `null`.
    *   Subscriber exceptions are not rethrown. They are logged and counted; see [Error Handling](#error-handling).

### Subscriber Contract

Subscribers implement `IAnalysisEventSubscriber`:

*   **`Task OnEventAsync(AnalysisEvent @event)`**
    Called once for every published event the subscriber receives. Subscribers that only care about some event types should check the type (for example with a `switch` expression) and return `Task.CompletedTask` for the rest.

## Subscription Management

*   **Order.** Subscribers are notified in the order they were added with `Subscribe`.
*   **Identity.** Subscribers are compared by reference (or by whatever `Equals` the implementation overrides). Two different instances of the same class are two separate subscribers.
*   **Snapshot dispatch.** `PublishAsync` copies the subscriber list under a lock before it starts notifying. Calls to `Subscribe` or `Unsubscribe` made during a publish affect later publishes only; the current publish still reaches the snapshot.
*   **Removed subscribers stay registered until they are removed.** Auto-unsubscribed subscribers are removed from the list and are not re-added. To receive events again, call `Subscribe` again.

## Publishing

`PublishAsync` works as follows:

1. Throws `ArgumentNullException` (via the returned task) if the event is `null`.
2. Copies the current subscriber list under the subscription lock.
3. Logs an information message with the event type and subscriber count.
4. For each subscriber in order, awaits `OnEventAsync(event)`. Dispatch is sequential, so a slow subscriber delays the ones after it.
5. If the subscriber completes normally, clears its failure count.
6. If it throws, records the failure (see below).
7. If at least one subscriber failed during this publish, logs an error with the number of failed subscribers.

Because dispatch is awaited one subscriber at a time, `PublishAsync` completes only when every subscriber has finished.

## Error Handling

Errors from subscribers are isolated per subscriber:

*   **Generic exceptions** are caught. The publisher logs a warning that includes the subscriber type, the failure count, the threshold, and the exception message, and then continues with the next subscriber. `PublishAsync` itself does not throw for these failures.
*   **`OperationCanceledException`** is not caught. If a subscriber throws it, the exception propagates out of `PublishAsync`, and subscribers after that one in the current publish are not notified. Subscribers should only throw it when the work was genuinely cancelled.
*   **Consecutive failure tracking.** Each subscriber has a failure counter. A successful call to `OnEventAsync` resets it to zero, so the threshold counts *consecutive* failures only.
*   **Auto-unsubscribe.** When a subscriber's failure count reaches `maxConsecutiveFailures`, the publisher logs a warning, removes the subscriber from the list, and removes its counter. The check is `>=`, so a value of `0` or less removes a subscriber on its first failure.
*   **Per-call threshold.** `maxConsecutiveFailures` is supplied on each `PublishAsync` call, so different publishers of events can apply different tolerances to the same subscriber.
*   **No aggregate exception.** Failures are not collected into an `AggregateException` and are not wrapped in `SubscriberException`. Callers that need to know about subscriber failures must rely on the logs.

The `SubscriberException` type declared in the same file is not used by this class.

## Logging

When a logger is supplied, the publisher writes:

| Level | When |
|-------|------|
| Debug | A subscriber is added or removed, including the resulting subscriber count |
| Information | Each publish, with the event type and number of subscribers |
| Warning | Each subscriber failure, with the failure count and threshold |
| Warning | A subscriber is auto-unsubscribed after reaching the failure threshold |
| Error | At least one subscriber failed during a publish |

Because the publish message is logged at Information level for every event, consider filtering `SqlQueryAnalyzer.Events` to Warning or above in production.

## Usage

### Example 1: Registering Subscribers and Publishing

```csharp
var publisher = new AnalysisEventPublisher(logger);

var logging = new LoggingEventSubscriber(loggerFactory.CreateLogger<LoggingEventSubscriber>());
var notifications = new NotificationEventSubscriber(loggerFactory.CreateLogger<NotificationEventSubscriber>());

publisher.Subscribe(logging);
publisher.Subscribe(notifications);

await publisher.PublishAsync(new AnalysisStartedEvent
{
    QueryId = "query-42",
    Query = "SELECT * FROM Orders"
});
```

`logging` is notified first, then `notifications`, because they were subscribed in that order.

### Example 2: Custom Subscriber That Filters by Event Type

```csharp
public class SlowQueryAlertSubscriber : IAnalysisEventSubscriber
{
    public Task OnEventAsync(AnalysisEvent @event)
    {
        if (@event is AnalysisCompletedEvent completed &&
            completed.AnalysisDuration > TimeSpan.FromSeconds(5))
        {
            // Raise an alert for slow analyses
        }

        return Task.CompletedTask;
    }
}
```

### Example 3: Tolerating Flaky Subscribers

```csharp
// Remove a subscriber only after five failures in a row
await publisher.PublishAsync(analysisCompletedEvent, maxConsecutiveFailures: 5);
```

### Example 4: Removing a Subscriber

```csharp
publisher.Unsubscribe(notifications);
```

## Notes

*   **Thread Safety.** `Subscribe`, `Unsubscribe`, and `PublishAsync` may be called from multiple threads. The subscriber list is guarded by a lock, and failure counts are stored in a `ConcurrentDictionary`. Because the list is snapshotted before dispatch, a publish never iterates a list that is being modified.
*   **Race with Unsubscribe.** If a subscriber is unsubscribed while a publish that already took its snapshot is running, that subscriber still receives the current event. If it fails during that publish, its failure counter may be recreated after `Unsubscribe` cleared it.
*   **Ordering Guarantees.** Delivery order is deterministic for a given subscription sequence, but the order of multiple concurrent `PublishAsync` calls is not guaranteed.
*   **Subscriber Robustness.** Subscribers should avoid long blocking work in `OnEventAsync`, because dispatch is sequential and every later subscriber waits for it.
*   **Event Structure.** `AnalysisEvent` provides `EventType` (the runtime type name), `Timestamp` (UTC), `CorrelationId`, and `Metadata`. Type-specific properties are only populated on the matching event subclass.
