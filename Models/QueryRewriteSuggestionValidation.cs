#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace SqlQueryAnalyzer.Models;

/// <summary>
/// Provides validation helpers for <see cref="QueryRewriteSuggestion"/> instances.
/// </summary>
public static class QueryRewriteSuggestionValidation
{
    /// <summary>
    /// Validates a <see cref="QueryRewriteSuggestion"/> and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The suggestion to validate.</param>
    /// <returns>An empty list if valid; otherwise, a list of error messages.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/></exception>
    public static IReadOnlyList<string> Validate(this QueryRewriteSuggestion value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate SuggestionId
        if (string.IsNullOrWhiteSpace(value.SuggestionId))
        {
            errors.Add("SuggestionId must not be null or whitespace.");
        }
        else if (!Guid.TryParse(value.SuggestionId, out _))
        {
            errors.Add("SuggestionId must be a valid GUID.");
        }

        // Validate OriginalQuery
        if (string.IsNullOrWhiteSpace(value.OriginalQuery))
        {
            errors.Add("OriginalQuery must not be null or whitespace.");
        }

        // Validate RewrittenQuery
        if (string.IsNullOrWhiteSpace(value.RewrittenQuery))
        {
            errors.Add("RewrittenQuery must not be null or whitespace.");
        }
        else if (string.Equals(value.RewrittenQuery, value.OriginalQuery, StringComparison.Ordinal))
        {
            errors.Add("RewrittenQuery must differ from OriginalQuery.");
        }

        // Validate RewriteType
        if (!Enum.IsDefined(value.RewriteType))
        {
            errors.Add("RewriteType must be a valid enum value.");
        }

        // Validate AffectedClause
        if (string.IsNullOrWhiteSpace(value.AffectedClause))
        {
            errors.Add("AffectedClause must not be null or whitespace.");
        }

        // Validate Rationale
        if (string.IsNullOrWhiteSpace(value.Rationale))
        {
            errors.Add("Rationale must not be null or whitespace.");
        }

        // Validate AdditionalNotes
        // AdditionalNotes can be empty, but if present should not be whitespace-only
        if (value.AdditionalNotes is not null && string.IsNullOrWhiteSpace(value.AdditionalNotes))
        {
            errors.Add("AdditionalNotes must not be whitespace-only if provided.");
        }

        // Validate EstimatedImprovementPercent
        if (value.EstimatedImprovementPercent < 0 || value.EstimatedImprovementPercent > 100)
        {
            errors.Add("EstimatedImprovementPercent must be between 0 and 100 inclusive.");
        }

        // Validate Priority
        if (value.Priority < 1 || value.Priority > 10)
        {
            errors.Add("Priority must be between 1 and 10 inclusive.");
        }

        // Validate RelatedIndexSuggestions
        if (value.RelatedIndexSuggestions is null)
        {
            errors.Add("RelatedIndexSuggestions must not be null.");
        }

        // Validate GeneratedAt
        if (value.GeneratedAt == default)
        {
            errors.Add("GeneratedAt must be set to a non-default DateTime.");
        }
        else if (value.GeneratedAt.Kind is not DateTimeKind.Utc)
        {
            errors.Add("GeneratedAt must be in UTC.");
        }

        return errors.AsReadOnly();
    }


    /// <summary>
    /// Checks if a <see cref="QueryRewriteSuggestion"/> is valid.
    /// </summary>
    /// <param name="value">The suggestion to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/></exception>
    public static bool IsValid(this QueryRewriteSuggestion value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="QueryRewriteSuggestion"/> is valid, throwing an exception if it is not.
    /// </summary>
    /// <param name="value">The suggestion to validate.</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException"><paramref name="value"/> is invalid; the exception message lists all validation errors.</exception>
    public static void EnsureValid(this QueryRewriteSuggestion value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"QueryRewriteSuggestion is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }
}