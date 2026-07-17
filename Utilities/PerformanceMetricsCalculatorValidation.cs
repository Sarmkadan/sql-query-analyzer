using System;
using System.Collections.Generic;

using SqlQueryAnalyzer.Models;
using ModelIndex = SqlQueryAnalyzer.Models.Index;

namespace SqlQueryAnalyzer.Utilities
{
    /// <summary>
    /// Provides validation helpers for <see cref="PerformanceMetricsCalculator"/> method parameters.
    /// </summary>
    public sealed class PerformanceMetricsCalculatorValidation
    {
        /// <summary>
        /// Validates parameters for <see cref="PerformanceMetricsCalculator.CalculateCombinedScore(QueryAnalysisResult, double)"/>.
        /// </summary>
        /// <param name="analysis">The query analysis result to validate.</param>
        /// <param name="weight">The weight parameter to validate.</param>
        /// <returns>A list of human-readable validation problems; empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="analysis"/> is null.</exception>
        public static IReadOnlyList<string> Validate(
            QueryAnalysisResult analysis,
            double weight = 1.0)
        {
            ArgumentNullException.ThrowIfNull(analysis);

            var errors = new List<string>();

            if (weight <= 0)
            {
                errors.Add("Weight must be positive.");
            }

            if (weight > 100)
            {
                errors.Add("Weight should not exceed 100.");
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Validates parameters for <see cref="PerformanceMetricsCalculator.EstimateTotalOptimization(List{PerformanceIssue}, List{IndexSuggestion})"/>.
        /// </summary>
        /// <param name="issues">The list of performance issues to validate.</param>
        /// <param name="suggestions">The list of index suggestions to validate.</param>
        /// <returns>A list of human-readable validation problems; empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="issues"/> or <paramref name="suggestions"/> is null.</exception>
        public static IReadOnlyList<string> Validate(
            List<PerformanceIssue>? issues,
            List<IndexSuggestion>? suggestions)
        {
            ArgumentNullException.ThrowIfNull(issues);
            ArgumentNullException.ThrowIfNull(suggestions);

            var errors = new List<string>();

            if (issues.Count == 0)
            {
                errors.Add("Issues list must not be empty.");
            }

            if (suggestions.Count == 0)
            {
                errors.Add("Suggestions list must not be empty.");
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Validates parameters for <see cref="PerformanceMetricsCalculator.CalculateComplexityScore(DatabaseQuery)"/>.
        /// </summary>
        /// <param name="query">The database query to validate.</param>
        /// <returns>A list of human-readable validation problems; empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="query"/> is null.</exception>
        public static IReadOnlyList<string> Validate(DatabaseQuery query)
        {
            ArgumentNullException.ThrowIfNull(query);

            return Array.Empty<string>();
        }

        /// <summary>
        /// Validates parameters for <see cref="PerformanceMetricsCalculator.CalculateIndexUsageScore(ModelIndex)"/>.
        /// </summary>
        /// <param name="index">The index to validate.</param>
        /// <returns>A list of human-readable validation problems; empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="index"/> is null.</exception>
        public static IReadOnlyList<string> Validate(ModelIndex index)
        {
            ArgumentNullException.ThrowIfNull(index);

            var errors = new List<string>();

            if (!index.IsValid())
            {
                errors.Add("Index must be valid (IsValid() returned false).");
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Validates parameters for <see cref="PerformanceMetricsCalculator.CalculateMaintenanceEffort(List{ModelIndex})"/>.
        /// </summary>
        /// <param name="indexes">The list of indexes to validate.</param>
        /// <returns>A list of human-readable validation problems; empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="indexes"/> is null.</exception>
        public static IReadOnlyList<string> Validate(List<ModelIndex>? indexes)
        {
            ArgumentNullException.ThrowIfNull(indexes);

            var errors = new List<string>();

            if (indexes.Count == 0)
            {
                errors.Add("Indexes list must not be empty.");
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Validates parameters for <see cref="PerformanceMetricsCalculator.GetPerformanceTrend(List{QueryAnalysisResult})"/>.
        /// </summary>
        /// <param name="analysisHistory">The analysis history to validate.</param>
        /// <returns>A list of human-readable validation problems; empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="analysisHistory"/> is null.</exception>
        public static IReadOnlyList<string> Validate(List<QueryAnalysisResult> analysisHistory)
        {
            ArgumentNullException.ThrowIfNull(analysisHistory);

            var errors = new List<string>();

            if (analysisHistory.Count < 2)
            {
                errors.Add("Analysis history must contain at least 2 items to calculate trend.");
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Validates parameters for <see cref="PerformanceMetricsCalculator.CalculateExecutionTimeDistribution(QueryStatistics)"/>.
        /// </summary>
        /// <param name="stats">The query statistics to validate.</param>
        /// <returns>A list of human-readable validation problems; empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="stats"/> is null.</exception>
        public static IReadOnlyList<string> Validate(QueryStatistics stats)
        {
            ArgumentNullException.ThrowIfNull(stats);

            return Array.Empty<string>();
        }

        /// <summary>
        /// Validates parameters for <see cref="PerformanceMetricsCalculator.CalculateIndexROI(IndexSuggestion, long)"/>.
        /// </summary>
        /// <param name="suggestion">The index suggestion to validate.</param>
        /// <param name="tableSizeKB">The table size in KB to validate.</param>
        /// <returns>A list of human-readable validation problems; empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="suggestion"/> is null.</exception>
        public static IReadOnlyList<string> Validate(
            IndexSuggestion suggestion,
            long tableSizeKB)
        {
            ArgumentNullException.ThrowIfNull(suggestion);

            var errors = new List<string>();

            if (tableSizeKB <= 0)
            {
                errors.Add("Table size must be positive.");
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Validates parameters for <see cref="PerformanceMetricsCalculator.PredictExecutionTime(QueryStatistics, int)"/>.
        /// </summary>
        /// <param name="stats">The query statistics to validate.</param>
        /// <param name="estimatedRows">The estimated row count to validate.</param>
        /// <returns>A list of human-readable validation problems; empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="stats"/> is null.</exception>
        public static IReadOnlyList<string> Validate(
            QueryStatistics stats,
            int estimatedRows)
        {
            ArgumentNullException.ThrowIfNull(stats);

            var errors = new List<string>();

            if (estimatedRows <= 0)
            {
                errors.Add("Estimated rows must be positive.");
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the parameters for <see cref="PerformanceMetricsCalculator.CalculateCombinedScore(QueryAnalysisResult, double)"/> are valid.
        /// </summary>
        /// <param name="analysis">The query analysis result to check.</param>
        /// <param name="weight">The weight parameter to check.</param>
        /// <returns>True if the parameters are valid; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="analysis"/> is null.</exception>
        public static bool IsValid(
            QueryAnalysisResult analysis,
            double weight = 1.0) => Validate(analysis, weight).Count == 0;

        /// <summary>
        /// Determines whether the parameters for <see cref="PerformanceMetricsCalculator.EstimateTotalOptimization(List{PerformanceIssue}, List{IndexSuggestion})"/> are valid.
        /// </summary>
        /// <param name="issues">The list of performance issues to check.</param>
        /// <param name="suggestions">The list of index suggestions to check.</param>
        /// <returns>True if the parameters are valid; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="issues"/> or <paramref name="suggestions"/> is null.</exception>
        public static bool IsValid(
            List<PerformanceIssue>? issues,
            List<IndexSuggestion>? suggestions) => Validate(issues, suggestions).Count == 0;

        /// <summary>
        /// Determines whether the parameters for <see cref="PerformanceMetricsCalculator.CalculateComplexityScore(DatabaseQuery)"/> are valid.
        /// </summary>
        /// <param name="query">The database query to check.</param>
        /// <returns>True if the parameters are valid; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="query"/> is null.</exception>
        public static bool IsValid(DatabaseQuery query) => Validate(query).Count == 0;

        /// <summary>
        /// Determines whether the parameters for <see cref="PerformanceMetricsCalculator.CalculateIndexUsageScore(ModelIndex)"/> are valid.
        /// </summary>
        /// <param name="index">The index to check.</param>
        /// <returns>True if the parameters are valid; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="index"/> is null.</exception>
        public static bool IsValid(ModelIndex index) => Validate(index).Count == 0;

        /// <summary>
        /// Determines whether the parameters for <see cref="PerformanceMetricsCalculator.CalculateMaintenanceEffort(List{ModelIndex})"/> are valid.
        /// </summary>
        /// <param name="indexes">The list of indexes to check.</param>
        /// <returns>True if the parameters are valid; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="indexes"/> is null.</exception>
        public static bool IsValid(List<ModelIndex>? indexes) => Validate(indexes).Count == 0;

        /// <summary>
        /// Determines whether the parameters for <see cref="PerformanceMetricsCalculator.GetPerformanceTrend(List{QueryAnalysisResult})"/> are valid.
        /// </summary>
        /// <param name="analysisHistory">The analysis history to check.</param>
        /// <returns>True if the parameters are valid; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="analysisHistory"/> is null.</exception>
        public static bool IsValid(List<QueryAnalysisResult> analysisHistory) => Validate(analysisHistory).Count == 0;

        /// <summary>
        /// Determines whether the parameters for <see cref="PerformanceMetricsCalculator.CalculateExecutionTimeDistribution(QueryStatistics)"/> are valid.
        /// </summary>
        /// <param name="stats">The query statistics to check.</param>
        /// <returns>True if the parameters are valid; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="stats"/> is null.</exception>
        public static bool IsValid(QueryStatistics stats) => Validate(stats).Count == 0;

        /// <summary>
        /// Determines whether the parameters for <see cref="PerformanceMetricsCalculator.CalculateIndexROI(IndexSuggestion, long)"/> are valid.
        /// </summary>
        /// <param name="suggestion">The index suggestion to check.</param>
        /// <param name="tableSizeKB">The table size in KB to check.</param>
        /// <returns>True if the parameters are valid; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="suggestion"/> is null.</exception>
        public static bool IsValid(
            IndexSuggestion suggestion,
            long tableSizeKB) => Validate(suggestion, tableSizeKB).Count == 0;

        /// <summary>
        /// Determines whether the parameters for <see cref="PerformanceMetricsCalculator.PredictExecutionTime(QueryStatistics, int)"/> are valid.
        /// </summary>
        /// <param name="stats">The query statistics to check.</param>
        /// <param name="estimatedRows">The estimated row count to check.</param>
        /// <returns>True if the parameters are valid; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="stats"/> is null.</exception>
        public static bool IsValid(
            QueryStatistics stats,
            int estimatedRows) => Validate(stats, estimatedRows).Count == 0;

        /// <summary>
        /// Ensures that the parameters for <see cref="PerformanceMetricsCalculator.CalculateCombinedScore(QueryAnalysisResult, double)"/> are valid, throwing an <see cref="ArgumentException"/> if not.
        /// </summary>
        /// <param name="analysis">The query analysis result to validate.</param>
        /// <param name="weight">The weight parameter to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="analysis"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the parameters are not valid, containing a list of validation problems.</exception>
        public static void EnsureValid(
            QueryAnalysisResult analysis,
            double weight = 1.0)
        {
            var errors = Validate(analysis, weight);
            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    $"Parameters for CalculateCombinedScore are not valid. Problems:\n{string.Join("\n", errors)}");
            }
        }

        /// <summary>
        /// Ensures that the parameters for <see cref="PerformanceMetricsCalculator.EstimateTotalOptimization(List{PerformanceIssue}, List{IndexSuggestion})"/> are valid, throwing an <see cref="ArgumentException"/> if not.
        /// </summary>
        /// <param name="issues">The list of performance issues to validate.</param>
        /// <param name="suggestions">The list of index suggestions to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="issues"/> or <paramref name="suggestions"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the parameters are not valid, containing a list of validation problems.</exception>
        public static void EnsureValid(
            List<PerformanceIssue>? issues,
            List<IndexSuggestion>? suggestions)
        {
            var errors = Validate(issues, suggestions);
            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    $"Parameters for EstimateTotalOptimization are not valid. Problems:\n{string.Join("\n", errors)}");
            }
        }

        /// <summary>
        /// Ensures that the parameters for <see cref="PerformanceMetricsCalculator.CalculateComplexityScore(DatabaseQuery)"/> are valid, throwing an <see cref="ArgumentException"/> if not.
        /// </summary>
        /// <param name="query">The database query to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="query"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the parameters are not valid, containing a list of validation problems.</exception>
        public static void EnsureValid(DatabaseQuery query)
        {
            var errors = Validate(query);
            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    $"Parameters for CalculateComplexityScore are not valid. Problems:\n{string.Join("\n", errors)}");
            }
        }

        /// <summary>
        /// Ensures that the parameters for <see cref="PerformanceMetricsCalculator.CalculateIndexUsageScore(ModelIndex)"/> are valid, throwing an <see cref="ArgumentException"/> if not.
        /// </summary>
        /// <param name="index">The index to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="index"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the parameters are not valid, containing a list of validation problems.</exception>
        public static void EnsureValid(ModelIndex index)
        {
            var errors = Validate(index);
            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    $"Parameters for CalculateIndexUsageScore are not valid. Problems:\n{string.Join("\n", errors)}");
            }
        }

        /// <summary>
        /// Ensures that the parameters for <see cref="PerformanceMetricsCalculator.CalculateMaintenanceEffort(List{ModelIndex})"/> are valid, throwing an <see cref="ArgumentException"/> if not.
        /// </summary>
        /// <param name="indexes">The list of indexes to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="indexes"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the parameters are not valid, containing a list of validation problems.</exception>
        public static void EnsureValid(List<ModelIndex>? indexes)
        {
            var errors = Validate(indexes);
            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    $"Parameters for CalculateMaintenanceEffort are not valid. Problems:\n{string.Join("\n", errors)}");
            }
        }

        /// <summary>
        /// Ensures that the parameters for <see cref="PerformanceMetricsCalculator.GetPerformanceTrend(List{QueryAnalysisResult})"/> are valid, throwing an <see cref="ArgumentException"/> if not.
        /// </summary>
        /// <param name="analysisHistory">The analysis history to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="analysisHistory"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the parameters are not valid, containing a list of validation problems.</exception>
        public static void EnsureValid(List<QueryAnalysisResult> analysisHistory)
        {
            var errors = Validate(analysisHistory);
            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    $"Parameters for GetPerformanceTrend are not valid. Problems:\n{string.Join("\n", errors)}");
            }
        }

        /// <summary>
        /// Ensures that the parameters for <see cref="PerformanceMetricsCalculator.CalculateExecutionTimeDistribution(QueryStatistics)"/> are valid, throwing an <see cref="ArgumentException"/> if not.
        /// </summary>
        /// <param name="stats">The query statistics to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="stats"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the parameters are not valid, containing a list of validation problems.</exception>
        public static void EnsureValid(QueryStatistics stats)
        {
            var errors = Validate(stats);
            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    $"Parameters for CalculateExecutionTimeDistribution are not valid. Problems:\n{string.Join("\n", errors)}");
            }
        }

