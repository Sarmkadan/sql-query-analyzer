# IAnalysisRepository

`IAnalysisRepository` defines the storage abstraction for query analysis results. Implementations can persist results in memory, a database, a file system, or another backing store while consumers depend on a consistent asynchronous API.

Namespace: `SqlQueryAnalyzer.Repositories`

Source: `Repositories/IAnalysisRepository.cs`

## Members

### `SaveAnalysisAsync`

```csharp
Task<QueryAnalysisResult> SaveAnalysisAsync(QueryAnalysisResult result)
```

Persists the supplied analysis result and returns the saved result.

- `result`: The query analysis result to persist.

### `GetAnalysisAsync`

```csharp
Task<QueryAnalysisResult?> GetAnalysisAsync(string analysisId)
```

Retrieves an analysis result by its identifier. The result is `null` when the repository contains no matching analysis.

- `analysisId`: The identifier of the analysis to retrieve.

### `GetAnalysesForQueryAsync`

```csharp
Task<List<QueryAnalysisResult>> GetAnalysesForQueryAsync(string queryHash)
```

Retrieves all analysis results associated with a query hash. An implementation may return an empty list when no results match.

- `queryHash`: The hash used to identify the query.

### `DeleteAnalysisAsync`

```csharp
Task DeleteAnalysisAsync(string analysisId)
```

Deletes the analysis result identified by `analysisId`.

- `analysisId`: The identifier of the analysis to delete.

### `GetRecentAnalysesAsync`

```csharp
Task<List<QueryAnalysisResult>> GetRecentAnalysesAsync(int count = 100)
```

Retrieves up to `count` recent analysis results. If the caller omits `count`, the requested maximum is 100 results.

- `count`: The maximum number of recent results to retrieve.

## Purpose

The interface separates analysis workflows from persistence details. This allows callers to save, find, list, and delete analysis results without knowing which storage technology an implementation uses, and allows repository implementations to be substituted for development, testing, or production use.
