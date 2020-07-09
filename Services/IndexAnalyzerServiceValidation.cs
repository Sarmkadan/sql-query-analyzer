using System;
using System.Collections.Generic;
using System.Globalization;

namespace SqlQueryAnalyzer.Services
{
    /// <summary>
    /// Provides validation helpers for <see cref="IndexAnalyzerService"/> instances.
    /// </summary>
    public static class IndexAnalyzerServiceValidation
    {
        /// <summary>
        /// Validates the specified <see cref="IndexAnalyzerService"/> instance.
        /// </summary>
        /// <param name="value">The service instance to validate.</param>
        /// <returns>A list of validation messages; empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this IndexAnalyzerService value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = new List<string>();

            // Validate private fields via reflection
            var loggerField = typeof(IndexAnalyzerService).GetField(
                "_logger",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (loggerField?.GetValue(value) is null)
            {
                errors.Add("Logger dependency cannot be null.");
            }

            var repositoryField = typeof(IndexAnalyzerService).GetField(
                "_repository",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (repositoryField?.GetValue(value) is null)
            {
                errors.Add("Repository dependency cannot be null.");
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the specified <see cref="IndexAnalyzerService"/> instance is valid.
        /// </summary>
        /// <param name="value">The service instance to check.</param>
        /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static bool IsValid(this IndexAnalyzerService value)
        {
            ArgumentNullException.ThrowIfNull(value);
            return Validate(value).Count == 0;
        }

        /// <summary>
        /// Ensures that the specified <see cref="IndexAnalyzerService"/> instance is valid.
        /// </summary>
        /// <param name="value">The service instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the instance is not valid, containing all validation messages.</exception>
        public static void EnsureValid(this IndexAnalyzerService value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = Validate(value);
            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    $"IndexAnalyzerService validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
            }
        }
    }
}
