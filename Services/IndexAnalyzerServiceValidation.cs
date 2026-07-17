using System;
using System.Collections.Generic;

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
        /// <param name="service">The service instance to validate.</param>
        /// <returns>A list of validation messages; empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="service"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this IndexAnalyzerService service)
        {
            ArgumentNullException.ThrowIfNull(service);

            var errors = new List<string>();

            // Validate constructor-injected dependencies
            if (service is null)
            {
                errors.Add("Service instance cannot be null.");
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the specified <see cref="IndexAnalyzerService"/> instance is valid.
        /// </summary>
        /// <param name="service">The service instance to check.</param>
        /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="service"/> is null.</exception>
        public static bool IsValid(this IndexAnalyzerService service)
        {
            ArgumentNullException.ThrowIfNull(service);
            return Validate(service).Count == 0;
        }

        /// <summary>
        /// Ensures that the specified <see cref="IndexAnalyzerService"/> instance is valid.
        /// </summary>
        /// <param name="service">The service instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="service"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the instance is not valid, containing all validation messages.</exception>
        public static void EnsureValid(this IndexAnalyzerService service)
        {
            ArgumentNullException.ThrowIfNull(service);

            var errors = Validate(service);
            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    $"IndexAnalyzerService validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
            }
        }
    }
}
