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

public class ExplainPlanParserTests
{
    private readonly Mock<IQueryPlanAnalyzerService> _mockPlanAnalyzerService;
    private readonly Mock<ILogger<ExplainPlanParserService>> _mockLogger;
    private readonly ExplainPlanParserService _sut;

    public ExplainPlanParserTests()
    {
        _mockPlanAnalyzerService = new Mock<IQueryPlanAnalyzerService>();
        _mockLogger = new Mock<ILogger<ExplainPlanParserService>>();
        _sut = new ExplainPlanParserService(_mockPlanAnalyzerService.Object, _mockLogger.Object);
    }

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
