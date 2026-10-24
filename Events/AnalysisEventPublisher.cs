#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace SqlQueryAnalyzer.Events;

/// <summary>
/// Publishes domain events from the analysis pipeline.
/// Decouples analysis logic from side effects (logging, caching, notifications).
/// Implements observer pattern for event distribution.
/// </summary>
public interface IAnalysisEventPublisher
{
    void Subscribe(IAnalysisEventSubscriber subscriber);
    void Unsubscribe(IAnalysisEventSubscriber subscriber);
    Task PublishAsync(AnalysisEvent @event, int maxConsecutiveFailures = 3);
}

/// <summary>
/// Implementation of event publisher using observer pattern.
/// </summary>
public class AnalysisEventPublisher : IAnalysisEventPublisher
{
    // Guarded by _subscribersLock: subscription order matters and failing subscribers must be removed by identity.
    private readonly List<IAnalysisEventSubscriber> _subscribers = new();
    private readonly object _subscribersLock = new();
    private readonly ILogger<AnalysisEventPublisher>? _logger;
    private readonly ConcurrentDictionary<IAnalysisEventSubscriber, int> _exceptionCounts = new();

    public AnalysisEventPublisher(ILogger<AnalysisEventPublisher>? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// Registers an event subscriber.
    /// Subscriber will receive all published events. Registering the same subscriber twice is a no-op.
    /// </summary>
    /// <param name="subscriber">The subscriber to register. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown if subscriber is null.</exception>
    public void Subscribe(IAnalysisEventSubscriber subscriber)
    {
        ArgumentNullException.ThrowIfNull(subscriber);
        int count;
        lock (_subscribersLock)
        {
            if (_subscribers.Contains(subscriber))
                return;

            _subscribers.Add(subscriber);
            count = _subscribers.Count;
        }

        _logger?.LogDebug(
            "Subscribed {SubscriberType}; subscriber count is {SubscriberCount}",
            subscriber.GetType().Name,
            count);
    }

    /// <summary>
    /// Unregisters an event subscriber.
    /// </summary>
    /// <param name="subscriber">The subscriber to unregister. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown if subscriber is null.</exception>
    public void Unsubscribe(IAnalysisEventSubscriber subscriber)
    {
        ArgumentNullException.ThrowIfNull(subscriber);
        int count;
        lock (_subscribersLock)
        {
            _subscribers.Remove(subscriber);
            count = _subscribers.Count;
        }

        _exceptionCounts.TryRemove(subscriber, out _);
        _logger?.LogDebug(
            "Unsubscribed {SubscriberType}; subscriber count is {SubscriberCount}",
            subscriber.GetType().Name,
            count);
    }

    /// <summary>
    /// Publishes an event to all subscribers.
    /// Uses sequential, ordered dispatch to ensure subscribers receive events in subscription order.
    /// A failing subscriber is isolated: its exception is logged, and the remaining subscribers still receive the event.
    /// Subscribers that fail repeatedly are automatically unsubscribed based on <paramref name="maxConsecutiveFailures"/>.
    /// </summary>
    /// <param name="event">The event to publish. Must not be null.</param>
    /// <param name="maxConsecutiveFailures">Maximum consecutive failures before auto-unsubscribing a subscriber. Default is 3.</param>
    /// <returns>A task that completes when all subscribers have been notified.</returns>
    /// <exception cref="ArgumentNullException">Thrown if @event is null.</exception>
    public async Task PublishAsync(AnalysisEvent @event, int maxConsecutiveFailures = 3)
    {
        ArgumentNullException.ThrowIfNull(@event);
        List<IAnalysisEventSubscriber> snapshot;
        lock (_subscribersLock)
        {
            snapshot = [.. _subscribers];
        }

        _logger?.LogInformation(
            "Publishing event {EventType} to {SubscriberCount} subscribers",
            @event.EventType,
            snapshot.Count);

        int failedCount = 0;
        foreach (var subscriber in snapshot)
        {
            try
            {
                await PublishToSubscriberAsync(subscriber, @event);
                _exceptionCounts.TryRemove(subscriber, out _);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                failedCount++;
                TrackSubscriberFailure(subscriber, ex, maxConsecutiveFailures);
            }
        }

        if (failedCount > 0)
        {
            _logger?.LogError("Failed to publish event to {Count} subscriber(s)", failedCount);
        }
    }

    /// <summary>
    /// Publishes event to a single subscriber.
    /// </summary>
    /// <param name="subscriber">The subscriber to notify.</param>
    /// <param name="event">The event to publish.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task PublishToSubscriberAsync(IAnalysisEventSubscriber subscriber, AnalysisEvent @event)
    {
        ArgumentNullException.ThrowIfNull(subscriber);
        ArgumentNullException.ThrowIfNull(@event);
        await subscriber.OnEventAsync(@event);
    }

    /// <summary>
    /// Tracks subscriber failures and performs auto-unsubscribe if threshold is exceeded.
    /// </summary>
    /// <param name="subscriber">The subscriber that failed.</param>
    /// <param name="exception">The exception that was thrown.</param>
    /// <param name="maxConsecutiveFailures">Maximum allowed consecutive failures before unsubscribing.</param>
    private void TrackSubscriberFailure(IAnalysisEventSubscriber subscriber, Exception exception, int maxConsecutiveFailures)
    {
        var failureCount = _exceptionCounts.AddOrUpdate(
            subscriber,
            1,
            (_, current) => current + 1);

        _logger?.LogWarning(exception, "Subscriber {SubscriberType} failed ({FailureCount}/{MaxFailures} failures): {ErrorMessage}",
            subscriber.GetType().Name,
            failureCount,
            maxConsecutiveFailures,
            exception.Message);

        if (failureCount >= maxConsecutiveFailures)
        {
            _logger?.LogWarning("Auto-unsubscribing {SubscriberType} after {FailureCount} consecutive failures",
                subscriber.GetType().Name,
                failureCount);
            lock (_subscribersLock)
            {
                _subscribers.Remove(subscriber);
            }
            _exceptionCounts.TryRemove(subscriber, out _);
        }
    }
}

/// <summary>
/// Base interface for event subscribers.
/// Implementations handle specific event types.
/// </summary>
public interface IAnalysisEventSubscriber
{
    /// <summary>
    /// Called when an event is published.
    /// Implementation may filter by event type.
    /// </summary>
    Task OnEventAsync(AnalysisEvent @event);
}

/// <summary>
/// Base class for analysis domain events.
/// All events inherit from this class.
/// </summary>
public abstract class AnalysisEvent
{
    public string EventType => GetType().Name;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Event raised when query analysis starts.
/// </summary>
public class AnalysisStartedEvent : AnalysisEvent
{
    public string QueryId { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
}

/// <summary>
/// Event raised when query analysis completes.
/// Contains the analysis result.
/// </summary>
public class AnalysisCompletedEvent : AnalysisEvent
{
    public string QueryId { get; set; } = string.Empty;
    public double PerformanceScore { get; set; }
    public int IssuesFound { get; set; }
    public TimeSpan AnalysisDuration { get; set; }
}

/// <summary>
/// Event raised when critical issues are detected.
/// Triggers alerts and notifications.
/// </summary>
public class CriticalIssueDetectedEvent : AnalysisEvent
{
    public string QueryId { get; set; } = string.Empty;
    public string IssueType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double ImpactPercentage { get; set; }
}

/// <summary>
/// Event raised when analysis fails.
/// </summary>
public class AnalysisFailedEvent : AnalysisEvent
{
    public string QueryId { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public string ExceptionType { get; set; } = string.Empty;
}

/// <summary>
/// Subscriber that logs all events.
/// Useful for audit trail and debugging.
/// </summary>
public class LoggingEventSubscriber : IAnalysisEventSubscriber
{
    private readonly ILogger<LoggingEventSubscriber> _logger;

