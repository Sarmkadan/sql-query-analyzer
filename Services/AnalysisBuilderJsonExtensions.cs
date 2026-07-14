#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using SqlQueryAnalyzer.DTOs;

namespace SqlQueryAnalyzer.Services;

/// <summary>
/// Provides System.Text.Json serialization extensions for AnalysisBuilder
/// </summary>
public static class AnalysisBuilderJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Serializes the AnalysisBuilder to a JSON string
    /// </summary>
    /// <param name="value">The AnalysisBuilder instance to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation</param>
    /// <returns>A JSON string representation of the AnalysisBuilder</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static string ToJson(this AnalysisBuilder value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value.Build(), options);
    }

    /// <summary>
    /// Deserializes an AnalysisRequestDto from a JSON string and creates an AnalysisBuilder
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <returns>An AnalysisBuilder instance configured with the deserialized data</returns>
    /// <exception cref="ArgumentException">Thrown when json is null or empty</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid</exception>
    public static AnalysisBuilder? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        var dto = JsonSerializer.Deserialize<AnalysisRequestDto>(json, _jsonOptions);
        if (dto is null)
        {
            return null;
        }

        var builder = new AnalysisBuilder();

        // Reconstruct the builder state from the DTO
        if (!string.IsNullOrWhiteSpace(dto.QueryText))
        {
            builder.WithQuery(dto.QueryText);
        }

        if (!string.IsNullOrWhiteSpace(dto.ApplicationName))
        {
            builder.WithApplication(dto.ApplicationName);
        }

        if (!string.IsNullOrWhiteSpace(dto.ProcedureName))
        {
            builder.WithProcedure(dto.ProcedureName);
        }

        if (!string.IsNullOrWhiteSpace(dto.ModuleName))
        {
            builder.WithModule(dto.ModuleName);
        }

        if (dto.IncludeIndexSuggestions)
        {
            builder.IncludeIndexSuggestions(true);
        }

        if (dto.AnalyzeFragmentation)
        {
            builder.AnalyzeFragmentation(true);
        }

        if (dto.AnalyzePlan)
        {
            builder.AnalyzePlan(true);
        }

        if (!string.IsNullOrWhiteSpace(dto.ExecutionPlanXml))
        {
            builder.WithExecutionPlan(dto.ExecutionPlanXml);
        }

        return builder;
    }

    /// <summary>
    /// Attempts to deserialize an AnalysisRequestDto from a JSON string and creates an AnalysisBuilder
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <param name="value">Receives the AnalysisBuilder instance if successful</param>
    /// <returns>True if deserialization succeeded; otherwise, false</returns>
    public static bool TryFromJson(string json, out AnalysisBuilder? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = FromJson(json);
            return value is not null;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}