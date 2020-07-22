using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SqlQueryAnalyzer.Models
{
    /// <summary>
    /// Extension methods for <see cref="IndexRecommendation"/>.
    /// </summary>
    public static class IndexRecommendationExtensions
    {
        /// <summary>
        /// Returns a concise, human‑readable summary of the recommendation.
        /// </summary>
        /// <param name="recommendation">The recommendation to summarize.</param>
        /// <returns>A formatted string containing table name, index type and impact score.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="recommendation"/> is <c>null</c>.</exception>
        public static string GetSummary(this IndexRecommendation recommendation)
        {
            ArgumentNullException.ThrowIfNull(recommendation);
            return string.Format(
                CultureInfo.InvariantCulture,
                "Table: {0}, Index Type: {1}, Impact: {2:P1}",
                recommendation.TableName,
                recommendation.IndexType,
                recommendation.ImpactScore);
        }

        /// <summary>
        /// Returns a read‑only collection of all columns involved in the index (key columns followed by include columns).
        /// </summary>
        /// <param name="recommendation">The recommendation whose columns are to be retrieved.</param>
        /// <returns>An <see cref="IReadOnlyList{T}"/> of column names.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="recommendation"/> is <c>null</c>.</exception>
        public static IReadOnlyList<string> GetAllColumns(this IndexRecommendation recommendation)
        {
            ArgumentNullException.ThrowIfNull(recommendation);
            // Preserve order: key columns first, then include columns.
            return recommendation.KeyColumns
                .Concat(recommendation.IncludeColumns ?? Enumerable.Empty<string>())
                .ToList()
                .AsReadOnly();
        }

        /// <summary>
        /// Determines whether the recommendation is considered high impact.
        /// </summary>
        /// <param name="recommendation">The recommendation to evaluate.</param>
        /// <param name="threshold">
        /// The impact score threshold above which the recommendation is regarded as high impact.
        /// Defaults to <c>0.75</c>.
        /// </param>
        /// <returns><c>true</c> if <see cref="IndexRecommendation.ImpactScore"/> is greater than or equal to <paramref name="threshold"/>; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="recommendation"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="threshold"/> is not between 0 and 1.</exception>
        public static bool IsHighImpact(this IndexRecommendation recommendation, double threshold = 0.75)
        {
            ArgumentNullException.ThrowIfNull(recommendation);
            if (threshold < 0.0 || threshold > 1.0)
                throw new ArgumentException("Threshold must be between 0 and 1.", nameof(threshold));

            return recommendation.ImpactScore >= threshold;
        }

        /// <summary>
        /// Ensures that the <see cref="IndexRecommendation.GeneratedScript"/> property is populated.
        /// If the script is empty, the method invokes <see cref="IndexRecommendation.GenerateScript"/> to produce it.
        /// </summary>
        /// <param name="recommendation">The recommendation to process.</param>
        /// <returns>The generated T‑SQL script.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="recommendation"/> is <c>null</c>.</exception>
        public static string EnsureScriptGenerated(this IndexRecommendation recommendation)
        {
            ArgumentNullException.ThrowIfNull(recommendation);
            if (string.IsNullOrWhiteSpace(recommendation.GeneratedScript))
                recommendation.GenerateScript();

            return recommendation.GeneratedScript;
        }
    }
}
