#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SqlQueryAnalyzer.Models;
using SqlQueryAnalyzer.Services;
using Xunit;

namespace SqlQueryAnalyzer.Tests;

public class IndexRecommendationEngineTests
{
    private readonly IndexRecommendationEngine _sut;

    public IndexRecommendationEngineTests()
    {
        var logger = new Mock<ILogger<IndexRecommendationEngine>>();
        _sut = new IndexRecommendationEngine(logger.Object);
    }

    [Fact]
    public async Task RecommendAsync_WithWhereClause_ReturnsWhereColumnIndex()
    {
        var recommendations = await _sut.RecommendAsync("SELECT * FROM Orders WHERE CustomerId = 1");

        recommendations.Should().ContainSingle(r =>
            r.TableName == "Orders" &&
            r.Source == RecommendationSource.WhereClause &&
            r.KeyColumns.Count == 1 &&
            r.KeyColumns[0] == "CustomerId");
    }

    [Fact]
    public async Task RecommendAsync_WithJoinAndOrderBy_ReturnsRankedRecommendations()
    {
        const string query = "SELECT o.Id, c.Name FROM Orders o JOIN Customers c ON o.CustomerId = c.Id WHERE o.Status = 1 ORDER BY o.OrderDate DESC";

        var recommendations = await _sut.RecommendAsync(query);

        recommendations.Should().HaveCountGreaterThanOrEqualTo(3);
        recommendations.Should().BeInDescendingOrder(r => r.ImpactScore);
        recommendations.Should().Contain(r => r.TableName == "Orders" && r.KeyColumns.Contains("CustomerId") && r.Source == RecommendationSource.JoinCondition);
        recommendations.Should().Contain(r => r.TableName == "Orders" && r.KeyColumns.Contains("OrderDate") && r.Source == RecommendationSource.OrderBy);
    }
}
