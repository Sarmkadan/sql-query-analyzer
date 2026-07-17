#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace SqlQueryAnalyzer.Tests
{
    /// <summary>
    /// Provides validation helpers for <see cref="QueryValidatorTests"/> instances.
    /// Validates that the test fixture contains properly structured test methods.
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

            var testMethods = value.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(m => m.GetCustomAttribute<FactAttribute>() != null)
                .ToList();

            if (testMethods.Count == 0)
            {
                problems.Add("QueryValidatorTests fixture contains no test methods with [Fact] attribute.");
            }

            foreach (var method in testMethods)
            {
                if (method.GetParameters().Length > 0)
                {
                    problems.Add($"Test method '{method.Name}' has parameters. Test methods should be parameterless.");
                }

                if (method.ReturnType != typeof(void))
                {
                    problems.Add($"Test method '{method.Name}' returns {method.ReturnType.Name}. Test methods should return void.");
                }

                if (!method.IsPublic)
                {
                    problems.Add($"Test method '{method.Name}' is not public. Test methods should be public.");
                }

                if (method.Name.StartsWith("ctor", StringComparison.Ordinal) ||
                    method.Name.StartsWith("Finalize", StringComparison.Ordinal))
                {
                    problems.Add($"Test method '{method.Name}' appears to be a special method. Test methods should follow naming conventions.");
                }
            }

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
