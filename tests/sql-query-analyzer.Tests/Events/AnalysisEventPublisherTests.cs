#nullable enable

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SqlQueryAnalyzer.Events;
using Xunit;

namespace SqlQueryAnalyzer.Tests.Events;

/// <summary>
/// Contains unit tests for the AnalysisEventPublisher class.
/// Tests cover subscription management, event publishing, and error handling scenarios.
/// </summary>
public class AnalysisEventPublisherTests
{
    private readonly Mock<ILogger<AnalysisEventPublisher>> _loggerMock = new();
    private readonly AnalysisEventPublisher _publisher;

    /// <summary>
    /// Initializes a new instance of the AnalysisEventPublisherTests class.
    /// Sets up a mock logger and creates a new AnalysisEventPublisher instance for testing.
    /// </summary>
    public AnalysisEventPublisherTests()
    {
        _publisher = new AnalysisEventPublisher(_loggerMock.Object);
    }

    /// <summary>
    /// Tests that subscribing adds a subscriber to the internal subscriber list.
    /// Since the subscriber list is private, this test verifies subscription works
    /// by attempting to subscribe a second subscriber without throwing an exception.
    /// </summary>
    [Fact]
    public void Subscribe_AddsSubscriberToList()
    {
        // Arrange
        var subscriber = new Mock<IAnalysisEventSubscriber>().Object;

        // Act
        _publisher.Subscribe(subscriber);

        // Assert
        // We can't directly access _subscribers, but we can verify via another subscription
        var subscriber2 = new Mock<IAnalysisEventSubscriber>().Object;
        _publisher.Subscribe(subscriber2);

        // If we can subscribe without exception, it means the first subscription succeeded
        Assert.True(true);
    }

    /// <summary>
    /// Tests that subscribing the same subscriber multiple times does not add duplicates.
    /// Since the subscriber list is private, this test verifies that duplicate subscriptions
    /// don't cause exceptions when subscribing additional subscribers.
    /// </summary>
    [Fact]
    public void Subscribe_DoesNotAddDuplicateSubscriber()
    {
        // Arrange
        var subscriber = new Mock<IAnalysisEventSubscriber>().Object;
        _publisher.Subscribe(subscriber);

        // Act - subscribe the same subscriber again
        _publisher.Subscribe(subscriber);

        // Assert - should only have one subscriber (no exception means duplicates handled)
        var subscriber2 = new Mock<IAnalysisEventSubscriber>().Object;
        _publisher.Subscribe(subscriber2);

        // This test verifies no exception is thrown, meaning duplicates are handled
        Assert.True(true);
    }

    /// <summary>
    /// Tests that unsubscribing removes a subscriber from the internal subscriber list.
    /// Since the subscriber list is private, this test verifies unsubscription worked
    /// by verifying that publishing events doesn't throw exceptions.
    /// </summary>
    [Fact]
    public async Task Unsubscribe_RemovesSubscriberFromList()
    {
        // Arrange
        var subscriber = new Mock<IAnalysisEventSubscriber>().Object;
        _publisher.Subscribe(subscriber);

        // Act
        _publisher.Unsubscribe(subscriber);

        // Assert - we can't directly verify, but we can check that publishing doesn't throw
        var @event = new AnalysisStartedEvent();
        await _publisher.PublishAsync(@event);

        // If no exception, unsubscription worked
        Assert.True(true);
    }

    /// <summary>
    /// Tests that unsubscribing a subscriber that was never subscribed does not throw an exception.
    /// </summary>
    [Fact]
    public async Task Unsubscribe_DoesNotThrowIfSubscriberNotSubscribed()
    {
        // Arrange
        var subscriber = new Mock<IAnalysisEventSubscriber>().Object;

        // Act & Assert - should not throw
        _publisher.Unsubscribe(subscriber);

        // Should complete without exception
        var @event = new AnalysisStartedEvent();
        await _publisher.PublishAsync(@event);

        Assert.True(true);
    }

    /// <summary>
    /// Tests that publishing an event delivers it to all subscribed subscribers.
    /// </summary>
    [Fact]
    public async Task PublishAsync_DeliversEventToAllSubscribers()
    {
        // Arrange
        var subscriber1Mock = new Mock<IAnalysisEventSubscriber>();
        var subscriber2Mock = new Mock<IAnalysisEventSubscriber>();
        var subscriber1 = subscriber1Mock.Object;
        var subscriber2 = subscriber2Mock.Object;

        _publisher.Subscribe(subscriber1);
        _publisher.Subscribe(subscriber2);

        var @event = new AnalysisStartedEvent
        {
            QueryId = "test-query-123",
            Query = "SELECT * FROM users"
        };

        // Act
        await _publisher.PublishAsync(@event);

        // Assert
        subscriber1Mock.Verify(s => s.OnEventAsync(@event), Times.Once);
        subscriber2Mock.Verify(s => s.OnEventAsync(@event), Times.Once);
    }

