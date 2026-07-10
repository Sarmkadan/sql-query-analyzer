# SampleQueryProvider
The `SampleQueryProvider` class provides a set of pre-defined SQL query samples for various use cases, allowing developers to test and analyze query performance, optimization, and potential issues. These samples cover a range of scenarios, including optimized queries, queries with potential performance issues, and complex queries with multiple joins and subqueries.

## API
The `SampleQueryProvider` class offers the following public static members:
* `GetOptimizedQuery`: Returns a sample optimized SQL query.
	+ Parameters: None
	+ Return value: A string representing the optimized query.
	+ Throws: None
* `GetSelectStarQuery`: Returns a sample SQL query using `SELECT *`.
	+ Parameters: None
	+ Return value: A string representing the query.
	+ Throws: None
* `GetNPlusOneQuery`: Returns a sample SQL query that demonstrates the N+1 query issue.
	+ Parameters: None
	+ Return value: A string representing the query.
	+ Throws: None
* `GetImplicitConversionQuery`: Returns a sample SQL query that demonstrates implicit conversion.
	+ Parameters: None
	+ Return value: A string representing the query.
	+ Throws: None
* `GetNonSargableQuery`: Returns a sample SQL query that demonstrates a non-SARGable query.
	+ Parameters: None
	+ Return value: A string representing the query.
	+ Throws: None
* `GetComplexJoinQuery`: Returns a sample SQL query with complex joins.
	+ Parameters: None
	+ Return value: A string representing the query.
	+ Throws: None
* `GetLeadingWildcardQuery`: Returns a sample SQL query with a leading wildcard.
	+ Parameters: None
	+ Return value: A string representing the query.
	+ Throws: None
* `GetOrConditionQuery`: Returns a sample SQL query with an OR condition.
	+ Parameters: None
	+ Return value: A string representing the query.
	+ Throws: None
* `GetSubqueryQuery`: Returns a sample SQL query with a subquery.
	+ Parameters: None
	+ Return value: A string representing the query.
	+ Throws: None
* `GetDistinctQuery`: Returns a sample SQL query using the DISTINCT keyword.
	+ Parameters: None
	+ Return value: A string representing the query.
	+ Throws: None
* `GetSimpleQuery`: Returns a sample simple SQL query.
	+ Parameters: None
	+ Return value: A string representing the query.
	+ Throws: None
* `GetAggregationQuery`: Returns a sample SQL query with aggregation functions.
	+ Parameters: None
	+ Return value: A string representing the query.
	+ Throws: None
* `GetCteQuery`: Returns a sample SQL query using a Common Table Expression (CTE).
	+ Parameters: None
	+ Return value: A string representing the query.
	+ Throws: None
* `GetVeryComplexQuery`: Returns a sample very complex SQL query.
	+ Parameters: None
	+ Return value: A string representing the query.
	+ Throws: None
* `GetAllSamples`: Returns a dictionary containing all sample queries.
	+ Parameters: None
	+ Return value: A dictionary with query names as keys and query strings as values.
	+ Throws: None
* `GetRandomSample`: Returns a random sample query.
	+ Parameters: None
	+ Return value: A string representing the random query.
	+ Throws: None
* `GetSamplesByIssueType`: Returns a dictionary containing sample queries grouped by issue type.
	+ Parameters: None
	+ Return value: A dictionary with issue types as keys and lists of query strings as values.
	+ Throws: None

## Usage
Here are two examples of using the `SampleQueryProvider` class:
```csharp
// Example 1: Get a sample optimized query
string optimizedQuery = SampleQueryProvider.GetOptimizedQuery();
Console.WriteLine(optimizedQuery);

// Example 2: Get all sample queries and iterate over them
Dictionary<string, string> allSamples = SampleQueryProvider.GetAllSamples();
foreach (var sample in allSamples)
{
    Console.WriteLine($"Query: {sample.Key}");
    Console.WriteLine($"SQL: {sample.Value}");
}
```

## Notes
* All methods are static, making them thread-safe and accessible without creating an instance of the class.
* The `GetRandomSample` method may return any of the available sample queries, and its behavior is non-deterministic.
* The `GetSamplesByIssueType` method returns a dictionary with issue types as keys, which can be used to categorize and analyze the sample queries.
* The sample queries provided by this class are for demonstration and testing purposes only and may not be suitable for production use without modification.
