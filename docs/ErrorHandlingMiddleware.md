# ErrorHandlingMiddleware
The `ErrorHandlingMiddleware` class is designed to handle errors that occur during the execution of SQL queries, providing a robust mechanism for error reporting, degradation strategies, and recovery suggestions. It allows developers to execute queries with error handling and degradation capabilities, ensuring that applications remain stable and informative in the face of errors.

## API
### Constructors
* `public ErrorHandlingMiddleware`: Initializes a new instance of the `ErrorHandlingMiddleware` class.

### Methods
* `public async Task<T> ExecuteWithErrorHandlingAsync<T>`: Executes a query with error handling, returning a result of type `T`. This method takes care of catching and handling exceptions, providing a robust error reporting mechanism.
* `public async Task<T> ExecuteWithDegradationAsync<T>`: Executes a query with a degradation strategy, returning a result of type `T`. This method allows for a fallback approach when errors occur, ensuring that the application remains functional.

### Properties
* `public string ErrorMessage`: Gets the error message associated with the last error that occurred.
* `public string ErrorType`: Gets the type of error that occurred.
* `public string StackTrace`: Gets the stack trace associated with the last error that occurred.
* `public string Context`: Gets the context in which the error occurred.
* `public DateTime Timestamp`: Gets the timestamp when the error occurred.
* `public bool IsRecoverable`: Gets a value indicating whether the error is recoverable.
* `public string Suggestion`: Gets a suggestion for recovering from the error.
* `public DegradationStrategy`: Gets the degradation strategy used by the middleware.

### Other Members
* `public ErrorReport CreateErrorReport`: Creates an error report based on the last error that occurred.
* `public override string ToString`: Returns a string representation of the error handling middleware.

## Usage
The following examples demonstrate how to use the `ErrorHandlingMiddleware` class:
```csharp
// Example 1: Executing a query with error handling
var middleware = new ErrorHandlingMiddleware();
try
{
    var result = await middleware.ExecuteWithErrorHandlingAsync<string>(async () =>
    {
        // Execute a SQL query
        var query = "SELECT * FROM table";
        var connectionString = "Server=myServer;Database=myDatabase;User Id=myUser;Password=myPassword;";
        using (var connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();
            using (var command = new SqlCommand(query, connection))
            {
                var reader = await command.ExecuteReaderAsync();
                // Process the query results
            }
        }
    });
    Console.WriteLine(result);
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}

// Example 2: Executing a query with degradation strategy
var middleware = new ErrorHandlingMiddleware();
try
{
    var result = await middleware.ExecuteWithDegradationAsync<string>(async () =>
    {
        // Execute a SQL query
        var query = "SELECT * FROM table";
        var connectionString = "Server=myServer;Database=myDatabase;User Id=myUser;Password=myPassword;";
        using (var connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();
            using (var command = new SqlCommand(query, connection))
            {
                var reader = await command.ExecuteReaderAsync();
                // Process the query results
            }
        }
    });
    Console.WriteLine(result);
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}
```

## Notes
When using the `ErrorHandlingMiddleware` class, consider the following edge cases and thread-safety remarks:
* The `ExecuteWithErrorHandlingAsync` and `ExecuteWithDegradationAsync` methods are asynchronous, ensuring that the calling thread remains responsive. However, the underlying query execution may still block or timeout, depending on the database connection and query complexity.
* The `ErrorReport` created by the `CreateErrorReport` method contains sensitive information, such as the error message, stack trace, and context. Ensure that this information is handled and stored securely to prevent unauthorized access or data breaches.
* The `DegradationStrategy` property allows for customization of the degradation approach. However, the specific implementation details and trade-offs between different strategies are not specified by the `ErrorHandlingMiddleware` class. Developers should carefully evaluate and test their chosen degradation strategy to ensure it meets the application's requirements and performance characteristics.