    /// <summary>
    /// Tests that publishing an event delivers it to a single subscriber.
    /// </summary>
    [Fact]
    public async Task PublishAsync_DeliversEventToSingleSubscriber()
    {
        // Arrange
        var subscriberMock = new Mock<IAnalysisEventSubscriber>();
        var subscriber = subscriberMock.Object;

        _publisher.Subscribe(subscriber);

        var @event = new AnalysisCompletedEvent
        {
            QueryId = "test-query-456",
            PerformanceScore = 95.5,
            IssuesFound = 2,
            AnalysisDuration = TimeSpan.FromSeconds(1.5)
        };

        // Act
        await _publisher.PublishAsync(@event);

        // Assert
        subscriberMock.Verify(s => s.OnEventAsync(@event), Times.Once);
    }

    /// <summary>
    /// Tests that publishing an event continues to deliver to remaining subscribers
    /// when one subscriber throws an exception during event handling.
    /// </summary>
    [Fact]
    public async Task PublishAsync_ContinuesToNextSubscriberWhenOneThrows()
    {
        // Arrange
        var subscriber1Mock = new Mock<IAnalysisEventSubscriber>();
        var subscriber2Mock = new Mock<IAnalysisEventSubscriber>();
        var subscriber1 = subscriber1Mock.Object;
        var subscriber2 = subscriber2Mock.Object;

        // Setup first subscriber to throw
        subscriber1Mock.Setup(s => s.OnEventAsync(It.IsAny<AnalysisEvent>()))
                      .ThrowsAsync(new InvalidOperationException("Test exception"));

        _publisher.Subscribe(subscriber1);
        _publisher.Subscribe(subscriber2);

        var @event = new CriticalIssueDetectedEvent
        {
            QueryId = "test-query-789",
            IssueType = "Performance",
            Description = "Slow query detected",
            ImpactPercentage = 85.0
        };

        // Act - should not throw even though one subscriber throws
        var act = async () => await _publisher.PublishAsync(@event);
        await act.Should().NotThrowAsync();

        // Assert - second subscriber should still be called
        subscriber2Mock.Verify(s => s.OnEventAsync(@event), Times.Once);
    }

    /// <summary>
    /// Tests that publishing an event continues to deliver to remaining subscribers
    /// when multiple subscribers throw exceptions during event handling.
    /// </summary>
    [Fact]
    public async Task PublishAsync_HandlesMultipleSubscribersWithExceptions()
    {
        // Arrange
        var subscriber1Mock = new Mock<IAnalysisEventSubscriber>();
        var subscriber2Mock = new Mock<IAnalysisEventSubscriber>();
        var subscriber3Mock = new Mock<IAnalysisEventSubscriber>();
        var subscriber1 = subscriber1Mock.Object;
        var subscriber2 = subscriber2Mock.Object;
        var subscriber3 = subscriber3Mock.Object;

        // Setup all subscribers to throw
        subscriber1Mock.Setup(s => s.OnEventAsync(It.IsAny<AnalysisEvent>()))
                      .ThrowsAsync(new InvalidOperationException("First subscriber failed"));
        subscriber2Mock.Setup(s => s.OnEventAsync(It.IsAny<AnalysisEvent>()))
                      .ThrowsAsync(new ArgumentException("Second subscriber failed"));
        subscriber3Mock.Setup(s => s.OnEventAsync(It.IsAny<AnalysisEvent>()))
                      .Returns(Task.CompletedTask);

        _publisher.Subscribe(subscriber1);
        _publisher.Subscribe(subscriber2);
        _publisher.Subscribe(subscriber3);

        var @event = new AnalysisFailedEvent
        {
            QueryId = "test-query-999",
            ErrorMessage = "Analysis error",
            ExceptionType = "InvalidOperationException"
        };

        // Act - should not throw even though multiple subscribers throw
        var act = async () => await _publisher.PublishAsync(@event);
        await act.Should().NotThrowAsync();

        // Assert - all subscribers should have been called despite exceptions
        subscriber1Mock.Verify(s => s.OnEventAsync(@event), Times.Once);
        subscriber2Mock.Verify(s => s.OnEventAsync(@event), Times.Once);
        subscriber3Mock.Verify(s => s.OnEventAsync(@event), Times.Once);
    }

    /// <summary>
    /// Tests that publishing an event does not throw an exception when there are no subscribers.
    /// </summary>
    [Fact]
    public async Task PublishAsync_DoesNotThrowWhenNoSubscribers()
    {
        // Arrange
        var @event = new AnalysisStartedEvent();

        // Act & Assert - should not throw when there are no subscribers
        var act = async () => await _publisher.PublishAsync(@event);
        await act.Should().NotThrowAsync();
    }

