using System.Globalization;

namespace SqlQueryAnalyzer.Models;

/// <summary>
/// Provides validation helpers for <see cref="SlowQueryEntry"/> instances.
/// </summary>
public static class SlowQueryEntryValidation
{
    /// <summary>
    /// Validates a <see cref="SlowQueryEntry"/> instance and returns a list of validation problems.
    /// </summary>
    /// <param name="value">The entry to validate.</param>
    /// <returns>A read-only list of human-readable validation problems, or empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this SlowQueryEntry value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate EntryId
        if (string.IsNullOrWhiteSpace(value.EntryId))
        {
            problems.Add("EntryId must not be null or whitespace.");
        }
        else if (!IsValidGuidFormat(value.EntryId))
        {
            problems.Add("EntryId must be a valid GUID format.");
        }

        // Validate QueryText
        if (string.IsNullOrWhiteSpace(value.QueryText))
        {
            problems.Add("QueryText must not be null or whitespace.");
        }
        else if (value.QueryText.Length > 10_000)
        {
            problems.Add("QueryText exceeds maximum length of 10,000 characters.");
        }

        // Validate Duration
        if (value.Duration < TimeSpan.Zero)
        {
            problems.Add("Duration must not be negative.");
        }
        else if (value.Duration.TotalHours > 24)
        {
            problems.Add("Duration exceeds maximum allowed value of 24 hours.");
        }

        // Validate LockTime
        if (value.LockTime < TimeSpan.Zero)
        {
            problems.Add("LockTime must not be negative.");
        }
        else if (value.LockTime.TotalHours > 24)
        {
            problems.Add("LockTime exceeds maximum allowed value of 24 hours.");
        }

        // Validate RowsExamined
        if (value.RowsExamined < 0)
        {
            problems.Add("RowsExamined must not be negative.");
        }
        else if (value.RowsExamined > 1_000_000_000_000)
        {
            problems.Add("RowsExamined exceeds maximum allowed value of 1 trillion.");
        }

        // Validate RowsSent
        if (value.RowsSent < 0)
        {
            problems.Add("RowsSent must not be negative.");
        }
        else if (value.RowsSent > 1_000_000_000_000)
        {
            problems.Add("RowsSent exceeds maximum allowed value of 1 trillion.");
        }

        // Validate Timestamp
        if (value.Timestamp == default)
        {
            problems.Add("Timestamp must be set to a valid DateTime.");
        }
        else if (value.Timestamp > DateTime.UtcNow.AddHours(1))
        {
            problems.Add("Timestamp cannot be in the future.");
        }
        else if (value.Timestamp < DateTime.UtcNow.AddYears(-1))
        {
            problems.Add("Timestamp cannot be older than 1 year.");
        }

        // Validate UserHost
        if (string.IsNullOrWhiteSpace(value.UserHost))
        {
            problems.Add("UserHost must not be null or whitespace.");
        }
        else if (value.UserHost.Length > 500)
        {
            problems.Add("UserHost exceeds maximum length of 500 characters.");
        }

        // Validate Database
        if (string.IsNullOrWhiteSpace(value.Database))
        {
            problems.Add("Database must not be null or whitespace.");
        }
        else if (value.Database.Length > 100)
        {
            problems.Add("Database exceeds maximum length of 100 characters.");
        }

        // Validate LogSource
        if (string.IsNullOrWhiteSpace(value.LogSource))
        {
            problems.Add("LogSource must not be null or whitespace.");
        }
        else if (value.LogSource.Length > 100)
        {
            problems.Add("LogSource exceeds maximum length of 100 characters.");
        }

        // Validate Metadata
        if (value.Metadata is null)
        {
            problems.Add("Metadata dictionary must not be null.");
        }
        else if (value.Metadata.Count > 100)
        {
            problems.Add("Metadata dictionary exceeds maximum size of 100 entries.");
        }
        else
        {
            foreach (var kvp in value.Metadata)
            {
                if (string.IsNullOrWhiteSpace(kvp.Key))
                {
                    problems.Add("Metadata contains an entry with null or whitespace key.");
                    break;
                }

                if (kvp.Key.Length > 100)
                {
                    problems.Add("Metadata key exceeds maximum length of 100 characters.");
                    break;
                }

                if (kvp.Value is not null && kvp.Value.Length > 1000)
                {
                    problems.Add("Metadata value exceeds maximum length of 1000 characters.");
                    break;
                }
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="SlowQueryEntry"/> instance is valid.
    /// </summary>
    /// <param name="value">The entry to check.</param>
    /// <returns>True if the entry is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this SlowQueryEntry value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="SlowQueryEntry"/> instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The entry to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the entry is invalid, containing a list of problems.</exception>
    public static void EnsureValid(this SlowQueryEntry value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"SlowQueryEntry is invalid. Problems: {string.Join(" ", problems)}");
        }
    }

    /// <summary>
    /// Checks if a string represents a valid GUID format.
    /// </summary>
    /// <param name="input">The string to check.</param>
    /// <returns>True if the string is a valid GUID format; otherwise, false.</returns>
    private static bool IsValidGuidFormat(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        try
        {
            _ = Guid.Parse(input);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
        catch (OverflowException)
        {
            return false;
        }
    }
}