        /// <summary>
        /// Ensures that the parameters for <see cref="PerformanceMetricsCalculator.CalculateIndexROI(IndexSuggestion, long)"/> are valid, throwing an <see cref="ArgumentException"/> if not.
        /// </summary>
        /// <param name="suggestion">The index suggestion to validate.</param>
        /// <param name="tableSizeKB">The table size in KB to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="suggestion"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the parameters are not valid, containing a list of validation problems.</exception>
        public static void EnsureValid(
            IndexSuggestion suggestion,
            long tableSizeKB)
        {
            var errors = Validate(suggestion, tableSizeKB);
            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    $"Parameters for CalculateIndexROI are not valid. Problems:\n{string.Join("\n", errors)}");
            }
        }

        /// <summary>
        /// Ensures that the parameters for <see cref="PerformanceMetricsCalculator.PredictExecutionTime(QueryStatistics, int)"/> are valid, throwing an <see cref="ArgumentException"/> if not.
        /// </summary>
        /// <param name="stats">The query statistics to validate.</param>
        /// <param name="estimatedRows">The estimated row count to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="stats"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the parameters are not valid, containing a list of validation problems.</exception>
        public static void EnsureValid(
            QueryStatistics stats,
            int estimatedRows)
        {
            var errors = Validate(stats, estimatedRows);
            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    $"Parameters for PredictExecutionTime are not valid. Problems:\n{string.Join("\n", errors)}");
            }
        }
    }
}