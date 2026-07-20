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
        var input = " ";

        // Act
        var result = _normalizer.Normalize(input);

        // Assert
        result.Should().Be(input);
    }

    [Fact]
    public void Normalize_LowercaseSqlKeywords_ConvertsKeywordsToLowercase()
    {
        // Arrange
        var query = "select id from users where id = 1";

        // Act
        var result = _normalizer.Normalize(query);

        // Assert
        result.Should().Contain("select")
        .And.Contain("from")
        .And.Contain("where");
    }

    [Fact]
    public void Normalize_MixedCaseSqlKeywords_ConvertsToLowercase()
    {
        // Arrange
        var query = "SeLeCt id FrOm users WhErE id = 1";

        // Act
        var result = _normalizer.Normalize(query);

        // Assert
        result.Should().Contain("select")
        .And.Contain("from")
        .And.Contain("where");
    }

    [Fact]
    public void Normalize_StringLiteralInQuery_LiteralCaseIsPreserved()
    {
        // Arrange - 'active user' must survive keyword lowercasing untouched
        var query = "select name from users where status = 'active user'";

        // Act
        var result = _normalizer.Normalize(query);

        // Assert
        result.Should().Contain("'active user'");
        result.Should().Contain("select").And.Contain("from");
    }

    [Fact]
    public void Normalize_StringLiteralWithEscapedQuotes_PreservesEscapedQuotes()
    {
        // Arrange
        var query = "select name from users where status = 'O'Reilly' AND active = true";

        // Act
        var result = _normalizer.Normalize(query);

        // Assert
        result.Should().Contain("'O'Reilly'");
    }

    [Fact]
    public void Normalize_RemovesLineComments()
    {
        // Arrange
        var query = "select id from users -- this is a comment\nwhere id = 1";

        // Act
        var result = _normalizer.Normalize(query);

        // Assert
        result.Should().NotContain("--");
        result.Should().Contain("select")
            .And.Contain("from")
            .And.Contain("where");
    }

    [Fact]
    public void Normalize_RemovesBlockComments()
    {
        // Arrange
        var query = "select id /* multi-line\n   comment */ from users where id = 1";

        // Act
        var result = _normalizer.Normalize(query);

        // Assert
        result.Should().NotContain("/*");
        result.Should().NotContain("*/");
        result.Should().Contain("select").And.Contain("from");
    }

    [Fact]
    public void Normalize_CollapsesWhitespace()
    {
        // Arrange
        var query = "select    id   from    users\t\twhere\n\nid = 1";

        // Act
        var result = _normalizer.Normalize(query);

        // Assert
        result.Should().NotContain("    "); // Multiple spaces
        result.Should().NotContain("\t\t"); // Multiple tabs
        result.Should().NotContain("\n\n"); // Multiple newlines
        result.Should().Contain(" "); // Single spaces should remain
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

    [Fact]
    public void ExtractTableNames_QueryWithInto_ReturnsTableName()
    {
        // Arrange
        var query = "INSERT INTO users (id, name) VALUES (1, 'test')";

        // Act
        var tables = _normalizer.ExtractTableNames(query);

        // Assert
        tables.Should().Contain("users");
    }

    [Fact]
    public void ExtractColumnNames_QueryWithSelectColumns_ReturnsColumnNames()
    {
        // Arrange
        var query = "SELECT id, name as user_name, email FROM users";

        // Act
        var columns = _normalizer.ExtractColumnNames(query);

        // Assert
        columns.Should().Contain("id")
            .And.Contain("name")
            .And.Contain("email");
    }

    [Fact]
    public void ExtractColumnNames_QueryWithStar_ReturnsAsterisk()
    {
        // Arrange
        var query = "SELECT * FROM users";

        // Act
        var columns = _normalizer.ExtractColumnNames(query);

        // Assert
        columns.Should().Contain("*");
    }

    [Fact]
    public void IsSqlKeyword_RecognizesKeywords()
    {
        // Arrange & Act & Assert
        QueryNormalizer.IsSqlKeyword("select").Should().BeTrue();
        QueryNormalizer.IsSqlKeyword("from").Should().BeTrue();
        QueryNormalizer.IsSqlKeyword("where").Should().BeTrue();
        QueryNormalizer.IsSqlKeyword("and").Should().BeTrue();
        QueryNormalizer.IsSqlKeyword("join").Should().BeTrue();
    }

    [Fact]
    public void IsSqlKeyword_NotAKeyword_ReturnsFalse()
    {
        // Arrange & Act & Assert
        QueryNormalizer.IsSqlKeyword("users").Should().BeFalse();
        QueryNormalizer.IsSqlKeyword("id").Should().BeFalse();
        QueryNormalizer.IsSqlKeyword("test").Should().BeFalse();
    }
}
