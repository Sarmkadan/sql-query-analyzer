using System;
using System.Collections.Generic;
using System.Globalization;

namespace SqlQueryAnalyzer.Benchmarks
{
    /// <summary>
    /// Extension methods for <see cref="SqlPatternAnalyzerBenchmarks"/> to analyze SQL query patterns.
    /// </summary>
    public static class SqlPatternAnalyzerBenchmarksExtensions
    {
        /// <summary>
        /// Determines if the query contains excessive OR conditions that may impact performance.
        /// </summary>
        /// <param name="benchmarks">The benchmark instance to analyze.</param>
        /// <param name="threshold">The threshold for excessive OR conditions (default: 5).</param>
        /// <returns>True if OR conditions exceed the threshold.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="benchmarks"/> is null.</exception>
        public static bool HasExcessiveOrConditions(this SqlPatternAnalyzerBenchmarks benchmarks, int threshold = 5)
        {
            ArgumentNullException.ThrowIfNull(benchmarks);
            return benchmarks.CountOrConditions() > threshold;
        }

        /// <summary>
        /// Determines if the query has low readability based on its score.
        /// </summary>
        /// <param name="benchmarks">The benchmark instance to analyze.</param>
        /// <param name="threshold">The readability score threshold (default: 0.5).</param>
        /// <returns>True if readability score is below the threshold.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="benchmarks"/> is null.</exception>
        public static bool IsReadabilityScoreLow(this SqlPatternAnalyzerBenchmarks benchmarks, double threshold = 0.5)
        {
            ArgumentNullException.ThrowIfNull(benchmarks);
            return benchmarks.ReadabilityScoreProblematic() < threshold;
        }

        /// <summary>
        /// Determines if the query contains nested parentheses that may complicate execution.
        /// </summary>
        /// <param name="benchmarks">The benchmark instance to analyze.</param>
        /// <param name="threshold">The threshold for nested parentheses (default: 3).</param>
        /// <returns>True if nested parentheses exceed the threshold.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="benchmarks"/> is null.</exception>
        public static bool HasNestedParentheses(this SqlPatternAnalyzerBenchmarks benchmarks, int threshold = 3)
        {
            ArgumentNullException.ThrowIfNull(benchmarks);
            return benchmarks.CountParenthesesNested() > threshold;
        }

        /// <summary>
        /// Determines if the query applies functions directly to columns, which may prevent index usage.
        /// </summary>
        /// <param name="benchmarks">The benchmark instance to analyze.</param>
        /// <returns>True if the query contains functions applied to columns.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="benchmarks"/> is null.</exception>
        public static bool HasProblematicFunctionOnColumn(this SqlPatternAnalyzerBenchmarks benchmarks)
        {
            ArgumentNullException.ThrowIfNull(benchmarks);
            return benchmarks.HasFunctionOnColumn();
        }
    }
}
