# QueryValidator

`QueryValidator` is a static utility class in the `sql-query-analyzer` project that provides methods for validating SQL queries, database connections, and index configurations, as well as detecting potential SQL injection vulnerabilities and sanitizing queries for safe display.

## API

### `IsValidQuery`

**Purpose**: Determines whether a given SQL query string is syntactically valid.

**Parameters**:
- `query` (string): The SQL query to validate.

**Return Value**: `bool` - `true` if the query is valid; otherwise, `false`.

**Exceptions**: Throws `ArgumentNullException` if `query` is `null`.

---

### `ValidateDatabaseQuery`

**Purpose**: Validates a SQL query against a specified database connection to ensure it can be executed without errors.

**Parameters**:
- `query` (string): The SQL query to validate.
- `connectionString` (string): The connection string to the target database.

**Return Value**: `void`

**Exceptions**: Throws `ArgumentNullException` if either parameter is `null`. Throws `SqlException` if the query fails validation against the database.

---

### `ValidateAnalysisResult`

**Purpose**: Validates the structure and content of an analysis result object produced by the query analyzer.

**Parameters**:
- `result` (AnalysisResult): The analysis result to validate.

**Return Value**: `void`

**Exceptions**: Throws `ArgumentNullException` if `result` is `null`. Throws `InvalidOperationException` if the result contains invalid or incomplete data.

---

### `ValidateIndex`

**Purpose**: Validates the configuration of a database index, including its name, columns, and uniqueness constraints.

**Parameters**:
- `indexName` (string): The name of the index.
- `columns` (IEnumerable<string>): The columns included in the index.
- `isUnique` (bool): Whether the index enforces uniqueness.

**Return Value**: `void`

**Exceptions**: Throws `ArgumentNullException` if `indexName` or `columns` is `null`. Throws `ArgumentException` if `columns` is empty or contains invalid identifiers.

---

### `ValidateIndexSuggestion`

**Purpose**: Validates a suggested index configuration for correctness and applicability.

**Parameters**:
- `suggestion` (IndexSuggestion): The index suggestion to validate.

**Return Value**: `void`

**Exceptions**: Throws `ArgumentNullException` if `suggestion` is `null`. Throws `InvalidOperationException` if the suggestion is not applicable to the analyzed query.

---

### `DetectSQLInjectionRisks`

**Purpose**: Analyzes a SQL query for patterns indicative of SQL injection vulnerabilities.

**Parameters**:
- `query` (string): The SQL query to analyze.

**Return Value**: `List<string>` - A list of detected risk descriptions. Returns an empty list if no risks are found.

**Exceptions**: Throws `ArgumentNullException` if `query` is `null`.

---

### `ValidateConnectionString`

**Purpose**: Validates the format and required components of a database connection string.

**Parameters**:
- `connectionString` (string): The connection string to validate.

**Return Value**: `void`

**Exceptions**: Throws `ArgumentNullException` if `connectionString` is `null`. Throws `FormatException` if the connection string is malformed or missing required keys.

---

### `SanitizeQueryForDisplay`

**Purpose**: Sanitizes a SQL query string by removing or obfuscating sensitive data for safe display in logs or UI.

**Parameters**:
- `query` (string): The SQL query to sanitize.

**Return Value**: `string` - The sanitized query string.

**Exceptions**: Throws `ArgumentNullException` if `query` is `null`.

## Usage

```csharp
// Example 1: Validate a query and check for SQL injection risks
string query = "SELECT * FROM Users WHERE Id = @userId";
bool isValid = QueryValidator.IsValidQuery(query);
List<string> risks = QueryValidator.DetectSQLInjectionRisks(query);

if (isValid && risks.Count == 0)
{
    Console.WriteLine("Query is valid and safe.");
}
else
{
    Console.WriteLine($"Validation failed or risks detected: {string.Join(", ", risks)}");
}
```

```csharp
// Example 2: Validate a connection string and execute database query validation
string connectionString = "Server=myServer;Database=myDB;User Id=myUser;Password=myPass;";
try
{
    QueryValidator.ValidateConnectionString(connectionString);
    QueryValidator.ValidateDatabaseQuery("SELECT * FROM Products", connectionString);
    Console.WriteLine("Connection and query validated successfully.");
}
catch (Exception ex)
{
    Console.WriteLine($"Validation error: {ex.Message}");
}
```

## Notes

- All methods in `QueryValidator` are static and do not maintain instance state. Thread-safety depends on the implementation of underlying dependencies (e.g., database connections). External synchronization may be required for concurrent access to shared resources.
- Methods that accept string parameters (`IsValidQuery`, `ValidateDatabaseQuery`, `DetectSQLInjectionRisks`, `ValidateConnectionString`, `SanitizeQueryForDisplay`) throw `ArgumentNullException` for `null` inputs. Empty strings are generally treated as invalid unless explicitly allowed by the method's logic.
- `ValidateDatabaseQuery` requires an active database connection and may throw `SqlException` for connectivity issues or query execution errors unrelated to syntax validity.
- `SanitizeQueryForDisplay` does not modify the original query string but returns a new sanitized version. It is intended for display purposes only and does not alter query functionality.
