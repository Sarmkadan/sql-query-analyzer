#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using SqlQueryAnalyzer.Utilities;
using Xunit;

namespace SqlQueryAnalyzer.Tests;

/// <summary>
/// Provides unit tests for the <see cref="SqlPatternAnalyzer"/> class to verify SQL pattern detection and analysis functionality.
/// </summary>
public class SqlPatternAnalyzerTests
{
    /// <summary>
    /// Tests that the <see cref="SqlPatternAnalyzer.HasSelectStar(string)"/> method correctly identifies queries containing SELECT *.
    /// </summary>
    [Fact]
    public void HasSelectStar_QueryContainsStar_ReturnsTrue()
    {
        // Arrange
        var query = "SELECT * FROM products";

        // Act & Assert
        SqlPatternAnalyzer.HasSelectStar(query).Should().BeTrue();
    }

    /// <summary>
    /// Tests that the <see cref="SqlPatternAnalyzer.HasSelectStar(string)"/> method returns false when the query uses explicit column names instead of SELECT *.
    /// </summary>
    [Fact]
    public void HasSelectStar_QueryWithNamedColumns_ReturnsFalse()
    {
        // Arrange
        var query = "SELECT id, name, price FROM products";

        // Act & Assert
        SqlPatternAnalyzer.HasSelectStar(query).Should().BeFalse();
    }

    /// <summary>
    /// Tests that the <see cref="SqlPatternAnalyzer.HasLeadingWildcardLike(string)"/> method detects LIKE patterns with leading wildcards (e.g., '%son') that prevent index usage.
    /// </summary>
    [Fact]
    public void HasLeadingWildcardLike_PatternStartsWithPercent_ReturnsTrue()
    {
        // Arrange – leading % prevents index range seek on the column
        var query = "SELECT * FROM users WHERE last_name LIKE '%son'";

        // Act & Assert
        SqlPatternAnalyzer.HasLeadingWildcardLike(query).Should().BeTrue();
    }

    /// <summary>
    /// Tests that the <see cref="SqlPatternAnalyzer.DetectNPlusOnePattern(List{string})"/> method returns false when only a single query is provided, as N+1 pattern detection requires at least two queries.
    /// </summary>
    [Fact]
    public void DetectNPlusOnePattern_SingleQueryInList_ReturnsFalse()
    {
        // Arrange – minimum two queries required before the detector fires
        var queries = new List<string> { "SELECT * FROM orders WHERE id = 1" };

        // Act & Assert
        SqlPatternAnalyzer.DetectNPlusOnePattern(queries).Should().BeFalse();
    }

    /// <summary>
    /// Tests that the <see cref="SqlPatternAnalyzer.DetectNPlusOnePattern(List{string})"/> method detects N+1 query patterns when the same table is accessed more than five times.
    /// </summary>
    [Fact]
    public void DetectNPlusOnePattern_SameTableAccessedMoreThanFiveTimes_ReturnsTrue()
    {
        // Arrange – six identical per-row look-ups are the classic N+1 symptom
        var queries = Enumerable.Repeat("SELECT * FROM orders WHERE id = 1", 6).ToList();

        // Act & Assert
        SqlPatternAnalyzer.DetectNPlusOnePattern(queries).Should().BeTrue();
    }

    /// <summary>
    /// Tests that the <see cref="SqlPatternAnalyzer.CalculateReadabilityScore(string)"/> method returns a perfect score of 100 for well-written queries with specific columns, explicit WHERE clauses, no implicit joins, and no wildcards.
    /// </summary>
    [Fact]
    public void CalculateReadabilityScore_WellWrittenQuery_ReturnsFullScore()
    {
        // Arrange – specific columns, explicit WHERE, no implicit joins, no wildcards
        var query = "SELECT id, name FROM users WHERE id = 1";

        // Act
        var score = SqlPatternAnalyzer.CalculateReadabilityScore(query);

        // Assert
        score.Should().Be(100.0);
    }

    /// <summary>
    /// Tests that the <see cref="SqlPatternAnalyzer.CalculateReadabilityScore(string)"/> method deducts 30 points from the readability score when a query uses SELECT * with an implicit JOIN.
    /// </summary>
    [Fact]
    public void CalculateReadabilityScore_SelectStarWithImplicitJoin_DeductsThirtyPoints()
    {
        // Arrange – SELECT * costs 10, implicit JOIN costs 20
        var query = "SELECT * FROM users u, orders o WHERE u.id = o.user_id";

        // Act
        var score = SqlPatternAnalyzer.CalculateReadabilityScore(query);

        // Assert
        score.Should().Be(70.0);
    }

    /// <summary>
    /// Tests that the <see cref="SqlPatternAnalyzer.GenerateOptimizationRecommendations(string)"/> method includes advice to replace SELECT * with specific column names when analyzing a query that uses SELECT *.
    /// </summary>
    [Fact]
    public void GenerateOptimizationRecommendations_SelectStarQuery_IncludesColumnReplacementAdvice()
    {
        // Arrange
        var query = "SELECT * FROM products WHERE price > 10";

        // Act
        var recommendations = SqlPatternAnalyzer.GenerateOptimizationRecommendations(query);

        // Assert
        recommendations.Should().Contain("Replace SELECT * with specific column names");
    }
}
