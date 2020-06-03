# QueryNormalizerTests

Unit tests for the SQL query normalization logic in the `sql-query-analyzer` project. These tests verify that the `QueryNormalizer` class correctly handles whitespace normalization, keyword casing, string literal preservation, and table name extraction from SQL queries.

## API

### `Normalize_WhitespaceOnlyInput_ReturnsInputUnchanged()`
Verifies that a query containing only whitespace characters is returned unchanged. This test ensures the normalizer does not alter queries that do not require normalization.

- **Parameters**: None
- **Return value**: Void
- **Throws**: Does not throw exceptions under normal conditions

### `Normalize_LowercaseSqlKeywords_ConvertsKeywordsToUppercase()`
Ensures that SQL keywords in lowercase are converted to uppercase during normalization. This maintains consistent keyword casing in the output.

- **Parameters**: None
- **Return value**: Void
- **Throws**: Does not throw exceptions under normal conditions

### `Normalize_StringLiteralInQuery_LiteralCaseIsPreserved()`
Confirms that string literals within a query retain their original case during normalization. This prevents unintended modification of data embedded in queries.

- **Parameters**: None
- **Return value**: Void
- **Throws**: Does not throw exceptions under normal conditions

### `ExtractTableNames_QueryWithFromAndJoin_ReturnsBothTableNames()`
Validates that table names are correctly extracted from a query containing both `FROM` and `JOIN` clauses. This test ensures the extractor handles multi-table queries properly.

- **Parameters**: None
- **Return value**: Void
- **Throws**: Does not throw exceptions under normal conditions

## Usage
