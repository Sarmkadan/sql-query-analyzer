# WebhookNotificationServiceExtensions

Provides static extension methods for managing webhook notification configurations within the SQL query analyzer system. This type centralizes registration, unregistration, querying, and lifecycle control of webhooks that fire in response to query analysis events. All operations are stateless in the sense that they delegate to an underlying webhook store or service; the type itself does not hold mutable state.

## API

### `RegisterWebhooks`

```csharp
public static void RegisterWebhooks(WebhookConfiguration configuration)
```

Registers a new webhook configuration. The configuration is validated and persisted. If a webhook with the same identity already exists, the behavior depends on the underlying store (typically overwrite or duplicate detection).

- **Parameters**: `configuration` — the fully populated `WebhookConfiguration` to register.
- **Throws**: `ArgumentNullException` when `configuration` is `null`; `ArgumentException` when the URL is invalid or required fields are missing.

---

### `UnregisterWebhooksByPattern`

```csharp
public static int UnregisterWebhooksByPattern(string urlPattern)
public static int UnregisterWebhooksByPattern(string urlPattern, WebhookType? type)
```

Removes all webhooks whose URL matches the given pattern. The overload with `type` further restricts removal to webhooks of a specific `WebhookType`. Matching is performed using the underlying store’s pattern semantics (typically prefix or glob).

- **Parameters**:
  - `urlPattern` — the URL pattern to match against registered webhooks.
  - `type` — optional `WebhookType` filter; when `null`, all types are considered.
- **Returns**: the number of webhook configurations removed.
- **Exceptions**: `ArgumentNullException` when `urlPattern` is `null`.

---

### `UnregisterWebhooks`

```csharp
public static int UnregisterWebhooks(string eventName)
```

Removes all webhooks associated with the specified event name.

- **Parameters**: `eventName` — the exact event name whose webhooks should be removed.
- **Returns**: the number of webhook configurations removed.
- **Exceptions**: `ArgumentNullException` when `eventName` is `null`.

---

### `GetWebhooks`

```csharp
public static IReadOnlyList<WebhookConfiguration> GetWebhooks()
```

Returns all registered webhook configurations, regardless of event, type, or enabled state.

- **Returns**: a read-only list of all `WebhookConfiguration` instances. May be empty.

---

### `GetWebhooksForEvent`

```csharp
public static IReadOnlyList<WebhookConfiguration> GetWebhooksForEvent(string eventName)
```

Returns all webhook configurations subscribed to the specified event.

- **Parameters**: `eventName` — the event name to filter by.
- **Returns**: a read-only list of matching configurations.
- **Exceptions**: `ArgumentNullException` when `eventName` is `null`.

---

### `GetWebhooksByType`

```csharp
public static IReadOnlyList<WebhookConfiguration> GetWebhooksByType(WebhookType type)
```

Returns all webhook configurations of the given `WebhookType`.

- **Parameters**: `type` — the webhook type to filter by.
- **Returns**: a read-only list of matching configurations.

---

### `GetEnabledWebhooks`

```csharp
public static IReadOnlyList<WebhookConfiguration> GetEnabledWebhooks()
```

Returns only webhook configurations that are currently enabled.

- **Returns**: a read-only list of enabled `WebhookConfiguration` instances.

---

### `GetWebhook`

```csharp
public static WebhookConfiguration? GetWebhook(string id)
```

Retrieves a single webhook configuration by its unique identifier.

- **Parameters**: `id` — the unique identifier of the webhook.
- **Returns**: the matching `WebhookConfiguration`, or `null` if not found.
- **Exceptions**: `ArgumentNullException` when `id` is `null`.

---

### `HasWebhookForEvent`

```csharp
public static bool HasWebhookForEvent(string eventName)
```

Determines whether at least one webhook is registered for the given event.

- **Parameters**: `eventName` — the event name to check.
- **Returns**: `true` if one or more webhooks exist for the event; otherwise `false`.
- **Exceptions**: `ArgumentNullException` when `eventName` is `null`.

---

### `DisableWebhooks`

```csharp
public static int DisableWebhooks(string eventName)
```

Disables all webhooks registered for the specified event. Disabled webhooks are not invoked but remain in the configuration store.

- **Parameters**: `eventName` — the event name whose webhooks should be disabled.
- **Returns**: the number of webhook configurations that were disabled.
- **Exceptions**: `ArgumentNullException` when `eventName` is `null`.

---

### `EnableWebhooks`

```csharp
public static int EnableWebhooks(string eventName)
```

Enables all webhooks registered for the specified event.

- **Parameters**: `eventName` — the event name whose webhooks should be enabled.
- **Returns**: the number of webhook configurations that were enabled.
- **Exceptions**: `ArgumentNullException` when `eventName` is `null`.

---

### `GetEnabledWebhookCountForEvent`

```csharp
public static int GetEnabledWebhookCountForEvent(string eventName)
```

Returns the count of enabled webhooks for a given event.

- **Parameters**: `eventName` — the event name to query.
- **Returns**: the number of enabled webhooks for the event.
- **Exceptions**: `ArgumentNullException` when `eventName` is `null`.

---

### `CreateWebhookConfiguration`

```csharp
public static WebhookConfiguration CreateWebhookConfiguration(string url, string eventName, WebhookType type)
```

Factory method that creates a new `WebhookConfiguration` instance with sensible defaults (enabled, empty headers). The configuration is not automatically registered; call `RegisterWebhook` to persist it.

