#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SqlQueryAnalyzer.Models;

/// <summary>
/// Represents a non-fatal failure recorded while a pipeline stage or detector
/// could not complete (threw an exception, or exceeded its execution budget).
/// Allows the overall analysis to continue with the remaining stages while
/// keeping a record of what was skipped and why.
/// </summary>
public sealed class AnalysisDiagnostic
{
    /// <summary>
    /// Gets or sets the identifier of the rule, detector, or middleware stage that failed.
    /// </summary>
    public string RuleId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a human-readable description of the failure (typically the exception message).
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the failure was caused by the stage exceeding
    /// its configured timeout, as opposed to throwing an exception directly.
    /// </summary>
    public bool TimedOut { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp at which the failure was recorded.
    /// </summary>
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Returns a concise, single-line summary of the diagnostic: rule identifier, message,
    /// timeout flag (when set), and the UTC timestamp of the failure.
    /// </summary>
    /// <returns>A human-readable summary of this diagnostic.</returns>
    public override string ToString()
    {
        var timeoutMarker = TimedOut ? " [timed out]" : string.Empty;
        return $"{RuleId}: {Message}{timeoutMarker} at {OccurredAt:yyyy-MM-dd HH:mm:ss} UTC";
    }
}
