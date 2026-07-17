#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlQueryAnalyzer.Utilities;

/// <summary>
/// Provides validation helpers for <see cref="QueryCacheKeyGenerator"/> instances.
/// Validates all public members and ensures they meet expected constraints.
/// </summary>
public static class QueryCacheKeyGeneratorValidation
{
    /// <summary>
    /// Validates a <see cref="QueryCacheKeyGenerator"/> instance.
    /// Returns a list of human-readable validation problems.
    /// </summary>
    /// <param name="value">The generator to validate.</param>
    /// <returns>An empty list if valid, otherwise a list of validation errors.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this QueryCacheKeyGenerator value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate GenerateQueryKey behavior
        try
        {
            var queryKey = value.GenerateQueryKey("SELECT * FROM table");
            if (string.IsNullOrEmpty(queryKey))
            {
                errors.Add("GenerateQueryKey returned null or empty string");
            }
            else if (!value.IsValidCacheKey(queryKey))
            {
                errors.Add("GenerateQueryKey produced an invalid cache key format");
            }
        }
        catch (Exception ex)
        {
            errors.Add($"GenerateQueryKey threw exception: {ex.GetType().Name}: {ex.Message}");
        }

        // Validate GenerateResultKey behavior
        try
        {
            var resultKey = value.GenerateResultKey("SELECT * FROM table");
            if (string.IsNullOrEmpty(resultKey))
            {
                errors.Add("GenerateResultKey returned null or empty string");
            }
            else if (!value.IsValidCacheKey(resultKey))
            {
                errors.Add("GenerateResultKey produced an invalid cache key format");
            }
        }
        catch (Exception ex)
        {
            errors.Add($"GenerateResultKey threw exception: {ex.GetType().Name}: {ex.Message}");
        }

        // Validate GenerateMetadataKey behavior
        try
        {
            var metadataKey = value.GenerateMetadataKey("SELECT * FROM table");
            if (string.IsNullOrEmpty(metadataKey))
            {
                errors.Add("GenerateMetadataKey returned null or empty string");
            }
            else if (!value.IsValidCacheKey(metadataKey))
            {
                errors.Add("GenerateMetadataKey produced an invalid cache key format");
            }
        }
        catch (Exception ex)
        {
            errors.Add($"GenerateMetadataKey threw exception: {ex.GetType().Name}: {ex.Message}");
        }

        // Validate GenerateBatchKey behavior
        try
        {
            var batchKey = value.GenerateBatchKey(["SELECT * FROM table"]);
            if (string.IsNullOrEmpty(batchKey))
            {
                errors.Add("GenerateBatchKey returned null or empty string");
            }
            else if (!value.IsValidCacheKey(batchKey))
            {
                errors.Add("GenerateBatchKey produced an invalid cache key format");
            }
        }
        catch (Exception ex)
        {
            errors.Add($"GenerateBatchKey threw exception: {ex.GetType().Name}: {ex.Message}");
        }

        // Validate IsValidCacheKey behavior
        try
        {
            var validKey = value.GenerateQueryKey("SELECT 1");
            var invalidKey = "invalid-key";

            if (!value.IsValidCacheKey(validKey))
            {
                errors.Add("IsValidCacheKey returned false for a valid key");
            }

            if (value.IsValidCacheKey(invalidKey))
            {
                errors.Add("IsValidCacheKey returned true for an invalid key");
            }

            if (value.IsValidCacheKey(null))
            {
                errors.Add("IsValidCacheKey returned true for null input");
            }

            if (value.IsValidCacheKey(string.Empty))
            {
                errors.Add("IsValidCacheKey returned true for empty string input");
            }

            if (value.IsValidCacheKey("   "))
            {
                errors.Add("IsValidCacheKey returned true for whitespace-only input");
            }
        }
        catch (Exception ex)
        {
            errors.Add($"IsValidCacheKey validation threw exception: {ex.GetType().Name}: {ex.Message}");
        }

        // Validate ExtractHashFromKey behavior
        try
        {
            var validKey = value.GenerateQueryKey("SELECT * FROM table");
            var hash = value.ExtractHashFromKey(validKey);

            if (hash == null)
            {
                errors.Add("ExtractHashFromKey returned null for a valid key");
            }
            else if (hash.Length != 64)
            {
                errors.Add("ExtractHashFromKey returned a hash with incorrect length (expected 64 hex characters)");
            }
            else if (!IsHexString(hash))
            {
                errors.Add("ExtractHashFromKey returned a non-hexadecimal string");
            }

            var invalidHash = value.ExtractHashFromKey("invalid-key");
            if (invalidHash != null)
            {
                errors.Add("ExtractHashFromKey returned a value for an invalid key");
            }

            var nullHash = value.ExtractHashFromKey(null);
            if (nullHash != null)
            {
                errors.Add("ExtractHashFromKey returned a value for null input");
            }
        }
        catch (Exception ex)
        {
            errors.Add($"ExtractHashFromKey validation threw exception: {ex.GetType().Name}: {ex.Message}");
        }

        // Validate GetKeyType behavior
        try
        {
            var queryKey = value.GenerateQueryKey("SELECT 1");
            var resultKey = value.GenerateResultKey("SELECT 1");
            var metadataKey = value.GenerateMetadataKey("SELECT 1");
            var batchKey = value.GenerateBatchKey(["SELECT 1"]);
            var unknownKey = "unknown-key";

            if (value.GetKeyType(queryKey) != CacheKeyType.Query)
            {
                errors.Add("GetKeyType returned incorrect type for Query key");
            }

            if (value.GetKeyType(resultKey) != CacheKeyType.Result)
            {
                errors.Add("GetKeyType returned incorrect type for Result key");
            }

            if (value.GetKeyType(metadataKey) != CacheKeyType.Metadata)
            {
                errors.Add("GetKeyType returned incorrect type for Metadata key");
            }

            if (value.GetKeyType(batchKey) != CacheKeyType.Batch)
            {
                errors.Add("GetKeyType returned incorrect type for Batch key");
            }

            if (value.GetKeyType(unknownKey) != CacheKeyType.Unknown)
            {
                errors.Add("GetKeyType returned incorrect type for Unknown key");
            }
        }
        catch (Exception ex)
        {
            errors.Add($"GetKeyType validation threw exception: {ex.GetType().Name}: {ex.Message}");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="QueryCacheKeyGenerator"/> is valid.
    /// </summary>
    /// <param name="value">The generator to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this QueryCacheKeyGenerator value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures the specified <see cref="QueryCacheKeyGenerator"/> is valid.
    /// Throws an <see cref="ArgumentException"/> with a detailed message listing all validation problems.
    /// </summary>
    /// <param name="value">The generator to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the generator is invalid, containing all validation errors.</exception>
    public static void EnsureValid(this QueryCacheKeyGenerator value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"QueryCacheKeyGenerator is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
    }

    /// <summary>
    /// Checks if a string consists only of hexadecimal characters.
    /// </summary>
    /// <param name="input">The string to validate.</param>
    /// <returns><see langword="true"/> if the string contains only hexadecimal characters; otherwise, <see langword="false"/>.</returns>
    private static bool IsHexString(string input)
    {
        return !string.IsNullOrEmpty(input) && input.All(c => Uri.IsHexDigit(c));
    }
}