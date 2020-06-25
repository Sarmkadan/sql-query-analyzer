# IAnalysisRepository
The `IAnalysisRepository` interface is designed to provide a standardized way of interacting with a repository of query analysis results and database indexes. It offers a range of methods for saving, retrieving, and managing analysis results, as well as creating and manipulating database indexes. This interface is intended to be implemented by concrete repository classes, such as `InMemoryAnalysisRepository` and `InMemoryIndexRepository`, which provide the actual data storage and retrieval mechanisms.

## API
The `IAnalysisRepository` interface includes the following members:
* `InMemoryAnalysisRepository`: A property that returns an instance of `InMemoryAnalysisRepository`.
* `SaveAnalysisAsync`: Saves a query analysis result asynchronously. Returns a `Task<QueryAnalysisResult>`.
* `GetAnalysisAsync`: Retrieves a query analysis result by its identifier asynchronously. Returns a `Task<QueryAnalysisResult?>`.
* `GetAllAnalysesAsync`: Retrieves all query analysis results asynchronously. Returns a `Task<List<QueryAnalysisResult>>`.
* `GetAnalysesByDateRangeAsync`: Retrieves query analysis results within a specified date range asynchronously. Returns a `Task<List<QueryAnalysisResult>>`.
* `GetAnalysesForQueryAsync`: Retrieves query analysis results for a specific query asynchronously. Returns a `Task<List<QueryAnalysisResult>>`.
* `DeleteAnalysisAsync`: Deletes a query analysis result asynchronously.
* `GetRecentAnalysesAsync`: Retrieves the most recent query analysis results asynchronously. Returns a `Task<List<QueryAnalysisResult>>`.
* `GetIssuesByTypeAsync`: Retrieves performance issues of a specific type asynchronously. Returns a `Task<List<PerformanceIssue>>`.
* `GetCriticalIssuesAsync`: Retrieves critical performance issues asynchronously. Returns a `Task<List<PerformanceIssue>>`.
* `GetTotalIssueCountAsync`: Retrieves the total count of performance issues asynchronously. Returns a `Task<int>`.
* `InMemoryIndexRepository`: A property that returns an instance of `InMemoryIndexRepository`.
* `GetIndexByNameAsync`: Retrieves a database index by its name asynchronously. Returns a `Task<ModelIndex?>`.
* `GetIndexesByTableAsync`: Retrieves database indexes for a specific table asynchronously. Returns a `Task<List<ModelIndex>>`.
* `GetAllIndexesAsync`: Retrieves all database indexes asynchronously. Returns a `Task<List<ModelIndex>>`.
* `GetUnusedIndexesAsync`: Retrieves unused database indexes asynchronously. Returns a `Task<List<ModelIndex>>`.
* `GetFragmentedIndexesAsync`: Retrieves fragmented database indexes asynchronously. Returns a `Task<List<ModelIndex>>`.
* `AddIndexAsync`: Creates a new database index asynchronously. Returns a `Task<ModelIndex>`.
* `SaveIndexAsync`: Saves a database index asynchronously.
* `GetIndexesForTableAsync`: Retrieves database indexes for a specific table asynchronously. Returns a `Task<List<ModelIndex>>`.

## Usage
Here are two examples of using the `IAnalysisRepository` interface:
```csharp
// Example 1: Saving and retrieving a query analysis result
var analysisRepository = new InMemoryAnalysisRepository();
var queryAnalysisResult = new QueryAnalysisResult { Query = "SELECT * FROM table" };
await analysisRepository.SaveAnalysisAsync(queryAnalysisResult);
var retrievedResult = await analysisRepository.GetAnalysisAsync(queryAnalysisResult.Id);
Console.WriteLine(retrievedResult.Query); // Output: SELECT * FROM table

// Example 2: Creating and retrieving a database index
var indexRepository = new InMemoryIndexRepository();
var modelIndex = new ModelIndex { Name = "index_name", Table = "table_name" };
await indexRepository.AddIndexAsync(modelIndex);
var retrievedIndex = await indexRepository.GetIndexByNameAsync(modelIndex.Name);
Console.WriteLine(retrievedIndex.Name); // Output: index_name
```

## Notes
When using the `IAnalysisRepository` interface, consider the following edge cases and thread-safety remarks:
* The `SaveAnalysisAsync` and `SaveIndexAsync` methods may throw exceptions if the data cannot be saved, such as due to database connectivity issues or data validation errors.
* The `GetAnalysisAsync` and `GetIndexByNameAsync` methods may return null if no matching result is found.
* The `GetAllAnalysesAsync` and `GetAllIndexesAsync` methods may return an empty list if no results are found.
* The `IAnalysisRepository` interface is designed to be thread-safe, allowing multiple concurrent accesses to the repository. However, the underlying implementation may have specific thread-safety considerations, such as locking mechanisms or connection pooling.
