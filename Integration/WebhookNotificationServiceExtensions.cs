#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using SqlQueryAnalyzer.Events;
using SqlQueryAnalyzer.Models;

namespace SqlQueryAnalyzer.Integration;

/// <summary>
/// Extension methods for <see cref="WebhookNotificationService"/> that provide common operations
/// for webhook management, filtering, and bulk operations.
/// </summary>
public static class WebhookNotificationServiceExtensions
{
    /// <summary>
    /// Registers multiple webhook configurations at once.
    /// </summary>
    /// <param name="service">The webhook notification service.</param>
    /// <param name="configurations">Collection of webhook configurations to register.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> or <paramref name="configurations"/> is null.</exception>
    public static void RegisterWebhooks(this WebhookNotificationService service, IEnumerable<WebhookConfiguration> configurations)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(configurations);

        foreach (var config in configurations)
        {
            service.RegisterWebhook(config);
        }
    }

    /// <summary>
    /// Unregisters all webhooks matching the specified name pattern.
    /// Uses ordinal, case-sensitive comparison.
    /// </summary>
    /// <param name="service">The webhook notification service.</param>
    /// <param name="namePattern">The pattern to match against webhook names.</param>
    /// <returns>The number of webhooks that were unregistered.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> or <paramref name="namePattern"/> is null.</exception>
    public static int UnregisterWebhooksByPattern(this WebhookNotificationService service, string namePattern)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrEmpty(namePattern);

        return service.UnregisterWebhooksByPattern(namePattern, StringComparison.Ordinal);
    }

    /// <summary>
    /// Unregisters all webhooks matching the specified name pattern with custom comparison.
    /// </summary>
    /// <param name="service">The webhook notification service.</param>
    /// <param name="namePattern">The pattern to match against webhook names.</param>
    /// <param name="comparisonType">String comparison type to use.</param>
    /// <returns>The number of webhooks that were unregistered.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> or <paramref name="namePattern"/> is null.</exception>
    public static int UnregisterWebhooksByPattern(this WebhookNotificationService service, string namePattern, StringComparison comparisonType)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrEmpty(namePattern);

        var removed = service.UnregisterWebhooks(config => string.Equals(config.Name, namePattern, comparisonType));
        return removed;
    }

    /// <summary>
    /// Unregisters webhooks that match the specified predicate.
    /// </summary>
    /// <param name="service">The webhook notification service.</param>
    /// <param name="predicate">Predicate to determine which webhooks to unregister.</param>
    /// <returns>The number of webhooks that were unregistered.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> or <paramref name="predicate"/> is null.</exception>
    public static int UnregisterWebhooks(this WebhookNotificationService service, Func<WebhookConfiguration, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(predicate);

        var webhooks = service.GetWebhooks();
        var count = 0;

        foreach (var webhook in webhooks)
        {
            if (predicate(webhook))
            {
                service.UnregisterWebhook(webhook.Name);
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// Gets all registered webhooks as a read-only collection.
    /// </summary>
    /// <param name="service">The webhook notification service.</param>
    /// <returns>Read-only collection of webhook configurations.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null.</exception>
    public static IReadOnlyList<WebhookConfiguration> GetWebhooks(this WebhookNotificationService service)
    {
        ArgumentNullException.ThrowIfNull(service);

        return service.GetWebhooksInternal();
    }

    /// <summary>
    /// Gets webhooks filtered by notification type.
    /// </summary>
    /// <param name="service">The webhook notification service.</param>
    /// <param name="eventType">The event type to filter by.</param>
    /// <returns>Read-only collection of matching webhook configurations.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null.</exception>
    public static IReadOnlyList<WebhookConfiguration> GetWebhooksForEvent(this WebhookNotificationService service, Type eventType)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(eventType);

        return service.GetWebhooks().Where(config => config.ShouldNotifyForEvent(eventType)).ToList().AsReadOnly();
    }

    /// <summary>
    /// Gets webhooks filtered by webhook type.
    /// </summary>
    /// <param name="service">The webhook notification service.</param>
    /// <param name="webhookType">The webhook type to filter by.</param>
    /// <returns>Read-only collection of matching webhook configurations.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null.</exception>
    public static IReadOnlyList<WebhookConfiguration> GetWebhooksByType(this WebhookNotificationService service, WebhookType webhookType)
    {
        ArgumentNullException.ThrowIfNull(service);

        return service.GetWebhooks().Where(config => config.Type == webhookType).ToList().AsReadOnly();
    }

    /// <summary>
    /// Gets webhooks that are enabled.
    /// </summary>
    /// <param name="service">The webhook notification service.</param>
    /// <returns>Read-only collection of enabled webhook configurations.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null.</exception>
    public static IReadOnlyList<WebhookConfiguration> GetEnabledWebhooks(this WebhookNotificationService service)
    {
        ArgumentNullException.ThrowIfNull(service);

        return service.GetWebhooks().Where(config => config.Enabled).ToList().AsReadOnly();
    }

    /// <summary>
    /// Gets the first webhook with the specified name.
    /// </summary>
    /// <param name="service">The webhook notification service.</param>
    /// <param name="webhookName">The name of the webhook to find.</param>
    /// <returns>The webhook configuration, or null if not found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> or <paramref name="webhookName"/> is null.</exception>
    public static WebhookConfiguration? GetWebhook(this WebhookNotificationService service, string webhookName)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrEmpty(webhookName);

        return service.GetWebhooks().FirstOrDefault(config => config.Name == webhookName);
    }

    /// <summary>
    /// Determines whether any webhook is configured to notify for the specified event type.
    /// </summary>
    /// <param name="service">The webhook notification service.</param>
    /// <param name="eventType">The event type to check.</param>
    /// <returns>True if at least one webhook should notify for this event; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> or <paramref name="eventType"/> is null.</exception>
    public static bool HasWebhookForEvent(this WebhookNotificationService service, Type eventType)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(eventType);

        return service.GetWebhooksForEvent(eventType).Count > 0;
    }

    /// <summary>
    /// Disables all webhooks matching the specified predicate.
    /// </summary>
    /// <param name="service">The webhook notification service.</param>
    /// <param name="predicate">Predicate to determine which webhooks to disable.</param>
    /// <returns>The number of webhooks that were disabled.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> or <paramref name="predicate"/> is null.</exception>
    public static int DisableWebhooks(this WebhookNotificationService service, Func<WebhookConfiguration, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(predicate);

        var webhooks = service.GetWebhooks();
        var count = 0;

        foreach (var webhook in webhooks)
        {
            if (predicate(webhook))
            {
                webhook.Enabled = false;
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// Enables all webhooks matching the specified predicate.
    /// </summary>
    /// <param name="service">The webhook notification service.</param>
    /// <param name="predicate">Predicate to determine which webhooks to enable.</param>
    /// <returns>The number of webhooks that were enabled.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> or <paramref name="predicate"/> is null.</exception>
    public static int EnableWebhooks(this WebhookNotificationService service, Func<WebhookConfiguration, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(predicate);

        var webhooks = service.GetWebhooks();
        var count = 0;

        foreach (var webhook in webhooks)
        {
            if (predicate(webhook))
            {
                webhook.Enabled = true;
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// Gets the count of webhooks that are enabled and configured to notify for the specified event type.
    /// </summary>
    /// <param name="service">The webhook notification service.</param>
    /// <param name="eventType">The event type to check.</param>
    /// <returns>The number of matching webhooks.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> or <paramref name="eventType"/> is null.</exception>
    public static int GetEnabledWebhookCountForEvent(this WebhookNotificationService service, Type eventType)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(eventType);

        return service.GetWebhooksForEvent(eventType).Count(config => config.Enabled);
    }

    /// <summary>
    /// Creates a new webhook configuration with common defaults for the specified URL.
    /// </summary>
    /// <param name="service">The webhook notification service (unused but required for extension method).</param>
    /// <param name="url">The webhook URL.</param>
    /// <param name="name">The name of the webhook.</param>
    /// <param name="webhookType">The type of webhook service.</param>
    /// <returns>A new webhook configuration with sensible defaults.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="url"/> or <paramref name="name"/> is null.</exception>
    public static WebhookConfiguration CreateWebhookConfiguration(this WebhookNotificationService service, string url, string name, WebhookType webhookType = WebhookType.Custom)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrEmpty(url);
        ArgumentException.ThrowIfNullOrEmpty(name);

        return new WebhookConfiguration
        {
            Name = name,
            Url = url,
            Type = webhookType,
            Enabled = true,
            NotifyOnCompletion = true,
            NotifyOnCriticalIssues = true,
            NotifyOnFailures = true,
            CustomHeaders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        };
    }

    /// <summary>
    /// Adds custom headers to the specified webhook configuration.
    /// </summary>
    /// <param name="service">The webhook notification service (unused but required for extension method).</param>
    /// <param name="webhookName">The name of the webhook to update.</param>
    /// <param name="headers">Dictionary of headers to add.</param>
    /// <returns>The number of headers that were added.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="webhookName"/> or <paramref name="headers"/> is null.</exception>
    public static int AddCustomHeaders(this WebhookNotificationService service, string webhookName, Dictionary<string, string> headers)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrEmpty(webhookName);
        ArgumentNullException.ThrowIfNull(headers);

        var webhook = service.GetWebhook(webhookName);
        if (webhook is null)
        {
            return 0;
        }

        if (webhook.CustomHeaders is null)
        {
            webhook.CustomHeaders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        var count = 0;
        foreach (var header in headers)
        {
            webhook.CustomHeaders[header.Key] = header.Value;
            count++;
        }

        return count;
    }

    /// <summary>
    /// Removes custom headers from the specified webhook configuration.
    /// </summary>
    /// <param name="service">The webhook notification service (unused but required for extension method).</param>
    /// <param name="webhookName">The name of the webhook to update.</param>
    /// <param name="headerNames">Collection of header names to remove.</param>
    /// <returns>The number of headers that were removed.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="webhookName"/> or <paramref name="headerNames"/> is null.</exception>
    public static int RemoveCustomHeaders(this WebhookNotificationService service, string webhookName, IEnumerable<string> headerNames)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrEmpty(webhookName);
        ArgumentNullException.ThrowIfNull(headerNames);

        var webhook = service.GetWebhook(webhookName);
        if (webhook?.CustomHeaders is null)
        {
            return 0;
        }

        var count = 0;
        foreach (var headerName in headerNames)
        {
            if (webhook.CustomHeaders.Remove(headerName))
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// Gets all webhook URLs as a read-only collection.
    /// </summary>
    /// <param name="service">The webhook notification service.</param>
    /// <returns>Read-only collection of webhook URLs.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null.</exception>
    public static IReadOnlyList<string> GetWebhookUrls(this WebhookNotificationService service)
    {
        ArgumentNullException.ThrowIfNull(service);

        return service.GetWebhooks().Select(config => config.Url).ToList().AsReadOnly();
    }

    /// <summary>
    /// Determines whether the webhook notification service has any webhooks registered.
    /// </summary>
    /// <param name="service">The webhook notification service.</param>
    /// <returns>True if at least one webhook is registered; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null.</exception>
    public static bool HasWebhooks(this WebhookNotificationService service)
    {
        ArgumentNullException.ThrowIfNull(service);

        return service.GetWebhookCount() > 0;
    }

    /// <summary>
    /// Gets a summary of webhook statistics.
    /// </summary>
    /// <param name="service">The webhook notification service.</param>
    /// <returns>A dictionary containing webhook statistics.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null.</exception>
    public static Dictionary<string, int> GetWebhookStatistics(this WebhookNotificationService service)
    {
        ArgumentNullException.ThrowIfNull(service);

        var stats = new Dictionary<string, int>
        {
            ["Total"] = service.GetWebhookCount(),
            ["Enabled"] = service.GetEnabledWebhooks().Count,
            ["Disabled"] = service.GetWebhookCount() - service.GetEnabledWebhooks().Count,
            ["Slack"] = service.GetWebhooksByType(WebhookType.Slack).Count,
            ["MicrosoftTeams"] = service.GetWebhooksByType(WebhookType.MicrosoftTeams).Count,
            ["Discord"] = service.GetWebhooksByType(WebhookType.Discord).Count,
            ["Custom"] = service.GetWebhooksByType(WebhookType.Custom).Count
        };

        return stats;
    }

    #region Private Helpers

    /// <summary>
    /// Internal method to get webhooks (reflection-friendly).
    /// </summary>
    private static List<WebhookConfiguration> GetWebhooksInternal(this WebhookNotificationService service)
    {
        // Use reflection to access the private _webhooks field
        var field = typeof(WebhookNotificationService).GetField("_webhooks", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (List<WebhookConfiguration>)field?.GetValue(service)!;
    }

    /// <summary>
    /// Determines if a webhook configuration should notify for a specific event type.
    /// </summary>
    private static bool ShouldNotifyForEvent(this WebhookConfiguration config, Type eventType)
    {
        return eventType switch
        {
            Type t when t == typeof(CriticalIssueDetectedEvent) => config.NotifyOnCriticalIssues,
            Type t when t == typeof(AnalysisFailedEvent) => config.NotifyOnFailures,
            Type t when t == typeof(AnalysisCompletedEvent) => config.NotifyOnCompletion,
            _ => false
        };
    }

    #endregion
}