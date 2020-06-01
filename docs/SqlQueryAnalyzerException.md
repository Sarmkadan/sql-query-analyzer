# SqlQueryAnalyzerException

The `SqlQueryAnalyzerException` is the base exception class for the `sql-query-analyzer` library, designed to encapsulate error conditions that occur during the parsing, validation, or execution of SQL query analysis tasks. It provides a structured approach for reporting errors, ensuring that consuming applications can reliably identify and handle specific failure scenarios such as syntax errors, connectivity issues, or invalid query configurations.

## API

### SqlQueryAnalyzerException

*   `public SqlQueryAnalyzerException(string message)`: Initializes a new instance with a specified error message.
*   `public SqlQueryAnalyzerException(string message, Exception innerException)`: Initializes a new instance with a specified error message and a reference to the inner exception that caused this exception.
*   `public string? ErrorCode`: Gets or sets an optional machine-readable error code describing the exception type.
*   `public string? ErrorDetails`: Gets or sets optional detailed information regarding the exception.

### AnalysisException

*   `public AnalysisException(string message)`: Initializes a new instance with a specified error message.
*   `public AnalysisException()`: Initializes a new instance.
*   `public AnalysisException(string message, Exception innerException)`: Initializes a new instance with a message and inner exception.
*   `public AnalysisException(SerializationInfo info, StreamingContext context)`: Initializes a new instance with serialized data.
*   `public string Query`: Gets or sets the SQL query text that caused the analysis failure.
*   `public int? LineNumber`: Gets or sets the line number in the query where the error occurred, if applicable.
*   `public int? ColumnNumber`: Gets or sets the column number in the query where the error occurred, if applicable.

### InvalidQueryException

*   `public InvalidQueryException(string message)`: Initializes a new instance with a specified error message, automatically setting the internal error code to "INVALID_QUERY".
*   `public InvalidQueryException()`: Initializes a new instance.
*   `public InvalidQueryException(string message, Exception innerException)`: Initializes a new instance with a message and inner exception.

### DatabaseConnectionException

*   `public DatabaseConnectionException()`: Initializes a new instance.
*   `public DatabaseConnectionException(string message)`: Initializes a new instance with a specified error message.
*   `public DatabaseConnectionException(string message, Exception innerException)`: Initializes a new instance with a message and inner exception.
*   `public string? ConnectionString`: Gets or sets the connection string used during the failed database attempt.
*   `public string? DatabaseName`: Gets or sets the name of the database that failed to connect.
*   `public string? PlanSource`: Gets or sets the source of the query plan, if applicable to the connection error.

## Usage

### Handling Query Analysis Errors

```csharp
try 
{
    analyzer.Analyze("SELECT * FROM NonExistentTable");
}
catch (AnalysisException ex)
{
    Console.WriteLine($"Analysis failed at line {ex.LineNumber}: {ex.Message}");
}
```

### Handling Database Connection Failures

```csharp
try 
{
    var connection = await dbConnector.ConnectAsync();
}
catch (DatabaseConnectionException ex)
{
    Console.WriteLine($"Failed to connect to database: {ex.DatabaseName}");
    // Log connection string (ensure sensitive info is masked in production logs)
    Logger.LogError($"Connection failed: {ex.ConnectionString}");
}
```

## Notes

*   **Thread-Safety**: All exception types listed are immutable after instantiation, making them safe to throw and catch across different threads.
*   **Nullability**: Members marked with `?` (such as `ErrorCode`, `ErrorDetails`, `ConnectionString`, `DatabaseName`, and `PlanSource`) may be null if the information was not available at the time the exception was thrown.
*   **Inheritance**: `AnalysisException`, `InvalidQueryException`, and `DatabaseConnectionException` inherit from `SqlQueryAnalyzerException`. Consumers can catch `SqlQueryAnalyzerException` to handle any error arising from the library, or catch more specific derived types for granular error handling.
