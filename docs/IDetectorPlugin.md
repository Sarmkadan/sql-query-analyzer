# IDetectorPlugin

`IDetectorPlugin` is the single contract for performance rules in the `sql-query-analyzer` project. It is declared in `Services/IDetectorPlugin.cs` under the `SqlQueryAnalyzer.Services` namespace. Each implementation checks one rule against a parsed `DatabaseQuery` and returns zero or more `PerformanceIssue` findings.

The interface replaces the older two-tier model, in which built-in checks were private methods on `PerformanceIssueDetectorService` and external rules used a separate plugin abstraction. Built-in and third-party rules now implement the same interface. `PerformanceIssueDetectorService` runs whatever collection of `IDetectorPlugin` instances it is given, so a rule can be added, removed, disabled, or reordered by changing that collection. The orchestrator does not need to change.

Implementations are expected to be stateless, individually testable, and safe to call concurrently for different queries.

## API

The interface has two members.

### `RuleId`
`string RuleId { get; }`

The stable, unique identifier of the rule (for example `"select-star"` or `"cartesian-join"`). It is used to:

- Toggle rules on or off.
- Correlate findings with the rule that produced them.
- Generate the rule catalog documentation.

The value must not change between releases, because other tooling may depend on it. It must also be unique across all registered plugins.

### `Analyze`
`IEnumerable<PerformanceIssue> Analyze(DatabaseQuery query)`

Analyzes the supplied query and returns the issues the rule detects.

- **Parameters**:
  - `query` (`DatabaseQuery`): the parsed query to analyze.
- **Return Value**: A sequence of `PerformanceIssue` objects. The sequence is empty when the rule does not apply.
- **Behavior**: Implementations must not have side effects and must be safe to call concurrently for different queries. Returning a lazily evaluated sequence (`yield return`) is allowed, and the built-in rules use it. Because the sequence is lazy, exceptions raised inside the iterator, such as a null-argument check, surface on first enumeration rather than on the call.

## Usage

### Example 1: Implementing a Custom Rule

```csharp
public sealed class LeadingWildcardDetectorPlugin : IDetectorPlugin
{
    public string RuleId => "leading-wildcard";

    public IEnumerable<PerformanceIssue> Analyze(DatabaseQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (query.QueryText.Contains("LIKE '%", StringComparison.OrdinalIgnoreCase))
        {
            // Build and yield a PerformanceIssue describing the finding
        }
    }
}
```

### Example 2: Passing Rules to the Orchestrator

```csharp
IDetectorPlugin[] rules =
{
    new SelectStarDetectorPlugin(),
    new LeadingWildcardDetectorPlugin(),
};

var detector = new PerformanceIssueDetectorService(logger, settings: null, rules);
```

If `plugins` is null or empty, `PerformanceIssueDetectorService` falls back to the default built-in rule set. The `Plugins` property exposes the registered rules as `IReadOnlyList<IDetectorPlugin>`, so callers can list the rule IDs.

## Notes

*   **Built-in Implementations**: The six built-in rules in `Services/DetectorPlugins.cs` implement this interface. See [DetectorPlugins](DetectorPlugins.md) for their behavior.
*   **Separate from `IAnalysisPlugin`**: `IDetectorPlugin` analyzes one `DatabaseQuery` at a time and returns `PerformanceIssue` values synchronously. It is not the same contract as `IAnalysisPlugin`, which is asynchronous and works with `QueryContext` and `QueryAnalysisResult`.
*   **Rule Identity**: `RuleId` is the only identifier the interface requires. Keep it stable, because it is the key used for toggling and correlation.
*   **Empty Results**: Return an empty sequence rather than `null` when a rule does not apply.
