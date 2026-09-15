# HttpQueryAnalysisClientValidation

Provides validation helpers for `HttpQueryAnalysisClient` instances. Validates constructor arguments, method parameters, and internal state.

## Members

### Validate(value)
Validates the specified `HttpQueryAnalysisClient` instance.

- **Parameters**:
  - `value`: The HTTP query analysis client to validate.
- **Returns**: An immutable list of validation errors; empty if valid.
- **Exceptions**:
  - `ArgumentNullException`: Thrown when `value` is null.

### IsValid(value)
Determines whether the specified `HttpQueryAnalysisClient` instance is valid.

- **Parameters**:
  - `value`: The HTTP query analysis client to check.
- **Returns**: True if the instance is valid; otherwise false.

### EnsureValid(value)
Ensures that the specified `HttpQueryAnalysisClient` instance is valid.

- **Parameters**:
  - `value`: The HTTP query analysis client to validate.
- **Exceptions**:
  - `ArgumentNullException`: Thrown when `value` is null.
  - `ArgumentException`: Thrown when validation fails, containing error messages.

### ValidateQuery(query)
Validates the query parameter for `HttpQueryAnalysisClient.AnalyzeQueryAsync(string)`.

- **Parameters**:
  - `query`: The SQL query to validate.
- **Returns**: An immutable list of validation errors; empty if valid.
- **Exceptions**:
  - `ArgumentException`: Thrown when query is null, empty, or exceeds maximum length.

### IsValidQuery(query)
Determines whether the specified query is valid.

- **Parameters**:
  - `query`: The SQL query to check.
- **Returns**: True if the query is valid; otherwise false.

### EnsureValidQuery(query)
Ensures that the specified query is valid.

- **Parameters**:
  - `query`: The SQL query to validate.
- **Exceptions**:
  - `ArgumentException`: Thrown when validation fails, containing error messages.

### ValidateQueries(queries)
Validates the queries array for `HttpQueryAnalysisClient.AnalyzeBatchAsync(string[])`.

- **Parameters**:
  - `queries`: The array of SQL queries to validate.
- **Returns**: An immutable list of validation errors; empty if valid.
- **Exceptions**:
  - `ArgumentNullException`: Thrown when queries array is null.

### IsValidQueries(queries)
Determines whether the specified queries array is valid.

- **Parameters**:
  - `queries`: The array of SQL queries to check.
- **Returns**: True if the queries array is valid; otherwise false.

### EnsureValidQueries(queries)
Ensures that the specified queries array is valid.

- **Parameters**:
  - `queries`: The array of SQL queries to validate.
- **Exceptions**:
  - `ArgumentException`: Thrown when validation fails, containing error messages.

### ValidateOptions(options)
Validates the options dictionary for query analysis.

- **Parameters**:
  - `options`: The options dictionary to validate.
- **Returns**: An immutable list of validation errors; empty if valid.
- **Exceptions**:
  - `ArgumentException`: Thrown when options dictionary is invalid.

### IsValidOptions(options)
Determines whether the specified options dictionary is valid.

- **Parameters**:
  - `options`: The options dictionary to check.
- **Returns**: True if the options dictionary is valid; otherwise false.

### EnsureValidOptions(options)
Ensures that the specified options dictionary is valid.

- **Parameters**:
  - `options`: The options dictionary to validate.
- **Exceptions**:
  - `ArgumentException`: Thrown when validation fails, containing error messages.

### ValidateMaxDegreeOfParallelism(maxDegreeOfParallelism)
Validates the max degree of parallelism parameter.

- **Parameters**:
  - `maxDegreeOfParallelism`: The maximum degree of parallelism.
- **Returns**: An immutable list of validation errors; empty if valid.

### IsValidMaxDegreeOfParallelism(maxDegreeOfParallelism)
Determines whether the specified max degree of parallelism is valid.

- **Parameters**:
  - `maxDegreeOfParallelism`: The maximum degree of parallelism to check.
- **Returns**: True if the max degree of parallelism is valid; otherwise false.

### EnsureValidMaxDegreeOfParallelism(maxDegreeOfParallelism)
Ensures that the specified max degree of parallelism is valid.

- **Parameters**:
  - `maxDegreeOfParallelism`: The maximum degree of parallelism to validate.
