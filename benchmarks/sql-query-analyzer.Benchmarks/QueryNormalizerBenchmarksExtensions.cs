using System;
using System.Collections.Generic;

namespace SqlQueryAnalyzer.Benchmarks
{
    /// <summary>
    /// Extension methods that simplify common benchmark scenarios for <see cref="QueryNormalizerBenchmarks"/>.
    /// </summary>
    public static class QueryNormalizerBenchmarksExtensions
    {
        /// <summary>
        /// Executes the benchmark <c>Setup</c> step and then runs the simple normalisation routine.
        /// </summary>
        /// <param name="benchmarks">The benchmark instance to operate on.</param>
        /// <returns>The normalised SQL string produced by <c>NormalizeSimple</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="benchmarks"/> is <c>null</c>.</exception>
        public static string RunSetupAndNormalizeSimple(this QueryNormalizerBenchmarks benchmarks)
        {
            ArgumentNullException.ThrowIfNull(benchmarks);
            benchmarks.Setup();
            return benchmarks.NormalizeSimple();
        }

        /// <summary>
        /// Executes the benchmark <c>Setup</c> step and then runs the complex normalisation routine.
        /// </summary>
        /// <param name="benchmarks">The benchmark instance to operate on.</param>
        /// <returns>The normalised SQL string produced by <c>NormalizeComplex</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="benchmarks"/> is <c>null</c>.</exception>
        public static string RunSetupAndNormalizeComplex(this QueryNormalizerBenchmarks benchmarks)
        {
            ArgumentNullException.ThrowIfNull(benchmarks);
            benchmarks.Setup();
            return benchmarks.NormalizeComplex();
        }

        /// <summary>
        /// Executes the benchmark <c>Setup</c> step and then runs the normalisation routine that preserves literals.
        /// </summary>
        /// <param name="benchmarks">The benchmark instance to operate on.</param>
        /// <returns>The normalised SQL string produced by <c>NormalizeWithLiterals</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="benchmarks"/> is <c>null</c>.</exception>
        public static string RunSetupAndNormalizeWithLiterals(this QueryNormalizerBenchmarks benchmarks)
        {
            ArgumentNullException.ThrowIfNull(benchmarks);
            benchmarks.Setup();
            return benchmarks.NormalizeWithLiterals();
        }

        /// <summary>
        /// Retrieves the table names extracted from a complex query as a read‑only collection.
        /// </summary>
        /// <param name="benchmarks">The benchmark instance to operate on.</param>
        /// <returns>An <see cref="IReadOnlyList{T}"/> containing the extracted table names.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="benchmarks"/> is <c>null</c>.</exception>
        public static IReadOnlyList<string> GetTableNames(this QueryNormalizerBenchmarks benchmarks)
        {
            ArgumentNullException.ThrowIfNull(benchmarks);
            // The original method returns a List<string>; expose it as a read‑only view.
            return benchmarks.ExtractTableNamesComplex().AsReadOnly();
        }

        /// <summary>
        /// Retrieves the column names extracted from a complex query as a read‑only collection.
        /// </summary>
        /// <param name="benchmarks">The benchmark instance to operate on.</param>
        /// <returns>An <see cref="IReadOnlyList{T}"/> containing the extracted column names.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="benchmarks"/> is <c>null</c>.</exception>
        public static IReadOnlyList<string> GetColumnNames(this QueryNormalizerBenchmarks benchmarks)
        {
            ArgumentNullException.ThrowIfNull(benchmarks);
            // The original method returns a List<string>; expose it as a read‑only view.
            return benchmarks.ExtractColumnNamesComplex().AsReadOnly();
        }
    }
}