    public LoggingEventSubscriber(ILogger<LoggingEventSubscriber> logger)
    {
        _logger = logger;
    }

    public Task OnEventAsync(AnalysisEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);
        _logger.LogInformation("Event: {EventType} at {Timestamp:yyyy-MM-dd HH:mm:ss}", @event.EventType, @event.Timestamp);
        return @event switch
        {
            AnalysisCompletedEvent completed =>
                Task.Run(() => _logger.LogInformation(
                    "Analysis completed: {QueryId}, Score: {PerformanceScore:F1}, Issues: {IssuesFound}",
                    completed.QueryId, completed.PerformanceScore, completed.IssuesFound)),

            CriticalIssueDetectedEvent critical =>
                Task.Run(() => _logger.LogError(
                    "Critical issue detected: {IssueType} - {Description} ({ImpactPercentage:F1}%)",
                    critical.IssueType, critical.Description, critical.ImpactPercentage)),

            AnalysisFailedEvent failed =>
                Task.Run(() => _logger.LogError(
                    "Analysis failed: {ErrorMessage} ({ExceptionType})",
                    failed.ErrorMessage, failed.ExceptionType)),

            _ => Task.CompletedTask
        };
    }
}

/// <summary>
/// Subscriber that sends notifications for critical events.
/// </summary>
public class NotificationEventSubscriber : IAnalysisEventSubscriber
{
    private readonly ILogger<NotificationEventSubscriber> _logger;

    public NotificationEventSubscriber(ILogger<NotificationEventSubscriber> logger)
    {
        _logger = logger;
    }

    public Task OnEventAsync(AnalysisEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);
        _logger.LogInformation("Event: {EventType} at {Timestamp:yyyy-MM-dd HH:mm:ss}", @event.EventType, @event.Timestamp);
        return @event switch
        {
            CriticalIssueDetectedEvent critical => SendNotificationAsync(
                "CRITICAL: SQL Performance Issue Detected",
                $"{critical.IssueType}: {critical.Description}"),

            AnalysisFailedEvent failed => SendNotificationAsync(
                "ERROR: Analysis Failed",
                failed.ErrorMessage),

            _ => Task.CompletedTask
        };
    }

    private async Task SendNotificationAsync(string subject, string message)
    {
        _logger.LogWarning("Notification: {Subject} - {Message}", subject, message);
        // In production, integrate with notification service (email, Slack, etc)
        await Task.Delay(10);
    }
}

/// <summary>
/// Exception thrown when a subscriber fails to process an event.
/// Contains information about which subscriber failed and the underlying exception.
/// </summary>
public class SubscriberException : Exception
{
    /// <summary>
    /// Gets the name of the subscriber that failed.
    /// </summary>
    public string SubscriberName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SubscriberException"/> class.
    /// </summary>
    /// <param name="subscriberName">Name of the subscriber that failed.</param>
    /// <param name="innerException">The exception that caused the failure.</param>
    public SubscriberException(string subscriberName, Exception innerException)
        : base($"Subscriber '{subscriberName}' failed to process event", innerException)
    {
        SubscriberName = subscriberName ?? throw new ArgumentNullException(nameof(subscriberName));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SubscriberException"/> class.
    /// </summary>
    /// <param name="subscriberName">Name of the subscriber that failed.</param>
    /// <param name="message">Custom error message.</param>
    /// <param name="innerException">The exception that caused the failure.</param>
    public SubscriberException(string subscriberName, string message, Exception innerException)
        : base(message, innerException)
    {
        SubscriberName = subscriberName ?? throw new ArgumentNullException(nameof(subscriberName));
    }
}
