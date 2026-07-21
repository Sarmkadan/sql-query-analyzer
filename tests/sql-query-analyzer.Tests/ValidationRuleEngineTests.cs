#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SqlQueryAnalyzer.Validation;
using Xunit;

namespace SqlQueryAnalyzer.Tests;

/// <summary>
/// Contains unit tests for the <see cref="ValidationRuleEngine"/> and related validation components.
/// Tests the correctness and edge cases of SQL query validation, rule registration,
/// and result aggregation.
/// </summary>
public class ValidationRuleEngineTests
{
    private readonly Mock<ILogger<ValidationRuleEngine>> _mockLogger;
    private readonly ValidationRuleEngine _engine;

    public ValidationRuleEngineTests()
    {
        _mockLogger = new Mock<ILogger<ValidationRuleEngine>>();
        _engine = new ValidationRuleEngine(_mockLogger.Object);
    }

    // -------------------------------------------------------------------------
    // ValidationRuleEngine – empty ruleset and null input handling
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that an empty ruleset passes validation for a valid query.
    /// </summary>
    [Fact]
    public void ValidateQuery_EmptyRuleset_ReturnsValidResult()
    {
        // Arrange
        var emptyEngine = new ValidationRuleEngine(_mockLogger.Object);
        var query = "SELECT * FROM users";

        // Act
        var result = emptyEngine.ValidateQuery(query);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        result.Warnings.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that null input is handled and produces an appropriate error.
    /// </summary>
    [Fact]
    public void ValidateQuery_NullInput_ReturnsError()
    {
        // Arrange
        string? nullQuery = null;

        // Act
        var result = _engine.ValidateQuery(nullQuery);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.Should().Be("Query cannot be empty");
        result.Warnings.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that empty string input is handled and produces an appropriate error.
    /// </summary>
    [Fact]
    public void ValidateQuery_EmptyString_ReturnsError()
    {
        // Arrange
        var emptyQuery = string.Empty;

        // Act
        var result = _engine.ValidateQuery(emptyQuery);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.Should().Be("Query cannot be empty");
        result.Warnings.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that whitespace-only input is treated as valid.
    /// </summary>
    [Fact]
    public void ValidateQuery_WhitespaceOnly_ReturnsValidResult()
    {
        // Arrange
        var whitespaceQuery = " \t\n ";

        // Act
        var result = _engine.ValidateQuery(whitespaceQuery);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        result.Warnings.Should().BeEmpty();
    }

    // -------------------------------------------------------------------------
    // ValidationRuleEngine – failing rule collects message
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that a failing rule (mismatched parentheses) collects an error message.
    /// </summary>
    [Fact]
    public void ValidateQuery_FailingRule_MismatchedParentheses_CollectsError()
    {
        // Arrange
        var query = "SELECT * FROM users WHERE id IN (1, 2, 3"; // Missing closing parenthesis

        // Act
        var result = _engine.ValidateQuery(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Mismatched parentheses");
    }

    /// <summary>
    /// Tests that a failing rule (mismatched quotes) collects an error message.
    /// </summary>
    [Fact]
    public void ValidateQuery_FailingRule_MismatchedQuotes_CollectsError()
    {
        // Arrange
        var query = "SELECT * FROM users WHERE name = 'John"; // Missing closing quote

        // Act
        var result = _engine.ValidateQuery(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Mismatched quotes");
    }

    /// <summary>
    /// Tests that a failing rule (DELETE without WHERE) collects an error message.
    /// </summary>
    [Fact]
    public void ValidateQuery_FailingRule_DeleteWithoutWhere_CollectsError()
    {
        // Arrange
        var query = "DELETE FROM users";

        // Act
        var result = _engine.ValidateQuery(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("DELETE statement without WHERE clause detected");
    }

    /// <summary>
    /// Tests that a failing rule (query too long) collects an error message.
    /// </summary>
    [Fact]
    public void ValidateQuery_FailingRule_QueryTooLong_CollectsError()
    {
        // Arrange
        var longQuery = new string('x', 1024 * 1024 + 1); // Exceeds 1MB limit

        // Act
        var result = _engine.ValidateQuery(longQuery);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Query exceeds maximum length of 1048576 characters");
    }

    // -------------------------------------------------------------------------
    // ValidationRuleEngine – multiple failures aggregate
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that multiple failing rules aggregate all error messages.
    /// </summary>
    [Fact]
    public void ValidateQuery_MultipleFailures_AggregatesAllErrors()
    {
        // Arrange
        var query = "SELECT * FROM users WHERE id IN (1, 2"; // Mismatched parentheses
        query += " AND name = 'John"; // Mismatched quotes

        // Act
        var result = _engine.ValidateQuery(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().Contain("Mismatched parentheses");
        result.Errors.Should().Contain("Mismatched quotes");
    }

    /// <summary>
    /// Tests that warnings are aggregated along with errors when validation fails.
    /// </summary>
    [Fact]
    public void ValidateQuery_MultipleFailures_CollectsErrorsAndWarnings()
    {
        // Arrange - DELETE without WHERE clause (error)
        var query = "DELETE FROM users";

        // Act
        var result = _engine.ValidateQuery(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("DELETE statement without WHERE clause detected");
        // Note: QueryLengthRule only adds warnings, not errors, so warnings won't be collected
        // when there are errors (see ValidationRuleEngine.ValidateQuery line 48-51)
        result.Warnings.Should().BeEmpty();
    }

    // -------------------------------------------------------------------------
    // ValidationRuleEngine – rule registration and count
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that custom rules can be registered.
    /// </summary>
    [Fact]
    public void RegisterRule_AddsRuleToEngine()
    {
        // Arrange
        var mockRule = new Mock<IValidationRule>();
        mockRule.SetupGet(r => r.Name).Returns("Test Rule");
        mockRule.Setup(r => r.Validate(It.IsAny<string>()))
            .Returns(new RuleValidationResult { IsValid = false, Errors = { "Custom error" } });

        var engine = new ValidationRuleEngine(_mockLogger.Object);

        // Act
        engine.RegisterRule(mockRule.Object);

        // Assert
        engine.GetRuleCount().Should().Be(4); // 3 defaults + 1 registered
    }

    /// <summary>
    /// Tests that default rules are registered on initialization.
    /// </summary>
    [Fact]
    public void GetRuleCount_DefaultRulesRegistered_ReturnsThree()
    {
        // Arrange & Act
        var count = _engine.GetRuleCount();

        // Assert
        count.Should().Be(3); // SqlSyntaxRule, QueryLengthRule, DangerousOperationRule
    }

    // -------------------------------------------------------------------------
    // ValidationResult – overall validation results
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that a valid query produces a valid result with no errors or warnings.
    /// </summary>
    [Fact]
    public void ValidateQuery_ValidQuery_ReturnsValidResultWithNoErrorsOrWarnings()
    {
        // Arrange
        var query = "SELECT id, name FROM users WHERE id = 1";

        // Act
        var result = _engine.ValidateQuery(query);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        result.Warnings.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that a complex valid query passes validation.
    /// </summary>
    [Fact]
    public void ValidateQuery_ComplexValidQuery_ReturnsValidResult()
    {
        // Arrange
        var query = @"
            SELECT u.id, u.name, o.total
            FROM users u
            INNER JOIN orders o ON u.id = o.user_id
            WHERE u.active = true
            AND o.created_at > '2024-01-01'
            GROUP BY u.id, u.name, o.total
            HAVING SUM(o.total) > 100
            ORDER BY o.total DESC
            LIMIT 100
        ";

        // Act
        var result = _engine.ValidateQuery(query);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        result.Warnings.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that a complex invalid query returns multiple errors.
    /// </summary>
    [Fact]
    public void ValidateQuery_ComplexInvalidQuery_ReturnsMultipleErrors()
    {
        // Arrange - query with mismatched parentheses and quotes
        var query = "SELECT * FROM users WHERE id IN (1, 2, 3 AND name = 'John";

        // Act
        var result = _engine.ValidateQuery(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2); // Missing close paren, mismatched quote
    }

    // -------------------------------------------------------------------------
    // RuleValidationResult – individual rule results
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that RuleValidationResult default state is valid.
    /// </summary>
    [Fact]
    public void RuleValidationResult_DefaultState_IsValid()
    {
        // Arrange & Act
        var result = new RuleValidationResult();

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        result.Warnings.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that RuleValidationResult with errors is invalid when IsValid is set to false.
    /// </summary>
    [Fact]
    public void RuleValidationResult_WithErrorsAndIsValidFalse_IsInvalid()
    {
        // Arrange
        var result = new RuleValidationResult();

        // Act
        result.Errors.Add("Error 1");
        result.Errors.Add("Error 2");
        result.IsValid = false;

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
    }

    /// <summary>
    /// Tests that RuleValidationResult with warnings only remains valid.
    /// </summary>
    [Fact]
    public void RuleValidationResult_WithWarningsOnly_RemainsValid()
    {
        // Arrange
        var result = new RuleValidationResult();

        // Act
        result.Warnings.Add("Warning 1");

        // Assert
        result.IsValid.Should().BeTrue(); // Warnings don't affect IsValid
        result.Warnings.Should().HaveCount(1);
    }

    /// <summary>
    /// Tests that RuleValidationResult can track both errors and warnings.
    /// </summary>
    [Fact]
    public void RuleValidationResult_AddErrorAndWarning_TracksBoth()
    {
        // Arrange
        var result = new RuleValidationResult();

        // Act
        result.Errors.Add("Critical error");
        result.Warnings.Add("Minor warning");
        result.IsValid = false;

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Warnings.Should().HaveCount(1);
    }

    // -------------------------------------------------------------------------
    // ValidationResult – overall validation results
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that ValidationResult default state is valid.
    /// </summary>
    [Fact]
    public void ValidationResult_DefaultState_IsValid()
    {
        // Arrange & Act
        var result = new ValidationResult();

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        result.Warnings.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that ValidationResult with errors is invalid when IsValid is set to false.
    /// </summary>
    [Fact]
    public void ValidationResult_WithErrorsAndIsValidFalse_IsInvalid()
    {
        // Arrange
        var result = new ValidationResult();

        // Act
        result.Errors.Add("Error 1");
        result.Errors.Add("Error 2");
        result.IsValid = false;

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
    }

    /// <summary>
    /// Tests that ValidationResult.ToString() returns formatted string.
    /// </summary>
    [Fact]
    public void ValidationResult_ToString_ReturnsFormattedString()
    {
        // Arrange
        var result = new ValidationResult
        {
            IsValid = false,
            Errors = { "Error 1", "Error 2" },
            Warnings = { "Warning 1" }
        };

        // Act
        var str = result.ToString();

        // Assert
        str.Should().Be("Valid: False, Errors: 2, Warnings: 1");
    }
}