- **Exceptions**:
  - `ArgumentException`: Thrown when validation fails, containing error messages.

### ValidateTimeoutSeconds(timeoutSeconds)
Validates the timeout parameter in seconds.

- **Parameters**:
  - `timeoutSeconds`: The timeout in seconds.
- **Returns**: An immutable list of validation errors; empty if valid.
- **Exceptions**:
  - `ArgumentException`: Thrown when timeout is not positive or exceeds maximum.

### IsValidTimeoutSeconds(timeoutSeconds)
Determines whether the specified timeout in seconds is valid.

- **Parameters**:
  - `timeoutSeconds`: The timeout in seconds to check.
- **Returns**: True if the timeout is valid; otherwise false.

### EnsureValidTimeoutSeconds(timeoutSeconds)
Ensures that the specified timeout in seconds is valid.

- **Parameters**:
  - `timeoutSeconds`: The timeout in seconds to validate.
- **Exceptions**:
  - `ArgumentException`: Thrown when validation fails, containing error messages.

## Usage Example
```csharp
using System;
using SqlQueryAnalyzer.Integration;

public class HttpQueryAnalysisClientValidator
{
    public void ValidateClient()
    {
        var client = new HttpQueryAnalysisClient
        {
            // Configure client properties
            Endpoint = "https://api.example.com/query",
            ApiKey = "your-api-key",
            TimeoutSeconds = 30
        };

        // Validate the client instance
        var errors = client.Validate();
        if (errors.Count > 0)
        {
            Console.WriteLine("Validation errors:");
            foreach (var error in errors)
            {
                Console.WriteLine($"- {error}");
            }
        }
        else
        {
            Console.WriteLine("Client is valid.");
        }

        // Check if client is valid
        bool isValid = client.IsValid();
        Console.WriteLine($"Is valid: {isValid}");

        // Ensure client is valid (throws exception if invalid)
        try
        {
            client.EnsureValid();
            Console.WriteLine("Client ensured valid.");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Validation failed: {ex.Message}");
        }

        // Validate a query
        string query = "SELECT * FROM table WHERE id = 1";
        var queryErrors = query.ValidateQuery();
        if (queryErrors.Count == 0)
        {
            Console.WriteLine("Query is valid.");
        }
        else
        {
            Console.WriteLine("Query validation errors:");
            foreach (var error in queryErrors)
            {
                Console.WriteLine($"- {error}");
            }
        }

        // Validate queries array
        string[] queries = { "SELECT * FROM table1", "SELECT * FROM table2" };
        var queriesErrors = queries.ValidateQueries();
        if (queriesErrors.Count == 0)
        {
            Console.WriteLine("Queries array is valid.");
        }
        else
        {
            Console.WriteLine("Queries array validation errors:");
            foreach (var error in queriesErrors)
            {
                Console.WriteLine($"- {error}");
            }
        }

        // Validate options
        var options = new Dictionary<string, string>
        {
            { "option1", "value1" },
            { "option2", "value2" }
        };
        var optionsErrors = options.ValidateOptions();
        if (optionsErrors.Count == 0)
        {
            Console.WriteLine("Options are valid.");
        }
        else
        {
            Console.WriteLine("Options validation errors:");
            foreach (var error in optionsErrors)
            {
                Console.WriteLine($"- {error}");
            }
        }

        // Validate max degree of parallelism
        int? maxDop = 4;
        var maxDopErrors = maxDop.ValidateMaxDegreeOfParallelism();
        if (maxDopErrors.Count == 0)
        {
            Console.WriteLine("Max degree of parallelism is valid.");
        }
        else
        {
            Console.WriteLine("Max degree of parallelism validation errors:");
            foreach (var error in maxDopErrors)
            {
                Console.WriteLine($"- {error}");
            }
        }

        // Validate timeout seconds
        int timeout = 30;
        var timeoutErrors = timeout.ValidateTimeoutSeconds();
        if (timeoutErrors.Count == 0)
        {
            Console.WriteLine("Timeout is valid.");
        }
        else
        {
            Console.WriteLine("Timeout validation errors:");
            foreach (var error in timeoutErrors)
            {
                Console.WriteLine($"- {error}");
            }
        }
    }
}
```