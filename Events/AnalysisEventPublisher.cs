#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;

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
    Task PublishAsync(AnalysisEvent @event);
}

/// <summary>
/// Implementation of event publisher using observer pattern.
/// </summary>
public class AnalysisEventPublisher : IAnalysisEventPublisher
{
    private readonly List<IAnalysisEventSubscriber> _subscribers = new();
    private readonly ILogger<AnalysisEventPublisher> _logger;

    public AnalysisEventPublisher(ILogger<AnalysisEventPublisher> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Registers an event subscriber.
    /// Subscriber will receive all published events.
    /// </summary>
    public void Subscribe(IAnalysisEventSubscriber subscriber)
    {
        if (!_subscribers.Contains(subscriber))
        {
            _subscribers.Add(subscriber);
            _logger.LogDebug($"Subscribed: {subscriber.GetType().Name}");
        }
    }

    /// <summary>
    /// Unregisters an event subscriber.
    /// </summary>
    public void Unsubscribe(IAnalysisEventSubscriber subscriber)
    {
        if (_subscribers.Remove(subscriber))
        {
            _logger.LogDebug($"Unsubscribed: {subscriber.GetType().Name}");
        }
    }

    /// <summary>
    /// Publishes an event to all subscribers.
    /// Uses asynchronous dispatch to avoid blocking analysis.
    /// </summary>
    public async Task PublishAsync(AnalysisEvent @event)
    {
        _logger.LogDebug($"Publishing event: {@event.EventType}");

        var tasks = _subscribers
            .Select(s => PublishToSubscriberAsync(s, @event))
            .ToList();

        try
        {
            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing event to subscribers");
        }
    }

    /// <summary>
    /// Publishes event to a single subscriber with error handling.
    /// </summary>
    private async Task PublishToSubscriberAsync(IAnalysisEventSubscriber subscriber, AnalysisEvent @event)
    {
        try
        {
            await subscriber.OnEventAsync(@event);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"Subscriber {subscriber.GetType().Name} threw exception");
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
        _logger.LogInformation($"Event: {@event.EventType} at {@event.Timestamp:yyyy-MM-dd HH:mm:ss}");

        return @event switch
        {
            AnalysisCompletedEvent completed =>
                Task.Run(() => _logger.LogInformation(
                    $"Analysis completed: {completed.QueryId}, Score: {completed.PerformanceScore:F1}, Issues: {completed.IssuesFound}")),

            CriticalIssueDetectedEvent critical =>
                Task.Run(() => _logger.LogError(
                    $"Critical issue detected: {critical.IssueType} - {critical.Description} ({critical.ImpactPercentage:F1}%)")),

            AnalysisFailedEvent failed =>
                Task.Run(() => _logger.LogError(
                    $"Analysis failed: {failed.ErrorMessage} ({failed.ExceptionType})")),

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
        return @event switch
        {
            CriticalIssueDetectedEvent critical =>
                SendNotificationAsync(
                    "CRITICAL: SQL Performance Issue Detected",
                    $"{critical.IssueType}: {critical.Description}"),

            AnalysisFailedEvent failed =>
                SendNotificationAsync(
                    "ERROR: Analysis Failed",
                    failed.ErrorMessage),

            _ => Task.CompletedTask
        };
    }

    private async Task SendNotificationAsync(string subject, string message)
    {
        _logger.LogWarning($"Notification: {subject} - {message}");

        // In production, integrate with notification service (email, Slack, etc)
        await Task.Delay(10);
    }
}
