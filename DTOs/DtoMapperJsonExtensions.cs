#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SqlQueryAnalyzer.DTOs;

/// <summary>
/// Provides JSON serialization extensions for DTO mapper types
/// </summary>
public static class DtoMapperJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>
    /// Serializes an AnalysisRequestDto to a JSON string
    /// </summary>
    /// <param name="value">The DTO to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation</param>
    /// <returns>A JSON string representation of the DTO</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static string ToJson(this AnalysisRequestDto value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes an AnalysisRequestDto from a JSON string
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <returns>The deserialized DTO</returns>
    /// <exception cref="ArgumentException">Thrown when json is null or empty</exception>
    /// <exception cref="JsonException">Thrown when JSON is invalid</exception>
    public static AnalysisRequestDto? FromJsonToAnalysisRequest(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<AnalysisRequestDto>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize an AnalysisRequestDto from a JSON string
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <param name="value">Receives the deserialized DTO if successful</param>
    /// <returns>True if deserialization succeeded; otherwise false</returns>
    public static bool TryFromJsonToAnalysisRequest(string json, out AnalysisRequestDto? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<AnalysisRequestDto>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }

    /// <summary>
    /// Serializes an AnalysisResponseDto to a JSON string
    /// </summary>
    /// <param name="value">The DTO to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation</param>
    /// <returns>A JSON string representation of the DTO</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static string ToJson(this AnalysisResponseDto value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes an AnalysisResponseDto from a JSON string
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <returns>The deserialized DTO</returns>
    /// <exception cref="ArgumentException">Thrown when json is null or empty</exception>
    /// <exception cref="JsonException">Thrown when JSON is invalid</exception>
    public static AnalysisResponseDto? FromJsonToAnalysisResponse(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<AnalysisResponseDto>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize an AnalysisResponseDto from a JSON string
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <param name="value">Receives the deserialized DTO if successful</param>
    /// <returns>True if deserialization succeeded; otherwise false</returns>
    public static bool TryFromJsonToAnalysisResponse(string json, out AnalysisResponseDto? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<AnalysisResponseDto>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }

    /// <summary>
    /// Serializes a PerformanceIssueDto to a JSON string
    /// </summary>
    /// <param name="value">The DTO to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation</param>
    /// <returns>A JSON string representation of the DTO</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static string ToJson(this PerformanceIssueDto value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a PerformanceIssueDto from a JSON string
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <returns>The deserialized DTO</returns>
    /// <exception cref="ArgumentException">Thrown when json is null or empty</exception>
    /// <exception cref="JsonException">Thrown when JSON is invalid</exception>
    public static PerformanceIssueDto? FromJsonToPerformanceIssue(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<PerformanceIssueDto>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a PerformanceIssueDto from a JSON string
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <param name="value">Receives the deserialized DTO if successful</param>
    /// <returns>True if deserialization succeeded; otherwise false</returns>
    public static bool TryFromJsonToPerformanceIssue(string json, out PerformanceIssueDto? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<PerformanceIssueDto>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }

    /// <summary>
    /// Serializes an IndexSuggestionDto to a JSON string
    /// </summary>
    /// <param name="value">The DTO to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation</param>
    /// <returns>A JSON string representation of the DTO</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static string ToJson(this IndexSuggestionDto value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes an IndexSuggestionDto from a JSON string
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <returns>The deserialized DTO</returns>
    /// <exception cref="ArgumentException">Thrown when json is null or empty</exception>
    /// <exception cref="JsonException">Thrown when JSON is invalid</exception>
    public static IndexSuggestionDto? FromJsonToIndexSuggestion(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<IndexSuggestionDto>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize an IndexSuggestionDto from a JSON string
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <param name="value">Receives the deserialized DTO if successful</param>
    /// <returns>True if deserialization succeeded; otherwise false</returns>
    public static bool TryFromJsonToIndexSuggestion(string json, out IndexSuggestionDto? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<IndexSuggestionDto>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }

    /// <summary>
    /// Serializes a BatchAnalysisRequestDto to a JSON string
    /// </summary>
    /// <param name="value">The DTO to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation</param>
    /// <returns>A JSON string representation of the DTO</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static string ToJson(this BatchAnalysisRequestDto value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a BatchAnalysisRequestDto from a JSON string
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <returns>The deserialized DTO</returns>
    /// <exception cref="ArgumentException">Thrown when json is null or empty</exception>
    /// <exception cref="JsonException">Thrown when JSON is invalid</exception>
    public static BatchAnalysisRequestDto? FromJsonToBatchAnalysisRequest(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<BatchAnalysisRequestDto>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a BatchAnalysisRequestDto from a JSON string
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <param name="value">Receives the deserialized DTO if successful</param>
    /// <returns>True if deserialization succeeded; otherwise false</returns>
    public static bool TryFromJsonToBatchAnalysisRequest(string json, out BatchAnalysisRequestDto? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<BatchAnalysisRequestDto>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }

    /// <summary>
    /// Serializes a BatchAnalysisResponseDto to a JSON string
    /// </summary>
    /// <param name="value">The DTO to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation</param>
    /// <returns>A JSON string representation of the DTO</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static string ToJson(this BatchAnalysisResponseDto value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a BatchAnalysisResponseDto from a JSON string
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <returns>The deserialized DTO</returns>
    /// <exception cref="ArgumentException">Thrown when json is null or empty</exception>
    /// <exception cref="JsonException">Thrown when JSON is invalid</exception>
    public static BatchAnalysisResponseDto? FromJsonToBatchAnalysisResponse(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<BatchAnalysisResponseDto>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a BatchAnalysisResponseDto from a JSON string
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <param name="value">Receives the deserialized DTO if successful</param>
    /// <returns>True if deserialization succeeded; otherwise false</returns>
    public static bool TryFromJsonToBatchAnalysisResponse(string json, out BatchAnalysisResponseDto? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<BatchAnalysisResponseDto>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }

    /// <summary>
    /// Serializes an IndexAnalysisRequestDto to a JSON string
    /// </summary>
    /// <param name="value">The DTO to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation</param>
    /// <returns>A JSON string representation of the DTO</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static string ToJson(this IndexAnalysisRequestDto value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes an IndexAnalysisRequestDto from a JSON string
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <returns>The deserialized DTO</returns>
    /// <exception cref="ArgumentException">Thrown when json is null or empty</exception>
    /// <exception cref="JsonException">Thrown when JSON is invalid</exception>
    public static IndexAnalysisRequestDto? FromJsonToIndexAnalysisRequest(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<IndexAnalysisRequestDto>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize an IndexAnalysisRequestDto from a JSON string
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <param name="value">Receives the deserialized DTO if successful</param>
    /// <returns>True if deserialization succeeded; otherwise false</returns>
    public static bool TryFromJsonToIndexAnalysisRequest(string json, out IndexAnalysisRequestDto? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<IndexAnalysisRequestDto>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }

    /// <summary>
    /// Serializes an IndexAnalysisResponseDto to a JSON string
    /// </summary>
    /// <param name="value">The DTO to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation</param>
    /// <returns>A JSON string representation of the DTO</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static string ToJson(this IndexAnalysisResponseDto value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes an IndexAnalysisResponseDto from a JSON string
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <returns>The deserialized DTO</returns>
    /// <exception cref="ArgumentException">Thrown when json is null or empty</exception>
    /// <exception cref="JsonException">Thrown when JSON is invalid</exception>
    public static IndexAnalysisResponseDto? FromJsonToIndexAnalysisResponse(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<IndexAnalysisResponseDto>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize an IndexAnalysisResponseDto from a JSON string
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <param name="value">Receives the deserialized DTO if successful</param>
       /// <returns>True if deserialization succeeded; otherwise false</returns>
    public static bool TryFromJsonToIndexAnalysisResponse(string json, out IndexAnalysisResponseDto? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<IndexAnalysisResponseDto>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }

    /// <summary>
    /// Serializes an IndexDetailDto to a JSON string
    /// </summary>
    /// <param name="value">The DTO to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation</param>
    /// <returns>A JSON string representation of the DTO</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static string ToJson(this IndexDetailDto value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes an IndexDetailDto from a JSON string
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <returns>The deserialized DTO</returns>
    /// <exception cref="ArgumentException">Thrown when json is null or empty</exception>
    /// <exception cref="JsonException">Thrown when JSON is invalid</exception>
    public static IndexDetailDto? FromJsonToIndexDetail(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<IndexDetailDto>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize an IndexDetailDto from a JSON string
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <param name="value">Receives the deserialized DTO if successful</param>
    /// <returns>True if deserialization succeeded; otherwise false</returns>
    public static bool TryFromJsonToIndexDetail(string json, out IndexDetailDto? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<IndexDetailDto>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}