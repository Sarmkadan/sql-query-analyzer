# StringExtensions

A static utility class providing extension methods for string manipulation tailored to SQL query analysis tasks. These methods facilitate common operations such as whitespace normalization, comment removal, keyword detection, and structural analysis of SQL statements.

## API

### NormalizeSqlWhitespace  
**Purpose**: Replaces sequences of whitespace characters in a SQL string with single spaces and trims leading/trailing whitespace.  
**Parameters**:  
- `this string sql` — The SQL string to normalize.  
**Returns**: A normalized SQL string with consistent whitespace.  
**Exceptions**: None; returns `null` if the input is `null`.

### RemoveSqlComments  
**Purpose**: Strips single-line (`--`) and multi-line (`/* */`) comments from a SQL string.  
**Parameters**:  
- `this string sql` — The SQL string to process.  
**Returns**: The SQL string with comments removed.  
**Exceptions**: None; returns `null` if the input is `null`.

### Truncate  
**Purpose**: Shortens a string to a specified maximum length, appending "..." if truncated.  
**Parameters**:  
- `this string value` — The string to truncate.  
- `int maxLength` — The maximum allowed length.  
**Returns**: The truncated string with "..." appended if necessary.  
**Exceptions**: Throws `ArgumentOutOfRangeException` if `maxLength` is less than 3.

### IsSqlKeyword  
**Purpose**: Determines whether a string represents a SQL keyword (e.g., SELECT, FROM).  
**Parameters**:  
- `this string word` — The string to evaluate.  
**Returns**: `true` if the string is a recognized SQL keyword; otherwise `false`.  
**Exceptions**: None; returns `false` if the input is `null` or empty.

### CapitalizeFirst  
**Purpose**: Converts the first character of a string to uppercase.  
**Parameters**:  
- `this string input` — The string to modify.  
**Returns**: The string with the first character capitalized.  
**Exceptions**: None; returns `null` if the input is `null`.

### ToSnakeCase  
**Purpose**: Transforms a string into snake_case format by inserting underscores before uppercase letters and converting to lowercase.  
**Parameters**:  
- `this string input` — The string to convert.  
**Returns**: The snake_case representation of the input.  
**Exceptions**: None; returns `null` if the input is `null`.

### CountOccurrences  
**Purpose**: Counts the number of times a substring appears within a string.  
**Parameters**:  
- `this string source` — The string to search.  
- `string search` — The substring to count.  
**Returns**: The number of occurrences of `search` in `source`.  
**Exceptions**: Throws `ArgumentNullException` if `search` is `null`.

### ContainsSuspiciousPatterns  
**Purpose**: Checks if a SQL string contains potentially dangerous patterns (e.g., unparameterized queries, excessive wildcards).  
**Parameters**:  
- `this string sql` — The SQL string to analyze.  
**Returns**: `true` if suspicious patterns are detected; otherwise `false`.  
**Exceptions**: None; returns `false` if the input is `null` or empty.

### ExtractQueryType  
**Purpose**: Identifies the primary SQL operation type (e.g., SELECT, INSERT, UPDATE) from a query string.  
**Parameters**:  
- `this string sql` — The SQL string to analyze.  
**Returns**: The detected query type as a string (e.g., "SELECT").  
**Exceptions**: None; returns `null` if the input is `null` or no valid query type is found.

### SplitStatements  
**Purpose**: Splits a SQL string containing multiple statements into individual statements.  
**Parameters**:  
- `this string sql` — The SQL string to split.  
**Returns**: A list of individual SQL statements.  
**Exceptions**: None; returns an empty list if the input is `null` or contains no statements.

### GetPosition  
**Purpose**: Calculates the line and column position of a given index within a SQL string.  
**Parameters**:  
- `this string sql` — The SQL string to analyze.  
- `int index` — The character index to locate.  
**Returns**: A tuple `(int Line, int Column)` representing the position.  
**Exceptions**: Throws `ArgumentOutOfRangeException` if `index` is out of bounds.

## Usage

```csharp
var sql = "SELECT * FROM users -- Get all users\nWHERE id = 1;";
var normalized = sql.NormalizeSqlWhitespace();
// Result: "SELECT * FROM users WHERE id = 1;"

var statements = normalized.SplitStatements();
// Result: ["SELECT * FROM users WHERE id = 1;"]
```

```csharp
var query = "select * from products where name like '%test%'";
var isKeyword = query.IsSqlKeyword("select");
// Result: true

var suspicious = query.ContainsSuspiciousPatterns();
// Result: true (due to '%test%' wildcard)
```

## Notes

- All methods are static and thread-safe, as they do not rely on mutable shared state.  
- Null inputs are generally handled gracefully, returning `null` or default values unless explicitly noted.  
- `Truncate` requires `maxLength >= 3` to accommodate the "..." suffix.  
- `CountOccurrences` and `GetPosition` throw exceptions for invalid arguments to enforce contract correctness.  
- `ExtractQueryType` and `ContainsSuspiciousPatterns` may yield false positives/negatives for complex or non-standard SQL syntax.