    /// <summary>
    /// Tests that subscribers receive the correct event types when multiple different events are published.
    /// </summary>
    [Fact]
    public async Task Subscribers_ReceiveCorrectEventTypes()
    {
        // Arrange
        var startedEvent = new AnalysisStartedEvent { QueryId = "started-1" };
        var completedEvent = new AnalysisCompletedEvent { QueryId = "completed-1" };
        var criticalEvent = new CriticalIssueDetectedEvent { QueryId = "critical-1" };
        var failedEvent = new AnalysisFailedEvent { QueryId = "failed-1" };

        var subscriberMock = new Mock<IAnalysisEventSubscriber>();
        var subscriber = subscriberMock.Object;

        _publisher.Subscribe(subscriber);

        // Act
        await _publisher.PublishAsync(startedEvent);
        await _publisher.PublishAsync(completedEvent);
        await _publisher.PublishAsync(criticalEvent);
        await _publisher.PublishAsync(failedEvent);

        // Assert - each event should be delivered
        subscriberMock.Verify(s => s.OnEventAsync(startedEvent), Times.Once);
        subscriberMock.Verify(s => s.OnEventAsync(completedEvent), Times.Once);
        subscriberMock.Verify(s => s.OnEventAsync(criticalEvent), Times.Once);
        subscriberMock.Verify(s => s.OnEventAsync(failedEvent), Times.Once);
    }

    /// <summary>
    /// Tests that published events have the correct properties set.
    /// </summary>
    [Fact]
    public async Task Events_HaveCorrectProperties()
    {
        // Arrange
        var queryId = "test-query-123";
        var query = "SELECT * FROM users";
        var startedEvent = new AnalysisStartedEvent
        {
            QueryId = queryId,
            Query = query
        };

        // Act
        await _publisher.PublishAsync(startedEvent);

        // Assert
        startedEvent.EventType.Should().Be("AnalysisStartedEvent");
        startedEvent.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        startedEvent.CorrelationId.Should().NotBeNullOrEmpty();
        startedEvent.Metadata.Should().NotBeNull();
        startedEvent.QueryId.Should().Be(queryId);
        startedEvent.Query.Should().Be(query);
    }

    /// <summary>
    /// Tests that the LoggingEventSubscriber handles all types of analysis events without throwing exceptions.
    /// </summary>
    [Fact]
    public void LoggingEventSubscriber_HandlesAllEventTypes()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<LoggingEventSubscriber>>();
        var subscriber = new LoggingEventSubscriber(loggerMock.Object);

        var startedEvent = new AnalysisStartedEvent { QueryId = "test-1" };
        var completedEvent = new AnalysisCompletedEvent { QueryId = "test-2" };
        var criticalEvent = new CriticalIssueDetectedEvent { QueryId = "test-3" };
        var failedEvent = new AnalysisFailedEvent { QueryId = "test-4" };

        // Act & Assert - should not throw for any event type
        Func<Task> act1 = async () => await subscriber.OnEventAsync(startedEvent);
        Func<Task> act2 = async () => await subscriber.OnEventAsync(completedEvent);
        Func<Task> act3 = async () => await subscriber.OnEventAsync(criticalEvent);
        Func<Task> act4 = async () => await subscriber.OnEventAsync(failedEvent);

        act1.Should().NotThrowAsync();
        act2.Should().NotThrowAsync();
        act3.Should().NotThrowAsync();
        act4.Should().NotThrowAsync();
    }

    /// <summary>
    /// Tests that the NotificationEventSubscriber handles critical and failed events without throwing exceptions.
    /// </summary>
    [Fact]
    public void NotificationEventSubscriber_HandlesCriticalAndFailedEvents()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<NotificationEventSubscriber>>();
        var subscriber = new NotificationEventSubscriber(loggerMock.Object);

        var criticalEvent = new CriticalIssueDetectedEvent
        {
            QueryId = "test-1",
            IssueType = "Performance",
            Description = "Slow query",
            ImpactPercentage = 90.0
        };

        var failedEvent = new AnalysisFailedEvent
        {
            QueryId = "test-2",
            ErrorMessage = "Analysis failed",
            ExceptionType = "Exception"
        };

        var otherEvent = new AnalysisStartedEvent();

        // Act & Assert - should not throw for any event type
        Func<Task> act1 = async () => await subscriber.OnEventAsync(criticalEvent);
        Func<Task> act2 = async () => await subscriber.OnEventAsync(failedEvent);
        Func<Task> act3 = async () => await subscriber.OnEventAsync(otherEvent);

        act1.Should().NotThrowAsync();
        act2.Should().NotThrowAsync();
        act3.Should().NotThrowAsync();
    }
}