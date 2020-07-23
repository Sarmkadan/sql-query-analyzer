using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using SqlQueryAnalyzer.Models;

namespace SqlQueryAnalyzer.Models;

/// <summary>
/// Provides extension methods for the <see cref="IndexSuggestion"/> class.
/// </summary>
public static class IndexSuggestionExtensions
{
    /// <summary>
    /// Returns a comprehensive list of all columns involved in the index, including both key and included columns.
    /// </summary>
    /// <param name="suggestion">The index suggestion.</param>
    /// <returns>A read-only list of all column names.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="suggestion"/> is null.</exception>
    public static IReadOnlyList<string> GetAllColumns(this IndexSuggestion suggestion)
    {
        ArgumentNullException.ThrowIfNull(suggestion);
        return suggestion.IndexColumns.Concat(suggestion.IncludeColumns).ToList().AsReadOnly();
    }

    /// <summary>
    /// Determines if the index suggestion has a performance gain meeting or exceeding the specified threshold.
    /// </summary>
    /// <param name="suggestion">The index suggestion.</param>
    /// <param name="threshold">The performance gain threshold (0-100).</param>
    /// <returns>True if the gain is greater than or equal to the threshold; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="suggestion"/> is null.</exception>
    public static bool HasSignificantGain(this IndexSuggestion suggestion, double threshold)
    {
        ArgumentNullException.ThrowIfNull(suggestion);
        return suggestion.EstimatedPerformanceGain >= threshold;
    }

    /// <summary>
    /// Returns a formatted string representation of the index suggestion for UI or diagnostic display.
    /// </summary>
    /// <param name="suggestion">The index suggestion.</param>
    /// <returns>A formatted string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="suggestion"/> is null.</exception>
    public static string ToDisplayString(this IndexSuggestion suggestion)
    {
        ArgumentNullException.ThrowIfNull(suggestion);
        return string.Format(CultureInfo.InvariantCulture,
            "{0} ({1}) on {2}. Est. Gain: {3:F1}%",
            suggestion.IndexName,
            suggestion.IndexType,
            suggestion.TableName,
            suggestion.EstimatedPerformanceGain);
    }
}
