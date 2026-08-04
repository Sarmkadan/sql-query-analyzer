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