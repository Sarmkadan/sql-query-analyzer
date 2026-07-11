#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

namespace SqlQueryAnalyzer.Utilities;

/// <summary>
/// Provides validation helpers for the <see cref="SqlPatternAnalyzer"/> static class.
/// Validates all public static methods to ensure they work correctly and return
/// appropriate values for SQL pattern analysis.
/// </summary>
public static class SqlPatternAnalyzerValidation
{
    /// <summary>
    /// Validates the <see cref="SqlPatternAnalyzer"/> static class methods and returns a list of human-readable problems.
    /// </summary>
    /// <returns>An empty list if valid, otherwise a list of validation errors</returns>
    public static IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();

        // Test queries for validation
        const string simpleQuery = "SELECT id, name FROM users WHERE active = 1";
        const string queryWithStar = "SELECT * FROM users";
        const string queryWithWhere = "SELECT id FROM orders WHERE customer_id = 1";
        const string queryWithOr = "SELECT * FROM users WHERE name = 'John' OR name = 'Jane'";
        const string queryWithDistinct = "SELECT DISTINCT name FROM users";
        const string queryWithOrderBy = "SELECT DISTINCT name FROM users ORDER BY name";
        const string queryWithLike = "LIKE '%test%'";
        const string queryWithFunction = "SELECT UPPER(name) FROM users WHERE id = 1";
        const string queryWithImplicitJoin = "SELECT * FROM users, orders";
        const string queryWithSubquery = "SELECT * FROM (SELECT id FROM users) AS sub";
        const string queryWithUnion = "SELECT id FROM table1 UNION SELECT id FROM table2";
        const string queryWithCase = "SELECT CASE WHEN active THEN 'Y' ELSE 'N' END FROM users";
        const string queryWithAggregate = "SELECT COUNT(*) FROM users";
        const string queryWithWindow = "SELECT ROW_NUMBER() OVER (PARTITION BY dept) FROM employees";
        const string queryWithCte = "WITH cte AS (SELECT id FROM users) SELECT * FROM cte";
        const string queryWithJoin = "SELECT * FROM users INNER JOIN orders ON users.id = orders.user_id";
        const string queryWithWhereClause = "SELECT id FROM users WHERE name = 'John'";

        var testQueries = new List<string> { simpleQuery, queryWithStar, queryWithWhere };

        // Validate DetectNPlusOnePattern
        try
        {
            var result = SqlPatternAnalyzer.DetectNPlusOnePattern(testQueries);
            // Method should return a boolean without throwing
        }
        catch (Exception ex)
        {
            errors.Add($"DetectNPlusOnePattern threw exception: {ex.GetType().Name}: {ex.Message}");
        }

        // Validate HasMissingWhereClause
        try
        {
            var result1 = SqlPatternAnalyzer.HasMissingWhereClause(simpleQuery);
            var result2 = SqlPatternAnalyzer.HasMissingWhereClause(queryWithWhereClause);
            if (result1 == result2)
                errors.Add("HasMissingWhereClause may not be working correctly - both queries returned same result");
        }
        catch (Exception ex)
        {
            errors.Add($"HasMissingWhereClause threw exception: {ex.GetType().Name}: {ex.Message}");
        }

        // Validate HasSelectStar
        try
        {
            var result = SqlPatternAnalyzer.HasSelectStar(queryWithStar);
            if (!result)
                errors.Add("HasSelectStar should return true for query with SELECT *");
        }
        catch (Exception ex)
        {
            errors.Add($"HasSelectStar threw exception: {ex.GetType().Name}: {ex.Message}");
        }

        // Validate HasLeadingWildcardLike
        try
        {
            var result = SqlPatternAnalyzer.HasLeadingWildcardLike(queryWithLike);
            if (!result)
                errors.Add("HasLeadingWildcardLike should detect leading wildcard LIKE patterns");
        }
        catch (Exception ex)
        {
            errors.Add($"HasLeadingWildcardLike threw exception: {ex.GetType().Name}: {ex.Message}");
        }

        // Validate HasFunctionOnColumn
        try
        {
            var result = SqlPatternAnalyzer.HasFunctionOnColumn(queryWithFunction);
            if (!result)
                errors.Add("HasFunctionOnColumn should detect functions on columns");
        }
        catch (Exception ex)
        {
            errors.Add($"HasFunctionOnColumn threw exception: {ex.GetType().Name}: {ex.Message}");
        }

        // Validate HasImplicitJoin
        try
        {
            var result = SqlPatternAnalyzer.HasImplicitJoin(queryWithImplicitJoin);
            if (!result)
                errors.Add("HasImplicitJoin should detect implicit JOIN patterns");
        }
        catch (Exception ex)
        {
            errors.Add($"HasImplicitJoin threw exception: {ex.GetType().Name}: {ex.Message}");
        }

        // Validate HasDistinctWithoutOrder
        try
        {
            var result1 = SqlPatternAnalyzer.HasDistinctWithoutOrder(queryWithDistinct);
            var result2 = SqlPatternAnalyzer.HasDistinctWithoutOrder(queryWithOrderBy);
            if (!result1)
                errors.Add("HasDistinctWithoutOrder should return true for DISTINCT without ORDER BY");
            if (result2)
                errors.Add("HasDistinctWithoutOrder should return false for DISTINCT with ORDER BY");
        }
        catch (Exception ex)
        {
            errors.Add($"HasDistinctWithoutOrder threw exception: {ex.GetType().Name}: {ex.Message}");
        }

        // Validate CountOrConditions
        try
        {
            var result = SqlPatternAnalyzer.CountOrConditions(queryWithOr);
            if (result <= 0)
                errors.Add("CountOrConditions should return positive count for query with OR conditions");
        }
        catch (Exception ex)
        {
            errors.Add($"CountOrConditions threw exception: {ex.GetType().Name}: {ex.Message}");
        }

