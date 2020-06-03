# HttpQueryAnalysisClient
The `HttpQueryAnalysisClient` class is designed to analyze SQL queries and provide insights into their structure and performance. It allows users to analyze individual queries or batches of queries, check the health of the analysis service, and retrieve the version of the service. This client is useful for developers and database administrators who need to optimize and troubleshoot their SQL queries.

## API
### Constructors
* `public HttpQueryAnalysisClient`: Initializes a new instance of the `HttpQueryAnalysisClient` class.

### Methods
* `public async Task<QueryAnalysisResult> AnalyzeQueryAsync`: Analyzes a single SQL query and returns the analysis result. The query to be analyzed is specified by the `Query` property. This method throws if the analysis fails or if the query is invalid.
* `public async Task<List<QueryAnalysisResult>> AnalyzeBatchAsync`: Analyzes a batch of SQL queries and returns a list of analysis results. The queries to be analyzed are specified by the `Queries` property. This method throws if the analysis fails or if any of the queries are invalid.
* `public async Task<bool> IsHealthyAsync`: Checks the health of the analysis service and returns a boolean indicating whether the service is healthy. This method throws if the health check fails.
* `public async Task<string?> GetVersionAsync`: Retrieves the version of the analysis service and returns it as a string. This method throws if the version retrieval fails.

### Properties
* `public string Query`: Gets or sets the SQL query to be analyzed.
* `public Dictionary<string, string>? Options`: Gets or sets additional options for the analysis.
* `public string[] Queries`: Gets or sets the batch of SQL queries to be analyzed.
* `public int? MaxDegreeOfParallelism`: Gets or sets the maximum degree of parallelism for the analysis.

## Usage
```csharp
// Analyzing a single query
var client = new HttpQueryAnalysisClient();
client.Query = "SELECT * FROM customers";
var result = await client.AnalyzeQueryAsync();
Console.WriteLine(result);

// Analyzing a batch of queries
var client = new HttpQueryAnalysisClient();
client.Queries = new[] { "SELECT * FROM customers", "SELECT * FROM orders" };
var results = await client.AnalyzeBatchAsync();
foreach (var result in results)
{
    Console.WriteLine(result);
}
```

## Notes
The `HttpQueryAnalysisClient` class is designed to be used in a multithreaded environment, but it is not thread-safe by default. Users should ensure that each instance of the class is used by only one thread at a time, or use synchronization mechanisms to protect access to the instance. Additionally, the `MaxDegreeOfParallelism` property can be used to control the level of parallelism for the analysis, but setting it too high can lead to performance issues. The `Options` property can be used to specify additional options for the analysis, but the available options and their effects are not documented here. Users should consult the documentation for the analysis service for more information.
