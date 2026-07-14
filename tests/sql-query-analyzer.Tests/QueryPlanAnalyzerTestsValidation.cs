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
        /// <param name="value">The instance to validate.</param>
        /// <returns>A list of human-readable validation problems; empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this QueryPlanAnalyzerTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            // No members to validate - all test methods are parameterless
            // The class is a test fixture class, not an instance with data

            return problems.AsReadOnly();
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
        /// <param name="value">The instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the instance is not valid, containing a list of validation problems.</exception>
        public static void EnsureValid(this QueryPlanAnalyzerTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = value.Validate();
            if (problems.Count > 0)
            {
                throw new ArgumentException(
                    $"QueryPlanAnalyzerTests instance is not valid. Problems:\n{string.Join("\n", problems)}");
            }
        }
    }
}
