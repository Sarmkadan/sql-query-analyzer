// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SqlQueryAnalyzer.Models;
using SqlQueryAnalyzer.Services;
using Xunit;

namespace SqlQueryAnalyzer.Tests;

/// <summary>
/// Unit tests for the <see cref="ExplainPlanParserService"/> class.
/// Tests parsing of PostgreSQL EXPLAIN plan JSON output and validation of parsed results.
/// </summary>
public class ExplainPlanParserTests
{
    /// <summary>
    /// Mock service for testing <see cref="IQueryPlanAnalyzerService"/> dependencies.
    /// </summary>
    private readonly Mock<IQueryPlanAnalyzerService> _mockPlanAnalyzerService;

    /// <summary>
    /// Mock logger for testing <see cref="ILogger"/> dependencies.
    /// </summary>
    private readonly Mock<ILogger<ExplainPlanParserService>> _mockLogger;

    /// <summary>
    /// System under test - the <see cref="ExplainPlanParserService"/> instance being tested.
    /// </summary>
    private readonly ExplainPlanParserService _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExplainPlanParserTests"/> class.
    /// Sets up mock dependencies for testing the <see cref="ExplainPlanParserService"/> class.
    /// </summary>
    public ExplainPlanParserTests()
    {
        _mockPlanAnalyzerService = new Mock<IQueryPlanAnalyzerService>();
        _mockLogger = new Mock<ILogger<ExplainPlanParserService>>();
        _sut = new ExplainPlanParserService(_mockPlanAnalyzerService.Object, _mockLogger.Object);
    }

    /// <summary>
    /// Tests parsing of PostgreSQL EXPLAIN plan in PostgreSQL 17 format.
    /// Validates that the parser correctly extracts plan metrics from a standard PostgreSQL 17 JSON plan format.
    /// </summary>
    /// <returns>An awaitable task containing the parsed <see cref="QueryPlan"/> object.</returns>
    [Fact]
    public async Task ParsePostgreSqlPlan_Postgres17Format_ReturnsCorrectPlan()
    {
        // Arrange
        var postgres17JsonPlan = @"
        [
          {
            ""Plan"": {
              ""Node Type"": ""Seq Scan"",
              ""Parent Relationship"": ""Outer"",
              ""Parallel Aware"": false,
              ""Async Capable"": false,
              ""Relation Name"": ""users"",
              ""Alias"": ""u"",
              ""Startup Cost"": 0.00,
              ""Total Cost"": 10.50,
              ""Plan Rows"": 100,
              ""Plan Width"": 8,
              ""Actual Startup Time"": 0.050,
              ""Actual Total Time"": 0.250,
              ""Actual Rows"": 100,
              ""Actual Loops"": 1
            },
            ""Planning Time"": 0.080,
            ""Triggers"": [],
            ""Execution Time"": 0.300
          }
        ]";

        // Act
        var result = await _sut.ParsePostgreSqlPlanAsync(postgres17JsonPlan);

        // Assert
        result.Should().NotBeNull();
        result.DatabaseName.Should().Be("PostgreSQL");
        result.Format.Should().Be(PlanFormat.PostgreSQL);
        result.TotalEstimatedCpuCost.Should().Be(0.080); // Planning Time
        result.TotalEstimatedCost.Should().Be(10.50); // Total Cost from Plan node
        result.TotalEstimatedRows.Should().Be(100); // Plan Rows from Plan node
        result.TotalElapsedTime.TotalMilliseconds.Should().Be(0.300); // Execution Time
    }

    /// <summary>
    /// Tests parsing of an empty PostgreSQL EXPLAIN plan JSON array.
    /// Validates that the parser returns a default plan with zero values when given empty input.
    /// </summary>
    /// <returns>An awaitable task containing the parsed <see cref="QueryPlan"/> object with default values.</returns>
    [Fact]
    public async Task ParsePostgreSqlPlan_EmptyJson_ReturnsDefaultPlan()
    {
        // Arrange
        var emptyJsonPlan = @"[]";

        // Act
        var result = await _sut.ParsePostgreSqlPlanAsync(emptyJsonPlan);

        // Assert
        result.Should().NotBeNull();
        result.DatabaseName.Should().Be("PostgreSQL");
        result.Format.Should().Be(PlanFormat.PostgreSQL);
        result.TotalEstimatedCpuCost.Should().Be(0);
        result.TotalEstimatedCost.Should().Be(0);
        result.TotalEstimatedRows.Should().Be(0);
        result.TotalElapsedTime.Should().Be(TimeSpan.Zero);
    }

    /// <summary>
    /// Tests parsing of invalid PostgreSQL EXPLAIN plan JSON.
    /// Validates that the parser logs an error and returns a default plan when given malformed JSON input.
    /// </summary>
    /// <returns>An awaitable task containing the parsed <see cref="QueryPlan"/> object with default values.</returns>
    [Fact]
    public async Task ParsePostgreSqlPlan_InvalidJson_LogsErrorAndReturnsDefaultPlan()
    {
        // Arrange
        var invalidJsonPlan = @"{""invalid""}";

        // Act
        var result = await _sut.ParsePostgreSqlPlanAsync(invalidJsonPlan);

        // Assert
        result.Should().NotBeNull();
        result.DatabaseName.Should().Be("PostgreSQL");
        result.Format.Should().Be(PlanFormat.PostgreSQL);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Error parsing PostgreSQL plan")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
