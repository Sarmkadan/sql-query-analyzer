#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

namespace SqlQueryAnalyzer.Tests
{
    /// <summary>
    /// Provides validation helpers for <see cref="QueryValidatorTests"/> instances.
    /// </summary>
    public static class QueryValidatorTestsValidation
    {
        /// <summary>
        /// Validates the specified <see cref="QueryValidatorTests"/> instance.
        /// </summary>
        /// <param name="value">The instance to validate.</param>
        /// <returns>A list of human-readable validation problems; empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this QueryValidatorTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            // QueryValidatorTests is a test fixture class with only parameterless test methods
            // There are no members to validate beyond the instance itself
            // All validation is performed by invoking the test methods

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the specified <see cref="QueryValidatorTests"/> instance is valid.
        /// </summary>
        /// <param name="value">The instance to check.</param>
        /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static bool IsValid(this QueryValidatorTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            return value.Validate().Count == 0;
        }

        /// <summary>
        /// Ensures that the specified <see cref="QueryValidatorTests"/> instance is valid.
        /// </summary>
        /// <param name="value">The instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the instance is not valid, containing a list of validation problems.</exception>
        public static void EnsureValid(this QueryValidatorTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = value.Validate();
            if (problems.Count > 0)
            {
                throw new ArgumentException(
                    $"QueryValidatorTests instance is not valid. Problems:\n{string.Join("\n", problems)}");
            }
        }
    }
}
