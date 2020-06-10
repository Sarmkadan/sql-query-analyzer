# QueryRewriteSuggestion

A data transfer object representing a suggested rewrite of a SQL query, including metadata about the rewrite's impact, rationale, and associated optimizations. Used to communicate query improvement opportunities between components of the SQL Query Analyzer system.

## API

### `public string SuggestionId`
A unique identifier for this rewrite suggestion. Used to correlate suggestions across different components or requests.

### `public string OriginalQuery`
The original SQL query text before any rewrites were applied. Presented as a raw string without normalization or formatting.

### `public string RewrittenQuery`
The proposed SQL query text after applying the suggested rewrite. May include structural changes, optimizations, or alternative syntax.

### `public RewriteType RewriteType`
The category of rewrite applied. Indicates whether the rewrite is structural, syntactic, or optimization-focused. See `RewriteType` for possible values.

### `public string AffectedClause`
The specific clause or section of the query affected by this rewrite (e.g., `WHERE`, `JOIN`, `SELECT`). May be `null` if not applicable.

### `public string Rationale`
A human-readable explanation of why this rewrite is beneficial. Describes the performance or correctness impact expected from applying the change.

### `public string AdditionalNotes`
Extra context or caveats about the rewrite. May include warnings, assumptions, or limitations of the suggestion.

### `public double EstimatedImprovementPercent`
The estimated percentage improvement in query execution time or resource usage after applying the rewrite. A value between `0.0` and `100.0`, where higher values indicate greater benefit. May be `0.0` if no reliable estimate is available.

### `public bool IsBreakingChange`
Indicates whether applying this rewrite would change the result set or behavior of the query. `true` if the rewrite alters semantics; `false` if the rewrite is purely syntactic or performance-oriented.

### `public bool IsAutoApplicable`
Indicates whether the rewrite can be applied automatically by the system without manual review. `true` if the system deems the change safe and beneficial; `false` if human validation is recommended.

### `public int Priority`
A numerical priority score for this suggestion, where higher values indicate more urgent or impactful changes. Used to order suggestions in UI or processing pipelines.

### `public List<IndexSuggestion> RelatedIndexSuggestions`
A list of index recommendations that complement this query rewrite. Each suggestion proposes an index to create or modify to support the rewritten query. May be empty if no relevant indexes are identified.

### `public DateTime GeneratedAt`
The UTC timestamp when this suggestion was generated. Used for tracking freshness and ordering suggestions by recency.

### `public bool IsValid`
Indicates whether this suggestion is valid and actionable. `false` if the suggestion is stale, malformed, or no longer applicable (e.g., due to schema changes).

### `public string GetRiskLevel()`
Returns a categorical risk assessment for applying this rewrite. Possible values include `"Low"`, `"Medium"`, `"High"`, or `"Critical"`, based on the combination of `IsBreakingChange`, `IsAutoApplicable`, and `EstimatedImprovementPercent`.

#### Returns
- `string`: The risk level as a human-readable label.

### `public string GetSummary()`
Generates a concise summary of the rewrite, suitable for display in a list or dashboard. Combines `SuggestionId`, `RewriteType`, `AffectedClause`, and a truncated version of `Rationale`.

#### Returns
- `string`: A single-line summary of the suggestion.

### `public Dictionary<string, object> ToJsonDictionary()`
Converts the suggestion into a dictionary representation suitable for serialization to JSON. Includes all public properties, with `RelatedIndexSuggestions` serialized as a list of dictionaries.

#### Returns
- `Dictionary<string, object>`: A dictionary containing all public fields and their current values.

## Usage

### Example 1: Basic Inspection
```csharp
var suggestion = new QueryRewriteSuggestion
{
    SuggestionId = "qr-12345",
    OriginalQuery = "SELECT * FROM Orders WHERE CustomerId = 1001",
    RewrittenQuery = "SELECT OrderId, CustomerId FROM Orders WHERE CustomerId = 1001",
    RewriteType = RewriteType.ColumnPruning,
    AffectedClause = "SELECT",
    Rationale = "Removes unused columns to reduce I/O and memory usage.",
    EstimatedImprovementPercent = 15.5,
    IsBreakingChange = false,
    IsAutoApplicable = true,
    Priority = 8,
    GeneratedAt = DateTime.UtcNow,
    IsValid = true
};

Console.WriteLine($"Suggestion: {suggestion.GetSummary()}");
Console.WriteLine($"Risk: {suggestion.GetRiskLevel()}");
```

### Example 2: Serialization and Index Correlation
```csharp
var suggestion = new QueryRewriteSuggestion
{
    SuggestionId = "qr-67890",
    OriginalQuery = "SELECT o.OrderId, c.Name FROM Orders o JOIN Customers c ON o.CustomerId = c.Id WHERE o.Date > '2024-01-01'",
    RewrittenQuery = "SELECT o.OrderId, c.Name FROM Orders o JOIN Customers c ON o.CustomerId = c.Id WHERE o.Date > '2024-01-01' ORDER BY o.Date DESC",
    RewriteType = RewriteType.JoinOrdering,
    AffectedClause = "ORDER BY",
    Rationale = "Adds ordering to leverage index on Orders.Date for faster pagination.",
    EstimatedImprovementPercent = 30.0,
    IsBreakingChange = false,
    IsAutoApplicable = false,
    Priority = 5,
    RelatedIndexSuggestions = new List<IndexSuggestion>
    {
        new IndexSuggestion
        {
            TableName = "Orders",
            ColumnName = "Date",
            SuggestionType = IndexSuggestionType.CreateMissingIndex
        }
    },
    GeneratedAt = DateTime.UtcNow.AddMinutes(-5),
    IsValid = true
};

var jsonDict = suggestion.ToJsonDictionary();
Console.WriteLine($"JSON Keys: {string.Join(", ", jsonDict.Keys)}");
```

## Notes

- **Thread Safety**: This type is immutable with respect to its public surface. All properties are get-only, and methods like `ToJsonDictionary()` return new collections. It is safe to read from multiple threads concurrently. However, the returned `List<IndexSuggestion>` and `Dictionary<string, object>` are mutable; if shared across threads, external synchronization is required.

- **Staleness**: `IsValid` should be checked before applying suggestions, especially in long-running processes. Schema or data changes may invalidate suggestions generated earlier.

- **Risk Assessment**: `GetRiskLevel()` uses heuristics based on `IsBreakingChange` and `EstimatedImprovementPercent`. It does not account for runtime conditions or external state.

- **Serialization**: `ToJsonDictionary()` includes all public properties. If `RelatedIndexSuggestions` is `null`, it will be serialized as an empty list. No circular references are possible.
