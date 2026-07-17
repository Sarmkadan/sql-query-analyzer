#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Text.Json;

namespace SqlQueryAnalyzer.Services;

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="QueryPlanAnalyzerService"/>.
/// <para><strong>Note:</strong> This class serializes service instances that contain dependency injection
/// components (e.g., <see cref="Microsoft.Extensions.Logging.ILogger"/>). The serialized JSON will contain
/// null values for injected dependencies. For serialization of actual analysis results, use the appropriate
/// model classes (e.g., <see cref="SqlQueryAnalyzer.Models.QueryPlan"/>, <see cref="SqlQueryAnalyzer.Models.PerformanceIssue"/>).</para>
/// </summary>
public static class QueryPlanAnalyzerServiceJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonSerializerOptions.Web.DefaultIgnoreCondition
    };

    /// <summary>
    /// Serializes the <see cref="QueryPlanAnalyzerService"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The service instance to serialize. Cannot be null.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the service instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this QueryPlanAnalyzerService value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented ? GetIndentedOptions() : _jsonOptions;
        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to an <see cref="QueryPlanAnalyzerService"/> instance.
    /// <para><strong>Note:</strong> Deserialization will produce a service instance with null dependencies.
    /// This is typically not useful for production scenarios. Consider using dependency injection instead.</para>
    /// </summary>
    /// <param name="json">The JSON string to deserialize. Cannot be null or empty.</param>
    /// <returns>The deserialized service instance, or null if deserialization fails.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    public static QueryPlanAnalyzerService? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            return JsonSerializer.Deserialize<QueryPlanAnalyzerService>(json, _jsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to an <see cref="QueryPlanAnalyzerService"/> instance.
    /// <para><strong>Note:</strong> The deserialized instance will have null dependencies.
    /// This method is provided for completeness but has limited practical use.</para>
    /// </summary>
    /// <param name="json">The JSON string to deserialize. Cannot be null or empty.</param>
    /// <param name="value">The deserialized service instance, or null if deserialization fails.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    public static bool TryFromJson(string json, out QueryPlanAnalyzerService? value)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<QueryPlanAnalyzerService>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }

    /// <summary>
    /// Gets a pre-configured JsonSerializerOptions with indentation enabled.
    /// </summary>
    /// <returns>A new JsonSerializerOptions instance with indentation enabled.</returns>
    private static JsonSerializerOptions GetIndentedOptions()
        => new JsonSerializerOptions(_jsonOptions) { WriteIndented = true };
}