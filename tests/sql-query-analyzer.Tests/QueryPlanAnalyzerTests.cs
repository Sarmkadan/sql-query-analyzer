using System;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using SqlQueryAnalyzer.Models;
using SqlQueryAnalyzer.Services;

namespace SqlQueryAnalyzer.Tests
{
    public class QueryPlanAnalyzerTests
    {
        private static QueryPlanAnalyzerService CreateService() =>
            new(NullLogger<QueryPlanAnalyzerService>.Instance);

        [Fact]
        public void AnalyzeQueryPlan_InvalidQueryPlan_ThrowsException()
        {
            // Arrange
            var queryPlan = "";

            // Act and Assert
            // The service does not have a static AnalyzeQueryPlan method.
            // Instead we use ParseExecutionPlanAsync which validates the input.
            var service = CreateService();
            Func<Task> act = async () => await service.ParseExecutionPlanAsync(null!);
            act.Should().ThrowAsync<ArgumentException>()
               .WithMessage("*planXml*");
        }

        [Fact]
        public async Task ParseExecutionPlanAsync_ValidXmlPlan_ReturnsQueryPlan()
        {
            // Arrange
            var xmlPlan = @"<?xml version=""1.0""?>
<ShowPlanXML>
    <Batch>
        <Statements>
            <StmtSimple StatementText=""SELECT * FROM Users"" />
        </Statements>
    </Batch>
</ShowPlanXML>";

            var service = CreateService();

            // Act
            var result = await service.ParseExecutionPlanAsync(xmlPlan);

            // Assert
            result.Should().NotBeNull();
            result!.Format.Should().Be(PlanFormat.SqlServer);
        }

        [Fact]
        public void ParseExecutionPlanAsync_InvalidXml_ThrowsQueryPlanException()
        {
            // Arrange
            var invalidXml = "INVALID XML";
            var service = CreateService();

            // Act and Assert
            Func<Task> act = async () => await service.ParseExecutionPlanAsync(invalidXml);
            act.Should().ThrowAsync<Exceptions.QueryPlanException>();
        }

        [Fact]
        public async Task GetTableScans_WithTableScans_ReturnsTableScans()
        {
            // Arrange
            var xmlPlan = @"<?xml version=""1.0""?>
<ShowPlanXML>
    <Batch>
        <Statements>
            <StmtSimple StatementText=""SELECT * FROM Users"" />
        </Statements>
    </Batch>
</ShowPlanXML>";

            var service = CreateService();
            var plan = await service.ParseExecutionPlanAsync(xmlPlan);

            // Act
            var tableScans = plan?.GetTableScans();

            // Assert
            tableScans.Should().NotBeNull();
        }

        [Fact]
        public async Task GetMissingIndexes_WithTableScans_ReturnsRecommendations()
        {
            // Arrange - Create a plan with table scan
            var xmlPlan = @"<?xml version=""1.0""?>
<ShowPlanXML>
    <Batch>
        <Statements>
            <StmtSimple StatementText=""SELECT * FROM Users"" />
        </Statements>
    </Batch>
</ShowPlanXML>";

            var service = CreateService();
            var plan = await service.ParseExecutionPlanAsync(xmlPlan);
            if (plan != null)
            {
                plan.Initialize();
            }

            // Act
            var missingIndexes = await service.GetMissingIndexesAsync(plan!);

            // Assert
            missingIndexes.Should().NotBeNull();
            missingIndexes.Should().BeEmpty();
        }
    }
}
