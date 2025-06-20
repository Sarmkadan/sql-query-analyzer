// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using SqlQueryAnalyzer.Utilities;
using Xunit;

namespace SqlQueryAnalyzer.Tests;

public class SqlPatternAnalyzerTests
{
    [Fact]
    public void HasSelectStar_QueryContainsStar_ReturnsTrue()
    {
        // Arrange
        var query = "SELECT * FROM products";

        // Act & Assert
        SqlPatternAnalyzer.HasSelectStar(query).Should().BeTrue();
    }

    [Fact]
    public void HasSelectStar_QueryWithNamedColumns_ReturnsFalse()
    {
        // Arrange
        var query = "SELECT id, name, price FROM products";

        // Act & Assert
        SqlPatternAnalyzer.HasSelectStar(query).Should().BeFalse();
    }

    [Fact]
    public void HasLeadingWildcardLike_PatternStartsWithPercent_ReturnsTrue()
    {
        // Arrange – leading % prevents index range seek on the column
        var query = "SELECT * FROM users WHERE last_name LIKE '%son'";

        // Act & Assert
        SqlPatternAnalyzer.HasLeadingWildcardLike(query).Should().BeTrue();
    }

    [Fact]
    public void DetectNPlusOnePattern_SingleQueryInList_ReturnsFalse()
    {
        // Arrange – minimum two queries required before the detector fires
        var queries = new List<string> { "SELECT * FROM orders WHERE id = 1" };

        // Act & Assert
        SqlPatternAnalyzer.DetectNPlusOnePattern(queries).Should().BeFalse();
    }

    [Fact]
    public void DetectNPlusOnePattern_SameTableAccessedMoreThanFiveTimes_ReturnsTrue()
    {
        // Arrange – six identical per-row look-ups are the classic N+1 symptom
        var queries = Enumerable.Repeat("SELECT * FROM orders WHERE id = 1", 6).ToList();

        // Act & Assert
        SqlPatternAnalyzer.DetectNPlusOnePattern(queries).Should().BeTrue();
    }

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
