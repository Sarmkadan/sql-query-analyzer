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
/// Tests for validation methods in QueryNormalizerValidation.
/// Ensures safe operations with null/empty inputs.
/// </summary>
public class QueryNormalizerValidationTests
{
    private readonly QueryNormalizer _normalizer = new();

    [Fact]
    public void TryNormalize_WithNullInput_ReturnsFalseAndNullOutput()
    {
        // Arrange
        string? query = null;
        string? normalizedQuery = "dummy";

        // Act
        var result = QueryNormalizerValidation.TryNormalize(_normalizer, query, out normalizedQuery);

        // Assert
        result.Should().BeFalse();
        normalizedQuery.Should().BeNull();
    }

    [Fact]
    public void TryNormalize_WithEmptyInput_ReturnsFalseAndNullOutput()
    {
        // Arrange
        var query = "";
        string? normalizedQuery = "dummy";

        // Act
        var result = QueryNormalizerValidation.TryNormalize(_normalizer, query, out normalizedQuery);

        // Assert
        result.Should().BeFalse();
        normalizedQuery.Should().BeNull();
    }

    [Fact]
    public void TryNormalize_WithWhitespaceInput_ReturnsFalseAndNullOutput()
    {
        // Arrange
        var query = "   \t\n  ";
        string? normalizedQuery = "dummy";

        // Act
        var result = QueryNormalizerValidation.TryNormalize(_normalizer, query, out normalizedQuery);

        // Assert
        result.Should().BeFalse();
        normalizedQuery.Should().BeNull();
    }

    [Fact]
    public void TryNormalize_WithValidInput_ReturnsTrueAndNormalizedQuery()
    {
        // Arrange
        var query = "select id from users where id = 1";
        string? normalizedQuery = null;

        // Act
        var result = QueryNormalizerValidation.TryNormalize(_normalizer, query, out normalizedQuery);

        // Assert
        result.Should().BeTrue();
        normalizedQuery.Should().NotBeNull();
        normalizedQuery.Should().Be("select id from users where id = 1");
    }

    [Fact]
    public void TryToParameterizedQuery_WithNullInput_ReturnsFalseAndNullOutput()
    {
        // Arrange
        string? query = null;
        string? parameterizedQuery = "dummy";

        // Act
        var result = QueryNormalizerValidation.TryToParameterizedQuery(_normalizer, query, out parameterizedQuery);

        // Assert
        result.Should().BeFalse();
        parameterizedQuery.Should().BeNull();
    }

    [Fact]
    public void TryToParameterizedQuery_WithEmptyInput_ReturnsFalseAndNullOutput()
    {
        // Arrange
        var query = "";
        string? parameterizedQuery = "dummy";

        // Act
        var result = QueryNormalizerValidation.TryToParameterizedQuery(_normalizer, query, out parameterizedQuery);

        // Assert
        result.Should().BeFalse();
        parameterizedQuery.Should().BeNull();
    }

    [Fact]
    public void TryToParameterizedQuery_WithValidInput_ReturnsTrueAndParameterizedQuery()
    {
        // Arrange
        var query = "select id from users where id = 1";
        string? parameterizedQuery = null;

        // Act
        var result = QueryNormalizerValidation.TryToParameterizedQuery(_normalizer, query, out parameterizedQuery);

        // Assert
        result.Should().BeTrue();
        parameterizedQuery.Should().NotBeNull();
        parameterizedQuery.Should().Be("select id from users where id = ?");
    }

    [Fact]
    public void TryExtractTableNames_WithNullInput_ReturnsFalseAndNullOutput()
    {
        // Arrange
        string? query = null;
        string[]? tableNames = ["dummy"];

        // Act
        var result = QueryNormalizerValidation.TryExtractTableNames(_normalizer, query, out tableNames);

        // Assert
        result.Should().BeFalse();
        tableNames.Should().BeNull();
    }

    [Fact]
    public void TryExtractTableNames_WithEmptyInput_ReturnsFalseAndNullOutput()
    {
        // Arrange
        var query = "";
        string[]? tableNames = ["dummy"];

        // Act
        var result = QueryNormalizerValidation.TryExtractTableNames(_normalizer, query, out tableNames);

        // Assert
        result.Should().BeFalse();
        tableNames.Should().BeNull();
    }

    [Fact]
    public void TryExtractTableNames_WithValidInput_ReturnsTrueAndTableNames()
    {
        // Arrange
        var query = "select id from users where id = 1";
        string[]? tableNames = null;

        // Act
        var result = QueryNormalizerValidation.TryExtractTableNames(_normalizer, query, out tableNames);

        // Assert
        result.Should().BeTrue();
        tableNames.Should().NotBeNull();
        tableNames.Should().Contain("users");
    }

    [Fact]
    public void TryExtractColumnNames_WithNullInput_ReturnsFalseAndNullOutput()
    {
        // Arrange
        string? query = null;
        string[]? columnNames = ["dummy"];

        // Act
        var result = QueryNormalizerValidation.TryExtractColumnNames(_normalizer, query, out columnNames);

        // Assert
        result.Should().BeFalse();
        columnNames.Should().BeNull();
    }

    [Fact]
    public void TryExtractColumnNames_WithEmptyInput_ReturnsFalseAndNullOutput()
    {
        // Arrange
        var query = "";
        string[]? columnNames = ["dummy"];

        // Act
        var result = QueryNormalizerValidation.TryExtractColumnNames(_normalizer, query, out columnNames);

        // Assert
        result.Should().BeFalse();
        columnNames.Should().BeNull();
    }

    [Fact]
    public void TryExtractColumnNames_WithValidInput_ReturnsTrueAndColumnNames()
    {
        // Arrange
        var query = "select id, name from users";
        string[]? columnNames = null;

        // Act
        var result = QueryNormalizerValidation.TryExtractColumnNames(_normalizer, query, out columnNames);

        // Assert
        result.Should().BeTrue();
        columnNames.Should().NotBeNull();
        columnNames.Should().Contain("id").And.Contain("name");
    }
}
