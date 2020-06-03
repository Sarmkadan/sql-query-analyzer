# SqlPatternAnalyzerTests

The `SqlPatternAnalyzerTests` class contains unit tests for the `SqlPatternAnalyzer` component, which evaluates SQL query patterns for potential performance, readability, and maintainability issues. These tests verify the analyzer's ability to detect common anti-patterns such as `SELECT *`, leading wildcard `LIKE` clauses, and N+1 query patterns, as well as its scoring and recommendation generation capabilities.

## API

### `HasSelectStar_QueryContainsStar_ReturnsTrue`
**Purpose**: Verifies that the analyzer correctly identifies queries containing `SELECT *` as a potential anti-pattern.
**Parameters**: None.
**Return Value**: None (assertion-based test).
**Throws**: Does not throw exceptions under normal operation. Test failures are reported via assertion exceptions.

### `HasSelectStar_QueryWithNamedColumns_ReturnsFalse`
**Purpose**: Ensures the analyzer does not flag queries with explicitly named columns as containing `SELECT *`.
**Parameters**: None.
**Return Value**: None (assertion-based test).
**Throws**: Does not throw exceptions under normal operation.

### `HasLeadingWildcardLike_PatternStartsWithPercent_ReturnsTrue`
**Purpose**: Tests the analyzer's ability to detect leading wildcard `LIKE` clauses (e.g., `LIKE '%value'`), which can prevent index usage.
**Parameters**: None.
**Return Value**: None (assertion-based test).
**Throws**: Does not throw exceptions under normal operation.

### `DetectNPlusOnePattern_SingleQueryInList_ReturnsFalse`
**Purpose**: Confirms the analyzer does not incorrectly identify an N+1 query pattern when only a single query is present.
**Parameters**: None.
**Return Value**: None (assertion-based test).
**Throws**: Does not throw exceptions under normal operation.

### `DetectNPlusOnePattern_SameTableAccessedMoreThanFiveTimes_ReturnsTrue`
**Purpose**: Validates that the analyzer flags an N+1 query pattern when the same table is accessed more than five times in a query batch.
**Parameters**: None.
**Return Value**: None (assertion-based test).
**Throws**: Does not throw exceptions under normal operation.

### `CalculateReadabilityScore_WellWrittenQuery_ReturnsFullScore`
**Purpose**: Ensures the analyzer assigns the maximum readability score to well-structured queries.
**Parameters**: None.
**Return Value**: None (assertion-based test).
**Throws**: Does not throw exceptions under normal operation.

### `CalculateReadabilityScore_SelectStarWithImplicitJoin_DeductsThirtyPoints`
**Purpose**: Tests that the analyzer deducts 30 points from the readability score for queries containing `SELECT *` with implicit joins.
**Parameters**: None.
**Return Value**: None (assertion-based test).
**Throws**: Does not throw exceptions under normal operation.

### `GenerateOptimizationRecommendations_SelectStarQuery_IncludesColumnReplacementAdvice`
**Purpose**: Verifies that the analyzer's optimization recommendations include advice to replace `SELECT *` with explicit column names.
**Parameters**: None.
**Return Value**: None (assertion-based test).
**Throws**: Does not throw exceptions under normal operation.

## Usage

### Example 1: Testing `SELECT *` Detection
