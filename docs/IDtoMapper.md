# IDtoMapper

`IDtoMapper` is the contract for converting domain models (analysis results, performance issues, index metadata, queries) into the DTOs returned by the API and CLI layers. It keeps the mapping logic behind an interface so controllers depend on the abstraction rather than on a concrete mapper, and so the mapper can be replaced or mocked in tests.

**Namespace**: `SqlQueryAnalyzer.DTOs`
**Source**: `DTOs/IDtoMapper.cs`
**Default implementation**: `DtoMapper` (`DTOs/DtoMapper.cs`), registered as a singleton in `Program.cs`:

```csharp
services.AddSingleton<IDtoMapper, DtoMapper>();
```

## Members

All members are instance methods. None of them is `async`, and none of them takes a cancellation token.

### `AnalysisResponseDto ToResponseDto(QueryAnalysisResult analysis, long analysisTimeMs)`

Maps a single query analysis into the top-level response DTO. `analysisTimeMs` is the elapsed analysis time in milliseconds and is passed through to the response, so the caller measures timing and the mapper only reports it.

- **`analysis`**: the completed `QueryAnalysisResult`.
- **`analysisTimeMs`**: analysis duration in milliseconds.
- **Returns**: `AnalysisResponseDto`.

### `PerformanceIssueDto ToIssueDto(PerformanceIssue issue)`

Maps one detected `PerformanceIssue` (for example a table scan, cartesian join, `SELECT *`, or N+1 pattern) into its DTO form.

- **`issue`**: the detected issue.
- **Returns**: `PerformanceIssueDto`.

### `IndexSuggestionDto ToSuggestionDto(IndexSuggestion suggestion)`

Maps an `IndexSuggestion` (a recommended index with its rationale) into its DTO form.

- **`suggestion`**: the index recommendation.
- **Returns**: `IndexSuggestionDto`.

### `IndexDetailDto ToIndexDetailDto(ModelIndex index)`

Maps an existing database index into a detail DTO. `ModelIndex` is an alias for `SqlQueryAnalyzer.Models.Index`. The alias is needed because `Index` collides with `System.Index` when `System` is imported.

- **`index`**: the index model.
- **Returns**: `IndexDetailDto`.

### `IndexAnalysisResponseDto ToIndexAnalysisResponseDto(string tableName, List<ModelIndex> indexes, List<IndexSuggestion> suggestions, List<string> scripts)`

Builds the response for an index analysis of one table. It combines the table name, the existing indexes, the suggested indexes, and the generated DDL scripts into one payload.

- **`tableName`**: table being analyzed.
- **`indexes`**: existing indexes on the table.
- **`suggestions`**: recommended indexes.
- **`scripts`**: DDL scripts for the suggestions.
- **Returns**: `IndexAnalysisResponseDto`.

### `QueryDetailDto ToQueryDetailDto(DatabaseQuery query)`

Maps a stored `DatabaseQuery` into its detail DTO.

- **`query`**: the database query model.
- **Returns**: `QueryDetailDto`.

### `BatchAnalysisResponseDto ToBatchResponseDto(List<QueryAnalysisResult> results, List<string> nPlusOnePatterns, long totalTimeMs)`

Builds the response for a batch analysis. It combines the per-query results, the N+1 patterns detected across the batch, and the total elapsed time.

- **`results`**: analysis result for each query in the batch.
- **`nPlusOnePatterns`**: N+1 patterns found across the batch.
- **`totalTimeMs`**: total batch duration in milliseconds.
- **Returns**: `BatchAnalysisResponseDto`.

## Usage in an implementing class

Inject `IDtoMapper` through the constructor in any class that builds a response, and call it there. Do not construct `DtoMapper` directly, so the registration in `Program.cs` stays the single place that decides which implementation is used. The example below is illustrative and is not an existing class in the codebase:

```csharp
public sealed class ResponseBuilder
{
    private readonly IDtoMapper _mapper;

    public ResponseBuilder(IDtoMapper mapper)
    {
        _mapper = mapper;
    }

    public AnalysisResponseDto Build(QueryAnalysisResult analysis, long elapsedMs)
        => _mapper.ToResponseDto(analysis, elapsedMs);
}
```

In unit tests, mock the interface (for example with Moq, `new Mock<IDtoMapper>()`) to isolate controller behavior from mapping behavior.
