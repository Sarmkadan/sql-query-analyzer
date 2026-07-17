#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SqlQueryAnalyzer.Integration;

/// <summary>
/// Provides validation helpers for <see cref="WebhookNotificationService"/> and <see cref="WebhookConfiguration"/>.
/// </summary>
public static class WebhookNotificationServiceValidation
{
    /// <summary>
    /// Validates a <see cref="WebhookConfiguration"/> instance.
    /// </summary>
    /// <param name="config">The webhook configuration to validate.</param>
    /// <returns>A list of validation errors; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if config is null.</exception>
    public static IReadOnlyList<string> Validate(this WebhookConfiguration? config)
    {
        ArgumentNullException.ThrowIfNull(config);

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(config.Name))
        {
            errors.Add("Webhook name cannot be null or whitespace.");
        }
        else if (config.Name.Length > 100)
        {
            errors.Add("Webhook name cannot exceed 100 characters.");
        }

        if (string.IsNullOrWhiteSpace(config.Url))
        {
            errors.Add("Webhook URL cannot be null or whitespace.");
        }
        else if (!Uri.TryCreate(config.Url, UriKind.Absolute, out _))
        {
            errors.Add("Webhook URL must be a valid absolute URI.");
        }
        else if (!config.Url.StartsWith("https://", StringComparison.OrdinalIgnoreCase) &&
                 !config.Url.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
        {
            errors.Add("Webhook URL should use http:// or https:// scheme.");
        }

        if (config.Type < WebhookType.Slack || config.Type > WebhookType.Custom)
        {
            errors.Add($"Webhook type must be a valid value between {WebhookType.Slack} and {WebhookType.Custom}.");
        }

        if (config.CustomHeaders is { Count: > 50 })
        {
            errors.Add("Custom headers cannot exceed 50 entries.");
        }

        if (config.CustomHeaders?.Count > 0)
        {
            foreach (var header in config.CustomHeaders)
            {
                if (string.IsNullOrWhiteSpace(header.Key))
                {
                    errors.Add("Custom header key cannot be null or whitespace.");
                }

                if (string.IsNullOrWhiteSpace(header.Value))
                {
                    errors.Add("Custom header value cannot be null or whitespace.");
                }

                if (header.Key?.Length > 100)
                {
                    errors.Add("Custom header key cannot exceed 100 characters.");
                }

                if (header.Value?.Length > 500)
                {
                    errors.Add("Custom header value cannot exceed 500 characters.");
                }
            }
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="WebhookConfiguration"/> instance is valid.
    /// </summary>
    /// <param name="config">The webhook configuration to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    public static bool IsValid(this WebhookConfiguration? config) => Validate(config).Count == 0;

    /// <summary>
    /// Ensures that a <see cref="WebhookConfiguration"/> instance is valid, throwing an <see cref="ArgumentException"/> if not.
    /// </summary>
    /// <param name="config">The webhook configuration to validate.</param>
    /// <exception cref="ArgumentException">Thrown if config is null or contains validation errors.</exception>
    public static void EnsureValid(this WebhookConfiguration? config)
    {
        ArgumentNullException.ThrowIfNull(config);

        var errors = Validate(config);
        if (errors.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"Webhook configuration is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
    }
}
