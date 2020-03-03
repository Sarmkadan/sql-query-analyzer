// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;
using SqlQueryAnalyzer.Events;
using SqlQueryAnalyzer.Models;

namespace SqlQueryAnalyzer.Integration;

/// <summary>
/// Sends webhook notifications for important analysis events.
/// Integrates with external systems (Slack, Teams, Discord, custom APIs).
/// Implements retry logic and failure tracking.
/// </summary>
public class WebhookNotificationService : IAnalysisEventSubscriber
{
    private readonly ILogger<WebhookNotificationService> _logger;
    private readonly List<WebhookConfiguration> _webhooks = new();
    private readonly HttpClient _httpClient;

    public WebhookNotificationService(ILogger<WebhookNotificationService> logger)
    {
        _logger = logger;
        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
    }

    /// <summary>
    /// Registers a webhook endpoint for notifications.
    /// </summary>
    public void RegisterWebhook(WebhookConfiguration config)
    {
        if (string.IsNullOrEmpty(config.Url))
        {
            _logger.LogWarning("Cannot register webhook with empty URL");
            return;
        }

        _webhooks.Add(config);
        _logger.LogInformation($"Registered webhook: {config.Name} ({config.Type})");
    }

    /// <summary>
    /// Unregisters a webhook by name.
    /// </summary>
    public void UnregisterWebhook(string webhookName)
    {
        var removed = _webhooks.RemoveAll(w => w.Name == webhookName);
        if (removed > 0)
        {
            _logger.LogInformation($"Unregistered webhook: {webhookName}");
        }
    }

    /// <summary>
    /// Handles analysis events and sends relevant webhooks.
    /// Filters events based on webhook configuration.
    /// </summary>
    public async Task OnEventAsync(AnalysisEvent @event)
    {
        foreach (var webhook in _webhooks)
        {
            if (!ShouldNotifyWebhook(@event, webhook))
                continue;

            try
            {
                var payload = CreatePayload(@event, webhook);
                await SendWebhookAsync(webhook, payload);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send webhook to {webhook.Name}");
            }
        }
    }

    /// <summary>
    /// Determines if an event should trigger a webhook notification.
    /// </summary>
    private bool ShouldNotifyWebhook(AnalysisEvent @event, WebhookConfiguration config)
    {
        if (!config.Enabled)
            return false;

        return @event switch
        {
            CriticalIssueDetectedEvent => config.NotifyOnCriticalIssues,
            AnalysisFailedEvent => config.NotifyOnFailures,
            AnalysisCompletedEvent => config.NotifyOnCompletion,
            _ => false
        };
    }

    /// <summary>
    /// Creates webhook payload in the appropriate format.
    /// </summary>
    private string CreatePayload(AnalysisEvent @event, WebhookConfiguration config)
    {
        return config.Type switch
        {
            WebhookType.Slack => CreateSlackPayload(@event),
            WebhookType.MicrosoftTeams => CreateTeamsPayload(@event),
            WebhookType.Custom => CreateCustomPayload(@event),
            _ => throw new ArgumentException($"Unknown webhook type: {config.Type}")
        };
    }

    /// <summary>
    /// Creates Slack webhook payload format.
    /// </summary>
    private string CreateSlackPayload(AnalysisEvent @event)
    {
        var color = @event switch
        {
            CriticalIssueDetectedEvent => "#FF0000",
            AnalysisFailedEvent => "#FFA500",
            AnalysisCompletedEvent => "#00FF00",
            _ => "#808080"
        };

        var text = GetEventDescription(@event);

        return $$"""
        {
          "attachments": [{
            "color": "{{color}}",
            "title": "{{@event.EventType}}",
            "text": "{{text}}",
            "ts": {{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}}
          }]
        }
        """;
    }

