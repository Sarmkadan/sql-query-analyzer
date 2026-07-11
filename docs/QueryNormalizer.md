# QueryNormalizer
The `QueryNormalizer` type provides utilities for normalizing SQL queries and extracting schema identifiers such as table and column names from a query string that has been supplied to the instance (e.g., via its constructor or another initialization mechanism not part of this public API). It also offers a static helper to test whether a given token is an SQL keyword.

## API
### Normalize
- **Purpose**: Returns a normalized form of the SQL query associated with the instance (e.g., standardized whitespace, case-insensitive keyword casing, removal of redundant parentheses).
- **Parameters**: None.
- **Return value**: A `string` containing the normalized query.
- **Exceptions**: 
  - `InvalidOperationException` if the instance has not been initialized with a query.
  - `ArgumentNullException` if the internal query is `null`.

### ExtractTableNames
- **Purpose**: Returns a list of distinct table names referenced in the query.
- **Parameters**: None.
- **Return value**: A `List<string>` containing the table names in the order they appear in the query.
- **Exceptions**: 
  - `InvalidOperationException` if the instance has not been initialized with a query.
  - `ArgumentNullException` if the internal query is `null`.

### ExtractColumnNames
- **Purpose**: Returns a list of distinct column names referenced in the query.
- **Parameters**: None.
- **Return value**: A `List<string>` containing the column names in the order they appear in the query.
- **Exceptions**: 
  - `InvalidOperationException` if the instance has not been initialized with a query.
  - `ArgumentNullException` if the internal query is `null`.

### IsSqlKeyword
- **Purpose**: Determines whether the empty string is considered an SQL keyword (this method takes no arguments and therefore can only evaluate a constant value).
- **Parameters**: None.
- **Return value**: `false`, because the empty string is not an SQL keyword.
- **Exceptions**: None.

## Usage
```csharp
// Example 1: Normalizing a query and extracting identifiers
var normalizer = new QueryNormalizer("SeLeCt  *  From   Orders  Join  Customers on Orders.CustomerId = Customers.Id");
string normalized = normalizer.Normalize;
List<string> tables = normalizer.ExtractTableNames;
List<string> columns = normalizer.ExtractColumnNames;
// normalized => "SELECT * FROM Orders JOIN Customers ON Orders.CustomerId = Customers.Id"
// tables => ["Orders", "Customers"]
// columns => ["*", "Orders.CustomerId", "Customers.Id"]
```

```csharp
// Example 2: Using the static keyword checker
bool isSelect = QueryNormalizer.IsSqlKeyword; // always false
// In practice, you would compare a token against a known list of keywords;
 // this method demonstrates the static API surface.
```

## Notes
- All instance members (`Normalize`, `ExtractTableNames`, `ExtractColumnNames`) depend on the query having been supplied to the `QueryNormalizer` instance prior to invocation. Calling them before initialization results in an `InvalidOperationException`.
- The methods perform case‑insensitive keyword recognition and ignore whitespace and comments when extracting identifiers.
- Instance members are **not thread‑safe**; concurrent calls from multiple threads on the same instance should be synchronized externally.
- The static `IsSqlKeyword` method is thread‑safe as it contains no mutable state and returns a constant value.
