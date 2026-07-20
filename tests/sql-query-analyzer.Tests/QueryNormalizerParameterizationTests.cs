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
/// Tests for SQL query parameterization functionality in QueryNormalizer.
/// Ensures that numeric and string literals are replaced with ? placeholders.
/// </summary>
public class QueryNormalizerParameterizationTests
{
    private readonly QueryNormalizer _normalizer = new();

    [Fact]
    public void ToParameterizedQuery_EmptyInput_ReturnsEmpty()
    {
        // Arrange
        var query = "";

        // Act
        var result = _normalizer.ToParameterizedQuery(query);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ToParameterizedQuery_WhitespaceOnlyInput_ReturnsEmpty()
    {
        // Arrange
        var query = "   \t\n  ";

        // Act
        var result = _normalizer.ToParameterizedQuery(query);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ToParameterizedQuery_SimpleQueryWithNumericLiteral_ReplacesWithPlaceholder()
    {
        // Arrange
        var query = "select id from users where id = 1";

        // Act
        var result = _normalizer.ToParameterizedQuery(query);

        // Assert
        result.Should().Be("select id from users where id = ?");
    }

    [Fact]
    public void ToParameterizedQuery_SimpleQueryWithMultipleNumericLiterals_ReplacesAll()
    {
        // Arrange
        var query = "select id from users where id = 1 and age > 25";

        // Act
        var result = _normalizer.ToParameterizedQuery(query);

        // Assert
        result.Should().Be("select id from users where id = ? and age > ?");
    }

    [Fact]
    public void ToParameterizedQuery_QueryWithStringLiteral_ReplacesWithPlaceholder()
    {
        // Arrange
        var query = "select name from users where status = 'active'";

        // Act
        var result = _normalizer.ToParameterizedQuery(query);

        // Assert
        result.Should().Be("select name from users where status = ?");
    }

    [Fact]
    public void ToParameterizedQuery_QueryWithMultipleStringLiterals_ReplacesAll()
    {
        // Arrange
        var query = "select name from users where status = 'active' and role = 'admin'";

        // Act
        var result = _normalizer.ToParameterizedQuery(query);

        // Assert
        result.Should().Be("select name from users where status = ? and role = ?");
    }

    [Fact]
    public void ToParameterizedQuery_QueryWithMixedLiterals_ReplacesAll()
    {
        // Arrange
        var query = "select id, name from users where id = 1 and status = 'active' and age > 25";

        // Act
        var result = _normalizer.ToParameterizedQuery(query);

        // Assert
        result.Should().Be("select id, name from users where id = ? and status = ? and age > ?");
    }

    [Fact]
    public void ToParameterizedQuery_QueryWithDecimalLiteral_ReplacesWithPlaceholder()
    {
        // Arrange
        var query = "select price from products where price = 19.99";

        // Act
        var result = _normalizer.ToParameterizedQuery(query);

        // Assert
        result.Should().Be("select price from products where price = ?");
    }

    [Fact]
    public void ToParameterizedQuery_QueryWithNegativeNumericLiteral_ReplacesWithPlaceholder()
    {
        // Arrange
        var query = "select balance from accounts where balance < -1000";

        // Act
        var result = _normalizer.ToParameterizedQuery(query);

        // Assert
        result.Should().Be("select balance from accounts where balance < ?");
    }

    [Fact]
    public void ToParameterizedQuery_QueryWithScientificNotation_ReplacesWithPlaceholder()
    {
        // Arrange
        var query = "select value from data where value = 1.5e-10";

        // Act
        var result = _normalizer.ToParameterizedQuery(query);

        // Assert
        result.Should().Be("select value from data where value = ?");
    }

    [Fact]
    public void ToParameterizedQuery_QueryWithStringLiteralContainingQuotes_PreservesEscapedQuotes()
    {
        // Arrange
        var query = "select name from users where status = 'O'Reilly'";

        // Act
        var result = _normalizer.ToParameterizedQuery(query);

        // Assert
        result.Should().Be("select name from users where status = ?");
    }

    [Fact]
    public void ToParameterizedQuery_QueryWithEscapedSingleQuotesInLiteral_ReplacesEntireLiteral()
    {
        // Arrange
        var query = "select name from users where status = 'can''t connect'";

        // Act
        var result = _normalizer.ToParameterizedQuery(query);

        // Assert
        result.Should().Be("select name from users where status = ?");
    }

    [Fact]
    public void ToParameterizedQuery_QueryWithBlockComments_CommentsRemoved()
    {
        // Arrange
        var query = "select id /* comment */ from users where id = 1";

        // Act
        var result = _normalizer.ToParameterizedQuery(query);

        // Assert
        result.Should().Be("select id from users where id = ?");
    }

    [Fact]
    public void ToParameterizedQuery_QueryWithLineComments_CommentsRemoved()
    {
        // Arrange
        var query = "select id from users -- get active users\nwhere id = 1";

        // Act
        var result = _normalizer.ToParameterizedQuery(query);

        // Assert
        result.Should().Be("select id from users where id = ?");
    }

    [Fact]
    public void ToParameterizedQuery_QueryWithWhitespace_CollapsesWhitespace()
    {
        // Arrange
        var query = "select    id   from    users\t\twhere\n\nid = 1";

        // Act
        var result = _normalizer.ToParameterizedQuery(query);

        // Assert
        result.Should().Be("select id from users where id = ?");
    }

    [Fact]
    public void ToParameterizedQuery_QueryWithKeywordsInMixedCase_NormalizesToLowercase()
    {
        // Arrange
        var query = "SeLeCt id FrOm users WhErE id = 1";

        // Act
        var result = _normalizer.ToParameterizedQuery(query);

        // Assert
        result.Should().Be("select id from users where id = ?");
    }

    [Fact]
    public void ToParameterizedQuery_ComplexQueryWithAllFeatures_ParameterizesCorrectly()
    {
        // Arrange
        var query = "SELECT u.id, u.name FROM users u WHERE u.status = 'active' AND u.age > 25 ORDER BY u.name LIMIT 10";

        // Act
        var result = _normalizer.ToParameterizedQuery(query);

        // Assert
        result.Should().Be("select u.id, u.name from users u where u.status = ? and u.age > ? order by u.name");
    }

    [Fact]
    public void ToParameterizedQueryAndTrim_ExtensionMethod_TrimsResult()
    {
        // Arrange
        var query = "   select id from users where id = 1   ";

        // Act
        var result = QueryNormalizerExtensions.ToParameterizedQueryAndTrim(_normalizer, query);

        // Assert
        result.Should().Be("select id from users where id = ?");
    }

    [Fact]
    public void ToParameterizedQueryAndTrim_WithNullInput_ReturnsEmpty()
    {
        // Arrange
        string? query = null;

        // Act
        var result = QueryNormalizerExtensions.ToParameterizedQueryAndTrim(_normalizer, query);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ToParameterizedQuery_QueryWithoutLiterals_ReturnsNormalizedQuery()
    {
        // Arrange
        var query = "select id, name from users";

        // Act
        var result = _normalizer.ToParameterizedQuery(query);

        // Assert
        result.Should().Be("select id, name from users");
    }

    [Fact]
    public void ToParameterizedQuery_QueryWithZeroLiteral_ReplacesWithPlaceholder()
    {
        // Arrange
        var query = "select count(*) from users where active = 0";

        // Act
        var result = _normalizer.ToParameterizedQuery(query);

        // Assert
        result.Should().Be("select count(*) from users where active = ?");
    }

    [Fact]
    public void ToParameterizedQuery_QueryWithBooleanLiterals_PreservesBooleanValues()
    {
        // Arrange
        var query = "select active from users where active = true and deleted = false";

        // Act
        var result = _normalizer.ToParameterizedQuery(query);

        // Assert - boolean literals (true/false) are not numeric literals, so they're preserved
        result.Should().Be("select active from users where active = true and deleted = false");
    }
}