- **Parameters**:
  - `url` — the target URL for the webhook.
  - `eventName` — the event to subscribe to.
  - `type` — the `WebhookType` of the configuration.
- **Returns**: a new `WebhookConfiguration` instance.
- **Exceptions**: `ArgumentNullException` when any parameter is `null`; `ArgumentException` when `url` is not a valid absolute URL.

---

### `AddCustomHeaders`

```csharp
public static int AddCustomHeaders(string id, IDictionary<string, string> headers)
```

Adds or updates custom HTTP headers on an existing webhook configuration identified by `id`. Headers with keys that already exist are overwritten.

- **Parameters**:
  - `id` — the webhook configuration identifier.
  - `headers` — a dictionary of header names and values to add.
- **Returns**: the total number of custom headers on the configuration after the operation.
- **Exceptions**: `ArgumentNullException` when `id` or `headers` is `null`; `KeyNotFoundException` when no webhook with the given `id` exists.

---

### `RemoveCustomHeaders`

```csharp
public static int RemoveCustomHeaders(string id, IEnumerable<string> headerNames)
```

Removes the specified custom headers from the webhook configuration.

- **Parameters**:
  - `id` — the webhook configuration identifier.
  - `headerNames` — the names of the headers to remove.
- **Returns**: the total number of custom headers remaining on the configuration.
- **Exceptions**: `ArgumentNullException` when `id` or `headerNames` is `null`; `KeyNotFoundException` when no webhook with the given `id` exists.

---

### `GetWebhookUrls`

```csharp
public static IReadOnlyList<string> GetWebhookUrls(string eventName)
```

Returns the distinct URLs of all webhooks registered for the given event.

- **Parameters**: `eventName` — the event name.
- **Returns**: a read-only list of URL strings. May be empty.
- **Exceptions**: `ArgumentNullException` when `eventName` is `null`.

---

### `HasWebhooks`

```csharp
public static bool HasWebhooks()
```

Indicates whether any webhook configurations exist at all.

- **Returns**: `true` if at least one webhook is registered; otherwise `false`.

---

### `GetWebhookStatistics`

```csharp
public static Dictionary<string, int> GetWebhookStatistics()
```

Returns a dictionary mapping event names to the number of webhooks registered for each event. Only events with at least one webhook are included.

- **Returns**: a dictionary of event names to webhook counts.

---

## Usage

### Example 1: Registering and querying webhooks

```csharp
// Create and register a new webhook for query analysis completion
var config = WebhookNotificationServiceExtensions.CreateWebhookConfiguration(
    "https://hooks.example.com/analyzer",
    "QueryAnalysisCompleted",
    WebhookType.HttpPost
);

WebhookNotificationServiceExtensions.RegisterWebhook(config);

// Add custom authentication header
WebhookNotificationServiceExtensions.AddCustomHeaders(
    config.Id,
    new Dictionary<string, string> { ["X-API-Key"] = "abc123" }
);

// Verify registration
bool exists = WebhookNotificationServiceExtensions.HasWebhookForEvent("QueryAnalysisCompleted");
var urls = WebhookNotificationServiceExtensions.GetWebhookUrls("QueryAnalysisCompleted");

Console.WriteLine($"Webhook registered: {exists}, URLs: {string.Join(", ", urls)}");
```

### Example 2: Bulk lifecycle management

```csharp
// Disable all webhooks for a deprecated event
int disabled = WebhookNotificationServiceExtensions.DisableWebhooks("LegacyQueryCompleted");
Console.WriteLine($"Disabled {disabled} webhooks for legacy event.");

// Remove webhooks matching a pattern
int removed = WebhookNotificationServiceExtensions.UnregisterWebhooksByPattern(
    "https://old-endpoint.example.com/",
    WebhookType.Http
);
Console.WriteLine($"Removed {removed} webhooks matching old endpoint.");

// Print statistics
var stats = WebhookNotificationServiceExtensions.GetWebhookStatistics();
foreach (var kvp in stats)
{
    Console.WriteLine($"Event: {kvp.Key}, Webhooks: {kvp.Value}");
}
```

## Notes

- **Thread safety**: All methods are static and delegate to an underlying thread-safe store or service. Concurrent calls to `RegisterWebhook`, `UnregisterWebhooks`, and the various query methods are safe; however, compound operations (e.g., check-then-register) are not atomic and may race with other callers.
- **Pattern matching**: `UnregisterWebhooksByPattern` uses the store’s URL matching semantics. Callers should verify the matching behavior (e.g., prefix vs. substring) before relying on it for precise removals.
- **Disabled webhooks**: Disabling a webhook does not remove it; `GetWebhooks` and `GetWebhooksForEvent` still return disabled configurations. Use `GetEnabledWebhooks` to filter them out.
- **Header operations**: `AddCustomHeaders` and `RemoveCustomHeaders` operate on an existing webhook identified by `id`. If the webhook is removed between retrieval and the header operation, a `KeyNotFoundException` is thrown.
- **Return values**: Methods returning `int` (e.g., `DisableWebhooks`, `UnregisterWebhooks`) return zero when no configurations match the criteria; they do not throw.
- **`CreateWebhookConfiguration`**: This is a pure factory method. The returned configuration is not persisted until `RegisterWebhook` is called. The caller is responsible for assigning any additional properties before registration.
