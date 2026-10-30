# AnalysisDiagnostic

**Namespace:** `SqlQueryAnalyzer.Models`
**Assembly:** `SqlQueryAnalyzer`
**Source:** `Models/AnalysisDiagnostic.cs`
**Base type:** `object`

`AnalysisDiagnostic` records a non-fatal failure from one stage of an analysis. A stage is a detector rule, a pipeline middleware, or a similar unit of work. When a stage throws an exception or runs past its time budget, the analyzer does not abort the whole analysis. It records an `AnalysisDiagnostic`, skips that stage's output, and continues with the remaining stages. The diagnostic keeps a record of what was skipped and why.

Diagnostics are collected in `QueryAnalysisResult.Diagnostics`. The pipeline's `RecordDiagnostic` also sets `QueryAnalysisResult.IsPartial`. Not every diagnostic source sets that flag, so check `Diagnostics` as well as `IsPartial`.

## Declaration

```csharp
public sealed class AnalysisDiagnostic
```

The class is `sealed`, so it cannot be used as a base class. It has no explicit constructors. Callers create it with an object initializer.

## Properties

| Property | Type | Accessors | Default | Description |
|---|---|---|---|---|
| `RuleId` | `string` | get, set | `string.Empty` | Identifier of the rule, detector, or middleware stage that failed. For detectors this is the plugin's `RuleId`. |
| `Message` | `string` | get, set | `string.Empty` | Human-readable description of the failure. Usually the exception message, or a timeout message that names the rule and the budget. |
| `TimedOut` | `bool` | get, set | `false` | `true` when the stage exceeded its time budget. `false` when the stage threw an exception. |
| `OccurredAt` | `DateTime` | get, set | `DateTime.UtcNow` at creation | UTC time at which the failure was recorded. The value is taken when the object is constructed, which is right after the failure is caught. |

All four properties are plain auto-properties, so they can be changed after the object is created.

## Purpose

Diagnostics let the analyzer report partial results instead of failing the whole request. Two outcomes are possible for a failing stage:

- **Exception:** the stage's findings are skipped and a diagnostic is recorded with `TimedOut = false` and `Message` set to the exception text.
- **Timeout:** the stage's findings are skipped and a diagnostic is recorded with `TimedOut = true`. Timeouts are reported separately so a caller can tell a slow rule from a broken one.

Caller cancellation is not recorded as a diagnostic. If the caller's own cancellation token fires, the operation is cancelled and the exception propagates.

## Where it is created

| Location | Source of the diagnostic | `TimedOut` |
|---|---|---|
| `Middleware/AnalysisPipeline.cs` (`RecordDiagnostic`) | A pipeline middleware that threw or timed out. Also sets `QueryAnalysisResult.IsPartial = true`. | Set from the caller's argument. |
| `Services/PerformanceIssueDetectorService.cs` (`RunRulesAsync`) | A detector plugin that exceeded `DetectorTimeout`. | `true` |
| `Services/PerformanceIssueDetectorService.cs` (`RunRulesAsync`) | A detector plugin that threw an exception. | `false` |

The detector service returns its diagnostics through `DetectIssuesWithDiagnosticsAsync`, which is declared on `IQueryAnalyzerService` with the signature `Task<(List<PerformanceIssue> Issues, List<Models.AnalysisDiagnostic> Diagnostics)>`.

## Example

```csharp
var diagnostic = new AnalysisDiagnostic
{
    RuleId = plugin.RuleId,
    Message = ex.Message,
    TimedOut = false
};
```

Timeouts use the same shape with `TimedOut = true` and a message that includes the budget:

```csharp
var diagnostic = new AnalysisDiagnostic
{
    RuleId = plugin.RuleId,
    Message = $"Detector '{plugin.RuleId}' timed out after {DetectorTimeout.TotalMilliseconds:F0}ms",
    TimedOut = true
};
```

## Reading diagnostics

```csharp
foreach (var diagnostic in result.Diagnostics)
{
    Console.WriteLine($"{diagnostic.RuleId} {(diagnostic.TimedOut ? "timed out" : "failed")}: {diagnostic.Message}");
}
```

`result` is a `QueryAnalysisResult`. A non-empty `Diagnostics` list means some stage's output is missing from the result.

## Notes

- `OccurredAt` is in UTC. Compare it with other UTC values, not with local time.
- `Message` can contain exception text. Treat it as untrusted when it is shown outside the service.
- The type is a plain data class with no behavior. It is a `class`, not a `record`, and does not override equality, so two diagnostics with the same values are different objects.

## Related

- [`QueryAnalysisResult`](QueryAnalysisResult.md) - holds `Diagnostics` and the `IsPartial` flag.
- [`PerformanceIssueDetectorService`](PerformanceIssueDetectorService.md) - produces diagnostics for detector plugins.
- [`AnalysisPipeline`](AnalysisPipeline.md) - records diagnostics for middleware stages.
- [`IQueryAnalyzerService`](IQueryAnalyzerService.md) - declares `DetectIssuesWithDiagnosticsAsync`.
