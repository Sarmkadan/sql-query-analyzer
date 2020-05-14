#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using SqlQueryAnalyzer.Utilities;
using Xunit;

namespace SqlQueryAnalyzer.Tests;

public class QueryNormalizerTests
{
    private readonly QueryNormalizer _normalizer = new();

    [Fact]
    public void Normalize_WhitespaceOnlyInput_ReturnsInputUnchanged()
    {
        // Arrange
        var input = "   ";

        // Act
        var result = _normalizer.Normalize(input);

        // Assert
        result.Should().Be(input);
    }

    [Fact]
    public void Normalize_LowercaseSqlKeywords_ConvertsKeywordsToUppercase()
    {
        // Arrange
        var query = "select id from users where id = 1";

        // Act
        var result = _normalizer.Normalize(query);

        // Assert
        result.Should().Contain("SELECT")
            .And.Contain("FROM")
            .And.Contain("WHERE");
    }

    [Fact]
    public void Normalize_StringLiteralInQuery_LiteralCaseIsPreserved()
    {
        // Arrange - 'active user' must survive keyword uppercasing untouched
        var query = "select name from users where status = 'active user'";

        // Act
        var result = _normalizer.Normalize(query);

        // Assert
        result.Should().Contain("'active user'");
        result.Should().Contain("SELECT").And.Contain("FROM");
    }

    [Fact]
    public void ExtractTableNames_QueryWithFromAndJoin_ReturnsBothTableNames()
    {
        // Arrange
        var query = "SELECT u.id FROM users u JOIN orders o ON u.id = o.user_id";

        // Act
        var tables = _normalizer.ExtractTableNames(query);

        // Assert
        tables.Should().Contain("users").And.Contain("orders");
    }
}
