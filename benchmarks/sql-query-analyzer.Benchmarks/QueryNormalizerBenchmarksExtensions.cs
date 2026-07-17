using System;
using System.Collections.Generic;

namespace SqlQueryAnalyzer.Benchmarks
{
    /// <summary>
    /// Extension methods that simplify common benchmark scenarios for <see cref="QueryNormalizerBenchmarks"/>.
    /// Provides convenience wrappers around benchmark methods to reduce boilerplate code in benchmark scenarios.
    /// </summary>
    public static class QueryNormalizerBenchmarksExtensions
    {
        /// <summary>
        /// Executes the benchmark <c>Setup</c> step and then runs the simple normalisation routine.
        /// </summary>
        /// <remarks>
        /// This method combines the <see cref="QueryNormalizerBenchmarks.Setup"/> and <see cref="QueryNormalizerBenchmarks.NormalizeSimple"/> calls
        /// to provide a convenient entry point for benchmarking simple SQL query normalization.
        /// </remarks>
        /// <param name="benchmarks">The benchmark instance to operate on. Must not be <c>null</c>.</param>
        /// <returns>The normalised SQL string produced by <see cref="QueryNormalizerBenchmarks.NormalizeSimple"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="benchmarks"/> is <c>null</c>.</exception>
        public static string RunSetupAndNormalizeSimple(this QueryNormalizerBenchmarks benchmarks)
        {
            ArgumentNullException.ThrowIfNull(benchmarks);
            benchmarks.Setup();
            return benchmarks.NormalizeSimple();
        }

        /// <summary>
        /// Executes the benchmark <c>Setup</c> step and then runs the complex normalisation routine.
        /// </summary>
        /// <remarks>
        /// This method combines the <see cref="QueryNormalizerBenchmarks.Setup"/> and <see cref="QueryNormalizerBenchmarks.NormalizeComplex"/> calls
        /// to provide a convenient entry point for benchmarking complex SQL query normalization with multiple JOINs.
        /// </remarks>
        /// <param name="benchmarks">The benchmark instance to operate on. Must not be <c>null</c>.</param>
        /// <returns>The normalised SQL string produced by <see cref="QueryNormalizerBenchmarks.NormalizeComplex"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="benchmarks"/> is <c>null</c>.</exception>
        public static string RunSetupAndNormalizeComplex(this QueryNormalizerBenchmarks benchmarks)
        {
            ArgumentNullException.ThrowIfNull(benchmarks);
            benchmarks.Setup();
            return benchmarks.NormalizeComplex();
        }

        /// <summary>
        /// Executes the benchmark <c>Setup</c> step and then runs the normalisation routine that preserves literals.
        /// </summary>
        /// <remarks>
        /// This method combines the <see cref="QueryNormalizerBenchmarks.Setup"/> and <see cref="QueryNormalizerBenchmarks.NormalizeWithLiterals"/> calls
        /// to provide a convenient entry point for benchmarking SQL query normalization with embedded string literals.
        /// </remarks>
        /// <param name="benchmarks">The benchmark instance to operate on. Must not be <c>null</c>.</param>
        /// <returns>The normalised SQL string produced by <see cref="QueryNormalizerBenchmarks.NormalizeWithLiterals"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="benchmarks"/> is <c>null</c>.</exception>
        public static string RunSetupAndNormalizeWithLiterals(this QueryNormalizerBenchmarks benchmarks)
        {
            ArgumentNullException.ThrowIfNull(benchmarks);
            benchmarks.Setup();
            return benchmarks.NormalizeWithLiterals();
        }

        /// <summary>
        /// Retrieves the table names extracted from a complex query as a read‑only collection.
        /// </summary>
        /// <remarks>
        /// This method wraps <see cref="QueryNormalizerBenchmarks.ExtractTableNamesComplex"/> and converts the result
        /// to an <see cref="IReadOnlyList{T}"/> for safer consumption in scenarios requiring immutable collections.
        /// </remarks>
        /// <param name="benchmarks">The benchmark instance to operate on. Must not be <c>null</c>.</param>
        /// <returns>An <see cref="IReadOnlyList{T}"/> containing the extracted table names.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="benchmarks"/> is <c>null</c>.</exception>
        public static IReadOnlyList<string> GetTableNames(this QueryNormalizerBenchmarks benchmarks)
        {
            ArgumentNullException.ThrowIfNull(benchmarks);
            // The original method returns a List<string>; expose it as a read‑only view.
            return benchmarks.ExtractTableNamesComplex().AsReadOnly();
        }

        /// <summary>
        /// Retrieves the column names extracted from a complex query as a read‑only collection.
        /// </summary>
        /// <remarks>
        /// This method wraps <see cref="QueryNormalizerBenchmarks.ExtractColumnNamesComplex"/> and converts the result
        /// to an <see cref="IReadOnlyList{T}"/> for safer consumption in scenarios requiring immutable collections.
        /// </remarks>
        /// <param name="benchmarks">The benchmark instance to operate on. Must not be <c>null</c>.</param>
        /// <returns>An <see cref="IReadOnlyList{T}"/> containing the extracted column names.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="benchmarks"/> is <c>null</c>.</exception>
        public static IReadOnlyList<string> GetColumnNames(this QueryNormalizerBenchmarks benchmarks)
        {
            ArgumentNullException.ThrowIfNull(benchmarks);
            // The original method returns a List<string>; expose it as a read‑only view.
            return benchmarks.ExtractColumnNamesComplex().AsReadOnly();
        }
    }
}
