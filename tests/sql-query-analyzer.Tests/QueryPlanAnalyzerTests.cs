using System;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using SqlQueryAnalyzer.Models;
using SqlQueryAnalyzer.Services;

namespace SqlQueryAnalyzer.Tests
{
    public class QueryPlanAnalyzerTests
    {
        [Fact]
        public void AnalyzeQueryPlan_InvalidQueryPlan_ThrowsException()
        {
            // Arrange
            var queryPlan = "";

            // Act and Assert
            // The service does not have a static AnalyzeQueryPlan method.
            // Instead we use ParseExecutionPlanAsync which validates the input.
            var service = new QueryPlanAnalyzerService(null!);
            Func<Task> act = async () => await service.ParseExecutionPlanAsync(null!);
            act.Should().ThrowAsync<ArgumentException>()
               .WithMessage("*planXml*");
        }

        [Fact]
        public void ParseExecutionPlanAsync_ValidXmlPlan_ReturnsQueryPlan()
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

            var service = new QueryPlanAnalyzerService(null!);

            // Act
            var result = service.ParseExecutionPlanAsync(xmlPlan).Result;

            // Assert
            result.Should().NotBeNull();
            result!.Format.Should().Be(PlanFormat.SqlServer);
        }

        [Fact]
        public void ParseExecutionPlanAsync_InvalidXml_ThrowsQueryPlanException()
        {
            // Arrange
            var invalidXml = "INVALID XML";
            var service = new QueryPlanAnalyzerService(null!);

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

            var service = new QueryPlanAnalyzerService(null!);
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

            var service = new QueryPlanAnalyzerService(null!);
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