        // Validate HasSubquery
        try
        {
            var result = SqlPatternAnalyzer.HasSubquery(queryWithSubquery);
            if (!result)
                errors.Add("HasSubquery should detect subquery patterns");
        }
        catch (Exception ex)
        {
            errors.Add($"HasSubquery threw exception: {ex.GetType().Name}: {ex.Message}");
        }

        // Validate CountUnion
        try
        {
            var result = SqlPatternAnalyzer.CountUnion(queryWithUnion);
            if (result != 1)
                errors.Add("CountUnion should return 1 for single UNION");
        }
        catch (Exception ex)
        {
            errors.Add($"CountUnion threw exception: {ex.GetType().Name}: {ex.Message}");
        }

        // Validate CountCaseStatements
        try
        {
            var result = SqlPatternAnalyzer.CountCaseStatements(queryWithCase);
            if (result != 1)
                errors.Add("CountCaseStatements should return 1 for single CASE statement");
        }
        catch (Exception ex)
        {
            errors.Add($"CountCaseStatements threw exception: {ex.GetType().Name}: {ex.Message}");
        }

        // Validate HasAggregateFunction
        try
        {
            var result = SqlPatternAnalyzer.HasAggregateFunction(queryWithAggregate);
            if (!result)
                errors.Add("HasAggregateFunction should detect aggregate functions");
        }
        catch (Exception ex)
        {
            errors.Add($"HasAggregateFunction threw exception: {ex.GetType().Name}: {ex.Message}");
        }

        // Validate HasWindowFunction
        try
        {
            var result = SqlPatternAnalyzer.HasWindowFunction(queryWithWindow);
            if (!result)
                errors.Add("HasWindowFunction should detect window functions");
        }
        catch (Exception ex)
        {
            errors.Add($"HasWindowFunction threw exception: {ex.GetType().Name}: {ex.Message}");
        }

        // Validate CalculateReadabilityScore
        try
        {
            var score = SqlPatternAnalyzer.CalculateReadabilityScore(simpleQuery);
            if (score < 0 || score > 100)
                errors.Add($"CalculateReadabilityScore returned out-of-range value: {score}");
        }
        catch (Exception ex)
        {
            errors.Add($"CalculateReadabilityScore threw exception: {ex.GetType().Name}: {ex.Message}");
        }

        // Validate CountParentheses
        try
        {
            var count = SqlPatternAnalyzer.CountParentheses("(SELECT (id) FROM (table))");
            if (count != 2)
                errors.Add("CountParentheses should return correct nesting depth");
        }
        catch (Exception ex)
        {
            errors.Add($"CountParentheses threw exception: {ex.GetType().Name}: {ex.Message}");
        }

        // Validate ExtractWhereClause
        try
        {
            var whereClause = SqlPatternAnalyzer.ExtractWhereClause(queryWithWhereClause);
            if (string.IsNullOrEmpty(whereClause))
                errors.Add("ExtractWhereClause should extract WHERE clause correctly");
        }
        catch (Exception ex)
        {
            errors.Add($"ExtractWhereClause threw exception: {ex.GetType().Name}: {ex.Message}");
        }

        // Validate ExtractCteNames
        try
        {
            var cteNames = SqlPatternAnalyzer.ExtractCteNames(queryWithCte);
            if (cteNames == null || cteNames.Count == 0)
                errors.Add("ExtractCteNames should extract CTE names correctly");
        }
        catch (Exception ex)
        {
            errors.Add($"ExtractCteNames threw exception: {ex.GetType().Name}: {ex.Message}");
        }

        // Validate ExtractTablesFromQuery
        try
        {
            var tables = SqlPatternAnalyzer.ExtractTablesFromQuery(simpleQuery);
            if (tables == null || tables.Count == 0)
                errors.Add("ExtractTablesFromQuery should extract table names correctly");
        }
        catch (Exception ex)
        {
            errors.Add($"ExtractTablesFromQuery threw exception: {ex.GetType().Name}: {ex.Message}");
        }

        // Validate ExtractJoinConditions
        try
        {
            var joinConditions = SqlPatternAnalyzer.ExtractJoinConditions(queryWithJoin);
            if (joinConditions == null)
                errors.Add("ExtractJoinConditions returned null");
        }
        catch (Exception ex)
        {
            errors.Add($"ExtractJoinConditions threw exception: {ex.GetType().Name}: {ex.Message}");
        }

        // Validate GenerateOptimizationRecommendations
        try
        {
            var recommendations = SqlPatternAnalyzer.GenerateOptimizationRecommendations(queryWithStar);
            if (recommendations == null)
                errors.Add("GenerateOptimizationRecommendations returned null");
            else if (recommendations.Count == 0)
                errors.Add("GenerateOptimizationRecommendations should return recommendations for problematic query");
        }
        catch (Exception ex)
        {
            errors.Add($"GenerateOptimizationRecommendations threw exception: {ex.GetType().Name}: {ex.Message}");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the <see cref="SqlPatternAnalyzer"/> static class is valid.
    /// </summary>
    /// <returns>True if valid; otherwise, false</returns>
    public static bool IsValid()
    {
        return Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the <see cref="SqlPatternAnalyzer"/> static class is valid,
    /// throwing an <see cref="InvalidOperationException"/> with detailed validation errors if not.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when validation fails with a list of problems</exception>
    public static void EnsureValid()
    {
        var errors = Validate();

        if (errors.Count == 0)
            return;

        throw new InvalidOperationException(
            $"SqlPatternAnalyzer validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", errors)}");
    }
}