    /// <summary>
    /// Creates Microsoft Teams webhook payload format.
    /// </summary>
    private string CreateTeamsPayload(AnalysisEvent @event)
    {
        var theme = @event switch
        {
            CriticalIssueDetectedEvent => "danger",
            AnalysisFailedEvent => "warning",
            AnalysisCompletedEvent => "success",
            _ => "default"
        };

        var text = GetEventDescription(@event);

        return $$"""
        {
          "@type": "MessageCard",
          "@context": "https://schema.org/extensions",
          "themeColor": "{{theme}}",
          "summary": "{{@event.EventType}}",
          "sections": [{
            "activityTitle": "{{@event.EventType}}",
            "text": "{{text}}"
          }]
        }
        """;
    }

    /// <summary>
    /// Creates generic JSON webhook payload.
    /// </summary>
    private string CreateCustomPayload(AnalysisEvent @event)
    {
        return $$"""
        {
          "event": "{{@event.EventType}}",
          "timestamp": "{{@event.Timestamp:O}}",
          "correlationId": "{{@event.CorrelationId}}",
          "data": {{@event switch {
            CriticalIssueDetectedEvent critical => $$"""
            {
              "queryId": "{{critical.QueryId}}",
              "issueType": "{{critical.IssueType}}",
              "description": "{{critical.Description}}",
              "impact": {{critical.ImpactPercentage}}
            }
            """,
            AnalysisCompletedEvent completed => $$"""
            {
              "queryId": "{{completed.QueryId}}",
              "score": {{completed.PerformanceScore}},
              "issuesFound": {{completed.IssuesFound}},
              "duration": {{completed.AnalysisDuration.TotalMilliseconds}}
            }
            """,
            AnalysisFailedEvent failed => $$"""
            {
              "queryId": "{{failed.QueryId}}",
              "error": "{{failed.ErrorMessage}}",
              "type": "{{failed.ExceptionType}}"
            }
            """,
            _ => "{}"
          }}}
        }
        """;
    }

    /// <summary>
    /// Gets human-readable description of an event.
    /// </summary>
    private string GetEventDescription(AnalysisEvent @event)
    {
        return @event switch
        {
            CriticalIssueDetectedEvent c => $"{c.IssueType}: {c.Description}",
            AnalysisCompletedEvent a => $"Score: {a.PerformanceScore:F1}/100, Issues: {a.IssuesFound}",
            AnalysisFailedEvent f => f.ErrorMessage,
            _ => "Analysis event"
        };
    }

    /// <summary>
    /// Sends webhook payload to endpoint with retry logic.
    /// </summary>
    private async Task SendWebhookAsync(WebhookConfiguration config, string payload, int retries = 3)
    {
        for (int attempt = 0; attempt < retries; attempt++)
        {
            try
            {
                var content = new StringContent(
                    payload,
                    System.Text.Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync(config.Url, content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogDebug($"Webhook sent successfully: {config.Name}");
                    return;
                }

                if ((int)response.StatusCode >= 500 && attempt < retries - 1)
                {
                    // Transient server error - retry
                    await Task.Delay(1000 * (attempt + 1));
                    continue;
                }

                _logger.LogWarning($"Webhook request failed: {config.Name} returned {response.StatusCode}");
            }
            catch (HttpRequestException ex) when (attempt < retries - 1)
            {
                _logger.LogWarning($"Webhook request failed: {ex.Message}. Retrying...");
                await Task.Delay(1000 * (attempt + 1));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send webhook: {config.Name}");
                return;
            }
        }

        _logger.LogError($"Webhook failed after {retries} attempts: {config.Name}");
    }

    /// <summary>
    /// Gets count of registered webhooks.
    /// </summary>
    public int GetWebhookCount() => _webhooks.Count;
}

/// <summary>
/// Configuration for a webhook endpoint.
/// </summary>
public class WebhookConfiguration
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public WebhookType Type { get; set; } = WebhookType.Custom;
    public bool Enabled { get; set; } = true;
    public bool NotifyOnCompletion { get; set; } = true;
    public bool NotifyOnCriticalIssues { get; set; } = true;
    public bool NotifyOnFailures { get; set; } = true;
    public Dictionary<string, string>? CustomHeaders { get; set; }
}

/// <summary>
/// Type of webhook service.
/// </summary>
public enum WebhookType
{
    Slack,
    MicrosoftTeams,
    Discord,
    Custom
}
