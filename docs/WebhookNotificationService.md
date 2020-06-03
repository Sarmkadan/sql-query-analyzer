# WebhookNotificationService
The `WebhookNotificationService` class is designed to handle webhook notifications for the sql-query-analyzer project. It provides a set of methods and properties to manage webhooks, including registration, unregistration, and event handling. This class is intended to be used in scenarios where notifications need to be sent to external services in response to specific events or conditions.

## API
### Constructors
* `public WebhookNotificationService`: Initializes a new instance of the `WebhookNotificationService` class.

### Methods
* `public void RegisterWebhook`: Registers a webhook with the service. This method does not take any parameters and does not return a value. It may throw exceptions if the registration process fails.
* `public void UnregisterWebhook`: Unregisters a previously registered webhook. This method does not take any parameters and does not return a value. It may throw exceptions if the unregistration process fails.
* `public async Task OnEventAsync`: Handles an event asynchronously. This method does not take any parameters and returns a `Task` object. It may throw exceptions if the event handling process fails.

### Properties
* `public int GetWebhookCount`: Gets the number of registered webhooks. This property returns an integer value and does not throw exceptions.
* `public string Name`: Gets or sets the name of the webhook. This property returns a string value and does not throw exceptions.
* `public string Url`: Gets or sets the URL of the webhook. This property returns a string value and does not throw exceptions.
* `public WebhookType Type`: Gets or sets the type of the webhook. This property returns a `WebhookType` value and does not throw exceptions.
* `public bool Enabled`: Gets or sets a value indicating whether the webhook is enabled. This property returns a boolean value and does not throw exceptions.
* `public bool NotifyOnCompletion`: Gets or sets a value indicating whether notifications should be sent on completion. This property returns a boolean value and does not throw exceptions.
* `public bool NotifyOnCriticalIssues`: Gets or sets a value indicating whether notifications should be sent on critical issues. This property returns a boolean value and does not throw exceptions.
* `public bool NotifyOnFailures`: Gets or sets a value indicating whether notifications should be sent on failures. This property returns a boolean value and does not throw exceptions.
* `public Dictionary<string, string>? CustomHeaders`: Gets or sets a dictionary of custom headers for the webhook. This property returns a dictionary of string keys and values, or null if no custom headers are set.

## Usage
The following examples demonstrate how to use the `WebhookNotificationService` class:
```csharp
// Example 1: Registering a webhook
var webhookService = new WebhookNotificationService();
webhookService.Name = "My Webhook";
webhookService.Url = "https://example.com/webhook";
webhookService.Type = WebhookType.Http;
webhookService.Enabled = true;
webhookService.NotifyOnCompletion = true;
webhookService.RegisterWebhook();

// Example 2: Handling an event
var webhookService = new WebhookNotificationService();
await webhookService.OnEventAsync();
```

## Notes
When using the `WebhookNotificationService` class, consider the following edge cases and thread-safety remarks:
* The `RegisterWebhook` and `UnregisterWebhook` methods may throw exceptions if the registration or unregistration process fails. It is recommended to handle these exceptions accordingly.
* The `OnEventAsync` method is asynchronous and may throw exceptions if the event handling process fails. It is recommended to handle these exceptions accordingly and to await the completion of the task.
* The `GetWebhookCount` property returns the number of registered webhooks, which may change over time. It is recommended to cache this value if necessary.
* The `CustomHeaders` property returns a dictionary of custom headers, which may be null if no custom headers are set. It is recommended to check for null before accessing the dictionary.
* The `WebhookNotificationService` class is not thread-safe by default. If multiple threads need to access the same instance, it is recommended to synchronize access using locks or other synchronization mechanisms.
