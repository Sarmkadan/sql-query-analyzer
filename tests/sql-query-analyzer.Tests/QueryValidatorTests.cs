// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SqlQueryAnalyzer.Exceptions;
using SqlQueryAnalyzer.Models;
using SqlQueryAnalyzer.Utilities;
using SqlQueryAnalyzer.Validation;
using Xunit;

namespace SqlQueryAnalyzer.Tests;

public class QueryValidatorTests
{
    // -------------------------------------------------------------------------
    // QueryValidator – static helpers
    // -------------------------------------------------------------------------

    [Fact]
    public void IsValidQuery_WellFormedSelectStatement_ReturnsTrue()
    {
        QueryValidator.IsValidQuery("SELECT id, name FROM users WHERE id = 1")
            .Should().BeTrue();
    }

    [Fact]
    public void IsValidQuery_EmptyString_ReturnsFalse()
    {
        QueryValidator.IsValidQuery(string.Empty).Should().BeFalse();
    }

    [Fact]
    public void IsValidQuery_TextWithNoRecognisedSqlKeyword_ReturnsFalse()
    {
        // "SHOW" is not in the accepted keyword list
        QueryValidator.IsValidQuery("SHOW TABLES").Should().BeFalse();
    }

    [Fact]
    public void ValidateDatabaseQuery_NullArgument_ThrowsValidationException()
    {
        // Arrange
        Action act = () => QueryValidator.ValidateDatabaseQuery(null!);

        // Act & Assert
        act.Should().Throw<ValidationException>()
            .WithMessage("*cannot be null*");
    }

    [Fact]
    public void SanitizeQueryForDisplay_QueryLongerThanMaxLength_TruncatesWithEllipsis()
    {
        // Arrange
        var longQuery = new string('A', 200);

        // Act
        var result = QueryValidator.SanitizeQueryForDisplay(longQuery, maxLength: 50);

        // Assert
        result.Should().HaveLength(50).And.EndWith("...");
    }

    // -------------------------------------------------------------------------
    // QueryCacheKeyGenerator – deterministic key generation
    // -------------------------------------------------------------------------

    [Fact]
    public void GenerateQueryKey_SameQueryWithVariableWhitespace_ProducesSameKey()
    {
        // Arrange – extra spaces are normalised before hashing
        var generator = new QueryCacheKeyGenerator();
        var compact = "SELECT id FROM users";
        var padded = "SELECT  id  FROM  users";

        // Act
        var key1 = generator.GenerateQueryKey(compact);
        var key2 = generator.GenerateQueryKey(padded);

        // Assert
        key1.Should().Be(key2);
    }

    [Fact]
    public void GenerateResultKey_ValidQuery_KeyHasResultPrefix()
    {
        // Arrange
        var generator = new QueryCacheKeyGenerator();

        // Act
        var key = generator.GenerateResultKey("SELECT 1");

        // Assert
        key.Should().StartWith("sqlanalyzer:result:");
    }

    // -------------------------------------------------------------------------
    // ValidationRuleEngine – Moq-based interaction test
    // -------------------------------------------------------------------------

    [Fact]
    public void ValidateQuery_CustomRuleRegistered_RuleIsInvokedExactlyOnce()
    {
        // Arrange
        var logger = new Mock<ILogger<ValidationRuleEngine>>();
        var engine = new ValidationRuleEngine(logger.Object);

        var mockRule = new Mock<IValidationRule>();
        mockRule.Setup(r => r.Name).Returns("CustomTestRule");
        mockRule.Setup(r => r.Validate(It.IsAny<string>()))
            .Returns(new RuleValidationResult { IsValid = true });

        engine.RegisterRule(mockRule.Object);

        // Act
        engine.ValidateQuery("SELECT id FROM users WHERE id = 1");

        // Assert – the engine must delegate to every registered rule
        mockRule.Verify(r => r.Validate("SELECT id FROM users WHERE id = 1"), Times.Once);
    }
}
