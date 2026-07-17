using System;
using System.Collections.Generic;
using System.Linq;
using SqlQueryAnalyzer.Models;

namespace SqlQueryAnalyzer.Extensions
{
    /// <summary>
    /// Provides validation helpers for <see cref="QueryRewriteExtensions"/> extension methods.
    /// </summary>
    public static class QueryRewriteExtensionsValidation
    {
        /// <summary>
        /// Validates the results of <see cref="QueryRewriteExtensions.GetAutoApplicable"/>.
        /// </summary>
        /// <param name="suggestions">The suggestions to validate.</param>
        /// <returns>A list of human-readable validation problems; empty if valid.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="suggestions"/> is <see langword="null"/></exception>
        public static IReadOnlyList<string> Validate(this IEnumerable<QueryRewriteSuggestion> suggestions)
        {
            ArgumentNullException.ThrowIfNull(suggestions);

            var problems = new List<string>();

            var list = suggestions.ToList();
            if (list.Count == 0)
            {
                problems.Add("Suggestions collection is empty");
                return problems.AsReadOnly();
            }

            // Validate each suggestion
            for (int i = 0; i < list.Count; i++)
            {
                var suggestion = list[i];
                if (suggestion == null)
                {
                    problems.Add($"Suggestion at index {i} is null");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(suggestion.Rationale))
                {
                    problems.Add($"Suggestion at index {i} has null or empty Rationale");
                }

                if (suggestion.RewriteType == default)
                {
                    problems.Add($"Suggestion at index {i} has default RewriteType");
                }

                if (suggestion.EstimatedImprovementPercent < 0 || suggestion.EstimatedImprovementPercent > 100)
                {
                    problems.Add($"Suggestion at index {i} has out-of-range EstimatedImprovementPercent: {suggestion.EstimatedImprovementPercent}");
                }

                if (suggestion.RelatedIndexSuggestions == null)
                {
                    problems.Add($"Suggestion at index {i} has null RelatedIndexSuggestions");
                }
            }

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Validates the results of <see cref="QueryRewriteExtensions.GetAutoApplicable"/>.
        /// </summary>
        /// <param name="suggestions">The suggestions to validate.</param>
        /// <returns>A list of human-readable validation problems; empty if valid.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="suggestions"/> is <see langword="null"/></exception>
        public static IReadOnlyList<string> ValidateAutoApplicable(this IEnumerable<QueryRewriteSuggestion> suggestions)
        {
            ArgumentNullException.ThrowIfNull(suggestions);

            var problems = new List<string>();

            var result = QueryRewriteExtensions.GetAutoApplicable(suggestions);
            if (result == null)
            {
                problems.Add("GetAutoApplicable() returned null");
                return problems.AsReadOnly();
            }

            for (int i = 0; i < result.Count; i++)
            {
                var suggestion = result[i];
                if (suggestion == null)
                {
                    problems.Add($"Auto-applicable suggestion at index {i} is null");
                    continue;
                }

                if (!suggestion.IsAutoApplicable)
                {
                    problems.Add($"Suggestion at index {i} is in GetAutoApplicable() result but IsAutoApplicable is false");
                }
            }

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Validates the results of <see cref="QueryRewriteExtensions.GetNonBreaking"/>.
        /// </summary>
        /// <param name="suggestions">The suggestions to validate.</param>
        /// <returns>A list of human-readable validation problems; empty if valid.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="suggestions"/> is <see langword="null"/></exception>
        public static IReadOnlyList<string> ValidateNonBreaking(this IEnumerable<QueryRewriteSuggestion> suggestions)
        {
            ArgumentNullException.ThrowIfNull(suggestions);

            var problems = new List<string>();

            var result = QueryRewriteExtensions.GetNonBreaking(suggestions);
            if (result == null)
            {
                problems.Add("GetNonBreaking() returned null");
                return problems.AsReadOnly();
            }

            for (int i = 0; i < result.Count; i++)
            {
                var suggestion = result[i];
                if (suggestion == null)
                {
                    problems.Add($"Non-breaking suggestion at index {i} is null");
                    continue;
                }

                if (suggestion.IsBreakingChange)
                {
                    problems.Add($"Suggestion at index {i} is in GetNonBreaking() result but IsBreakingChange is true");
                }
            }

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Validates the results of <see cref="QueryRewriteExtensions.OfType"/>.
        /// </summary>
        /// <param name="suggestions">The suggestions to validate.</param>
        /// <param name="rewriteType">The rewrite type to filter by.</param>
        /// <returns>A list of human-readable validation problems; empty if valid.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="suggestions"/> is <see langword="null"/></exception>
        public static IReadOnlyList<string> ValidateOfType(this IEnumerable<QueryRewriteSuggestion> suggestions, RewriteType rewriteType)
        {
            ArgumentNullException.ThrowIfNull(suggestions);

            var problems = new List<string>();

            var result = QueryRewriteExtensions.OfType(suggestions, rewriteType);
            if (result == null)
            {
                problems.Add("OfType() returned null");
                return problems.AsReadOnly();
            }

            // Validate that all results have the correct type
            for (int i = 0; i < result.Count; i++)
            {
                var suggestion = result[i];
                if (suggestion == null)
                {
                    problems.Add($"OfType result at index {i} is null");
                    continue;
                }

                if (suggestion.RewriteType != rewriteType)
                {
                    problems.Add($"Suggestion at index {i} has RewriteType {suggestion.RewriteType} but expected {rewriteType}");
                }
            }

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Validates the results of <see cref="QueryRewriteExtensions.ForClause"/>.
        /// </summary>
        /// <param name="suggestions">The suggestions to validate.</param>
        /// <param name="clause">The clause to filter by.</param>
        /// <returns>A list of human-readable validation problems; empty if valid.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="suggestions"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentException"><paramref name="clause"/> is null or whitespace</exception>
        public static IReadOnlyList<string> ValidateForClause(this IEnumerable<QueryRewriteSuggestion> suggestions, string clause)
        {
            ArgumentNullException.ThrowIfNull(suggestions);
            ArgumentException.ThrowIfNullOrWhiteSpace(clause);

            var problems = new List<string>();

            var result = QueryRewriteExtensions.ForClause(suggestions, clause);
            if (result == null)
            {
                problems.Add("ForClause() returned null");
                return problems.AsReadOnly();
            }

            // Validate that all results have the correct affected clause
            for (int i = 0; i < result.Count; i++)
            {
                var suggestion = result[i];
                if (suggestion == null)
                {
                    problems.Add($"ForClause result at index {i} is null");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(suggestion.AffectedClause))
                {
                    problems.Add($"Suggestion at index {i} has null or empty AffectedClause");
                }
                else if (!suggestion.AffectedClause.Equals(clause, StringComparison.OrdinalIgnoreCase))
                {
                    problems.Add($"Suggestion at index {i} has AffectedClause '{suggestion.AffectedClause}' but expected '{clause}'");
                }
            }

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Validates the results of <see cref="QueryRewriteExtensions.OrderByImpact"/>.
        /// </summary>
        /// <param name="suggestions">The suggestions to validate.</param>
        /// <returns>A list of human-readable validation problems; empty if valid.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="suggestions"/> is <see langword="null"/></exception>
        public static IReadOnlyList<string> ValidateOrderByImpact(this IEnumerable<QueryRewriteSuggestion> suggestions)
        {
            ArgumentNullException.ThrowIfNull(suggestions);

            var problems = new List<string>();

            var result = QueryRewriteExtensions.OrderByImpact(suggestions);
            if (result == null)
            {
                problems.Add("OrderByImpact() returned null");
                return problems.AsReadOnly();
            }

            var orderedList = result.ToList();
            // Validate ordering is correct (descending by EstimatedImprovementPercent)
            double prevImprovement = double.MaxValue;
            for (int i = 0; i < orderedList.Count; i++)
            {
                var suggestion = orderedList[i];
                if (suggestion == null)
                {
                    problems.Add($"Ordered suggestion at index {i} is null");
                    continue;
                }

                if (suggestion.EstimatedImprovementPercent > prevImprovement)
                {
                    problems.Add($"Suggestion at index {i} has EstimatedImprovementPercent {suggestion.EstimatedImprovementPercent} which is greater than previous {prevImprovement} - ordering is incorrect");
                }
                prevImprovement = suggestion.EstimatedImprovementPercent;
            }

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Validates the results of <see cref="QueryRewriteExtensions.GetTotalEstimatedImprovement"/>.
        /// </summary>
        /// <param name="suggestions">The suggestions to validate.</param>
        /// <returns>A list of human-readable validation problems; empty if valid.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="suggestions"/> is <see langword="null"/></exception>
        public static IReadOnlyList<string> ValidateTotalEstimatedImprovement(this IEnumerable<QueryRewriteSuggestion> suggestions)
        {
            ArgumentNullException.ThrowIfNull(suggestions);

            var problems = new List<string>();

            var result = QueryRewriteExtensions.GetTotalEstimatedImprovement(suggestions);
            if (double.IsNaN(result) || double.IsInfinity(result))
            {
                problems.Add($"GetTotalEstimatedImprovement() returned {result}");
            }
            else if (result < 0 || result > 100)
            {
                problems.Add($"GetTotalEstimatedImprovement() returned out-of-range value: {result}");
            }

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Validates the results of <see cref="QueryRewriteExtensions.GetAllIndexSuggestions"/>.
        /// </summary>
        /// <param name="suggestions">The suggestions to validate.</param>
        /// <returns>A list of human-readable validation problems; empty if valid.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="suggestions"/> is <see langword="null"/></exception>
        public static IReadOnlyList<string> ValidateAllIndexSuggestions(this IEnumerable<QueryRewriteSuggestion> suggestions)
        {
            ArgumentNullException.ThrowIfNull(suggestions);

            var problems = new List<string>();

            var result = QueryRewriteExtensions.GetAllIndexSuggestions(suggestions);
            if (result == null)
            {
                problems.Add("GetAllIndexSuggestions() returned null");
                return problems.AsReadOnly();
            }

            // Validate each index suggestion
            for (int i = 0; i < result.Count; i++)
            {
                var indexSuggestion = result[i];
                if (indexSuggestion == null)
                {
                    problems.Add($"Index suggestion at index {i} is null");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(indexSuggestion.IndexName))
                {
                    problems.Add($"Index suggestion at index {i} has null or empty IndexName");
                }

                if (indexSuggestion.EstimatedPerformanceGain <= 0)
                {
                    problems.Add($"Index suggestion at index {i} has non-positive EstimatedPerformanceGain: {indexSuggestion.EstimatedPerformanceGain}");
                }
            }

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Validates the results of <see cref="QueryRewriteExtensions.GetRewriteSummary"/>.
        /// </summary>
        /// <param name="suggestions">The suggestions to validate.</param>
        /// <returns>A list of human-readable validation problems; empty if valid.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="suggestions"/> is <see langword="null"/></exception>
        public static IReadOnlyList<string> ValidateRewriteSummary(this IEnumerable<QueryRewriteSuggestion> suggestions)
        {
            ArgumentNullException.ThrowIfNull(suggestions);

            var problems = new List<string>();

            var result = QueryRewriteExtensions.GetRewriteSummary(suggestions);
            if (result == null)
            {
                problems.Add("GetRewriteSummary() returned null");
            }
            else if (string.IsNullOrWhiteSpace(result))
            {
                problems.Add("GetRewriteSummary() returned null or empty string");
            }

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the specified suggestions are valid.
        /// </summary>
        /// <param name="suggestions">The suggestions to check.</param>
        /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="suggestions"/> is <see langword="null"/></exception>
        public static bool IsValid(this IEnumerable<QueryRewriteSuggestion> suggestions)
        {
            return Validate(suggestions).Count == 0;
        }

        /// <summary>
        /// Ensures that the specified suggestions are valid.
        /// </summary>
        /// <param name="suggestions">The suggestions to validate.</param>
        /// <exception cref="ArgumentNullException"><paramref name="suggestions"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentException">The suggestions are invalid; the exception message lists all problems.</exception>
        public static void EnsureValid(this IEnumerable<QueryRewriteSuggestion> suggestions)
        {
            ArgumentNullException.ThrowIfNull(suggestions);

            var problems = Validate(suggestions);
            if (problems.Count > 0)
            {
                throw new ArgumentException(
                    $"QueryRewriteSuggestions are invalid:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
            }
        }
    }
}