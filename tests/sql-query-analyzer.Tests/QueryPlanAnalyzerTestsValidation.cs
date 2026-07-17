using System;
using System.Collections.Generic;

namespace SqlQueryAnalyzer.Tests
{
    /// <summary>
    /// Provides validation helpers for <see cref="QueryPlanAnalyzerTests"/> instances.
    /// </summary>
    public static class QueryPlanAnalyzerTestsValidation
    {
        /// <summary>
        /// Validates the specified <see cref="QueryPlanAnalyzerTests"/> instance.
        /// </summary>
        /// <remarks>
        /// This method always returns an empty list because <see cref="QueryPlanAnalyzerTests"/> is a test fixture class
        /// with parameterless test methods and no instance state to validate.
        /// </remarks>
        /// <param name="value">The instance to validate.</param>
        /// <returns>An empty list of validation problems (always valid).</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this QueryPlanAnalyzerTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            return Array.Empty<string>();
        }

        /// <summary>
        /// Determines whether the specified <see cref="QueryPlanAnalyzerTests"/> instance is valid.
        /// </summary>
        /// <param name="value">The instance to validate.</param>
        /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static bool IsValid(this QueryPlanAnalyzerTests value)
        {
            ArgumentNullException.ThrowIfNull(value);
            return value.Validate().Count == 0;
        }

        /// <summary>
        /// Ensures that the specified <see cref="QueryPlanAnalyzerTests"/> instance is valid.
        /// </summary>
        /// <remarks>
        /// This method always succeeds because <see cref="QueryPlanAnalyzerTests"/> is a test fixture class
        /// with parameterless test methods and no instance state to validate.
        /// </remarks>
        /// <param name="value">The instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static void EnsureValid(this QueryPlanAnalyzerTests value)
        {
            ArgumentNullException.ThrowIfNull(value);
        }
    }
}
