#nullable enable

using System.Diagnostics.CodeAnalysis;

namespace SqlQueryAnalyzer.Export;

/// <summary>
/// Provides validation helpers for <see cref="ExportService"/> instances.
/// </summary>
public static class ExportServiceValidation
{
    /// <summary>
    /// Validates an <see cref="ExportService"/> instance.
    /// </summary>
    /// <param name="value">The export service instance to validate.</param>
    /// <returns>A list of validation errors; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate([NotNull] this ExportService? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate internal state
        if (value.GetSupportedFormats().Count == 0)
        {
            errors.Add("ExportService must have at least one registered formatter.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether an <see cref="ExportService"/> instance is valid.
    /// </summary>
    /// <param name="value">The export service instance to check.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid([NotNullWhen(true)] this ExportService? value)
    {
        return value is not null && value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that an <see cref="ExportService"/> instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The export service instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is not valid, containing validation errors.</exception>
    public static void EnsureValid([NotNull] this ExportService? value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value.Validate().Count > 0)
        {
            throw new ArgumentException(
                $"ExportService validation failed:{Environment.NewLine}• {string.Join($"{Environment.NewLine}• ", value.Validate())}");
        }
    }
}