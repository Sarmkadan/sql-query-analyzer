#nullable enable
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

/// <summary>
/// Contains unit tests for the <see cref="QueryValidator"/> and related validation components.
/// Tests the correctness and edge cases of SQL query validation, caching key generation,
/// and rule-based validation engine.
/// </summary>
public class QueryValidatorTests
{
	// -------------------------------------------------------------------------
	// QueryValidator – static helpers
	// -------------------------------------------------------------------------

	/// <summary>
	/// Tests the <see cref="QueryValidator.IsValidQuery(string)"/> method with a well-formed SELECT statement.
	/// Verifies that valid SQL queries are correctly identified as valid.
	/// </summary>
	[Fact]
	public void IsValidQuery_WellFormedSelectStatement_ReturnsTrue()
	{
		QueryValidator.IsValidQuery("SELECT id, name FROM users WHERE id = 1")
			.Should().BeTrue();
	}

	/// <summary>
	/// Tests the <see cref="QueryValidator.IsValidQuery(string)"/> method with an empty string input.
	/// Verifies that empty strings are correctly identified as invalid queries.
	/// </summary>
	[Fact]
	public void IsValidQuery_EmptyString_ReturnsFalse()
	{
		QueryValidator.IsValidQuery(string.Empty).Should().BeFalse();
	}

	/// <summary>
	/// Tests the <see cref="QueryValidator.IsValidQuery(string)"/> method with a statement containing no recognized SQL keywords.
	/// Verifies that queries without recognized keywords are correctly identified as invalid.
	/// </summary>
	[Fact]
	public void IsValidQuery_TextWithNoRecognisedSqlKeyword_ReturnsFalse()
	{
		// "SHOW" is not in the accepted keyword list
		QueryValidator.IsValidQuery("SHOW TABLES").Should().BeFalse();
	}

	/// <summary>
	/// Tests the <see cref="QueryValidator.ValidateDatabaseQuery(string)"/> method with a null argument.
	/// Verifies that null queries throw a <see cref="ValidationException"/> with an appropriate error message.
	/// </summary>
	[Fact]
	public void ValidateDatabaseQuery_NullArgument_ThrowsValidationException()
	{
		// Arrange
		Action act = () => QueryValidator.ValidateDatabaseQuery(null!);

		// Act & Assert
		act.Should().Throw<ValidationException>()
			.WithMessage("*cannot be null*");
	}

	/// <summary>
	/// Tests the <see cref="QueryValidator.SanitizeQueryForDisplay(string, int)"/> method with a query longer than the maximum allowed length.
	/// Verifies that long queries are truncated and suffixed with an ellipsis to fit within the specified maximum length.
	/// </summary>
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

	/// <summary>
	/// Tests the <see cref="QueryCacheKeyGenerator.GenerateQueryKey(string)"/> method with queries having variable whitespace.
	/// Verifies that queries with different whitespace patterns produce identical cache keys due to normalization.
	/// </summary>
	[Fact]
	public void GenerateQueryKey_SameQueryWithVariableWhitespace_ProducesSameKey()
	{
		// Arrange – extra spaces are normalised before hashing
		var generator = new QueryCacheKeyGenerator();
		var compact = "SELECT id FROM users";
		var padded = "SELECT id FROM users";

		// Act
		var key1 = generator.GenerateQueryKey(compact);
		var key2 = generator.GenerateQueryKey(padded);

		// Assert
		key1.Should().Be(key2);
	}

	/// <summary>
	/// Tests the <see cref="QueryCacheKeyGenerator.GenerateResultKey(string)"/> method with a valid SQL query.
	/// Verifies that generated result keys have the expected "sqlanalyzer:result:" prefix.
	/// </summary>
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

	/// <summary>
	/// Tests the <see cref="ValidationRuleEngine.ValidateQuery(string)"/> method with a custom validation rule registered.
	/// Verifies that registered rules are invoked exactly once during query validation.
	/// </summary>
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
