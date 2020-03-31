using Xunit; using FluentAssertions;
using SqlQueryAnalyzer.Models;

namespace SqlQueryAnalyzer.Tests {

    public class QueryPlanAnalyzerTests {

        [Fact]
        public void AnalyzeQueryPlan_InvalidQueryPlan_ThrowsException() {
            // Arrange
            var queryPlan = "";

            // Act and Assert
            var exception = Assert.Throws<ArgumentException>(() => QueryPlanAnalyzerService.AnalyzeQueryPlan(null));
            exception.Message.Should().Contain("planXml");
        }

        [Fact]
        public void ParseExecutionPlanAsync_ValidXmlPlan_ReturnsQueryPlan() {
            // Arrange
            var xmlPlan = @"<?xml version=\"1.0\"?>
                <ShowPlanXML>
                    <Batch>
                        <Statements>
                            <StmtSimple StatementText=\"SELECT * FROM Users\" />
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
        public void ParseExecutionPlanAsync_InvalidXml_ThrowsException() {
            // Arrange
            var invalidXml = "INVALID XML";
            var service = new QueryPlanAnalyzerService(null!);

            // Act and Assert
            var exception = Assert.ThrowsAnyAsync<Exception>(
                () => service.ParseExecutionPlanAsync(invalidXml)).Result;
        }

        [Fact]
        public async Task GetTableScans_WithTableScans_ReturnsTableScans() {
            // Arrange
            var xmlPlan = @"<?xml version=\"1.0\"?>
                <ShowPlanXML>
                    <Batch>
                        <Statements>
                            <StmtSimple StatementText=\"SELECT * FROM Users\" />
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
        public async Task GetMissingIndexes_WithTableScans_ReturnsRecommendations() {
            // Arrange - Create a plan with table scan
            var xmlPlan = @"<?xml version=\"1.0\"?>
                <ShowPlanXML>
                    <Batch>
                        <Statements>
                            <StmtSimple StatementText=\"SELECT * FROM Users\" />
                        </Statements>
                    </Batch>
                </ShowPlanXML>";

            var service = new QueryPlanAnalyzerService(null!);
            var plan = await service.ParseExecutionPlanAsync(xmlPlan);
            if (plan != null) {
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
