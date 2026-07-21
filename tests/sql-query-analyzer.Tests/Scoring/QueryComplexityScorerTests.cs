#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using SqlQueryAnalyzer.Constants;
using SqlQueryAnalyzer.Models;
using SqlQueryAnalyzer.Scoring;
using Xunit;

namespace SqlQueryAnalyzer.Tests.Scoring;

/// <summary>
/// Tests for <see cref="QueryComplexityScorer"/> class.
/// </summary>
public class QueryComplexityScorerTests
{
    [Fact]
    public void ComputeScore_EmptyQuery_ReturnsZero()
    {
        // Arrange
        var result = new QueryAnalysisResult
        {
            Query = ""
        };

        // Act
        var score = QueryComplexityScorer.ComputeScore(result);

        // Assert
        score.Should().Be(0);
    }

    [Fact]
    public void ComputeScore_WhitespaceQuery_ReturnsZero()
    {
        // Arrange
        var result = new QueryAnalysisResult
        {
            Query = "   \n\t  "
        };

        // Act
        var score = QueryComplexityScorer.ComputeScore(result);

        // Assert
        score.Should().Be(0);
    }

    [Fact]
    public void ComputeScore_SimpleSelectSingleTable_ReturnsOne()
    {
        // Arrange
        var result = new QueryAnalysisResult
        {
            Query = "SELECT * FROM Users"
        };

        // Act
        var score = QueryComplexityScorer.ComputeScore(result);

        // Assert
        score.Should().Be(1);
    }

    [Fact]
    public void ComputeScore_SelectWithTwoTables_ReturnsTwo()
    {
        // Arrange
        var result = new QueryAnalysisResult
        {
            Query = "SELECT * FROM Users u JOIN Orders o ON u.Id = o.UserId"
        };

        // Act
        var score = QueryComplexityScorer.ComputeScore(result);

        // Assert
        score.Should().Be(2);
    }

    [Fact]
    public void ComputeScore_SelectWithThreeTables_ReturnsThree()
    {
        // Arrange
        var result = new QueryAnalysisResult
        {
            Query = "SELECT * FROM Users u JOIN Orders o ON u.Id = o.UserId JOIN Products p ON o.ProductId = p.Id"
        };

        // Act
        var score = QueryComplexityScorer.ComputeScore(result);

        // Assert
        score.Should().Be(3);
    }

    [Fact]
    public void ComputeScore_QueryWithTableScan_AddsFivePoints()
    {
        // Arrange
        var result = new QueryAnalysisResult
        {
            Query = "SELECT * FROM Users WHERE Name = 'John'",
            Issues = new List<PerformanceIssue>
            {
                new PerformanceIssue
                {
                    IssueType = IssueType.TableScan,
                    Severity = IssueSeverity.Warning,
                    Description = "Full table scan detected"
                }
            }
        };

        // Act
        var score = QueryComplexityScorer.ComputeScore(result);

        // Assert
        score.Should().Be(6); // 1 table + 5 for table scan
    }

    [Fact]
    public void ComputeScore_QueryWithMultipleTableScans_AddsMultiplePoints()
    {
        // Arrange
        var result = new QueryAnalysisResult
        {
            Query = "SELECT * FROM Users WHERE Name = 'John'",
            Issues = new List<PerformanceIssue>
            {
                new PerformanceIssue { IssueType = IssueType.TableScan, Severity = IssueSeverity.Warning, Description = "Full table scan on Users" },
                new PerformanceIssue { IssueType = IssueType.TableScan, Severity = IssueSeverity.Warning, Description = "Full table scan on Orders" }
            }
        };

        // Act
        var score = QueryComplexityScorer.ComputeScore(result);

        // Assert
        score.Should().Be(11); // 1 table + 10 for two table scans (5 each)
    }

    [Fact]
    public void ComputeScore_QueryWithMissingIndex_AddsThreePoints()
    {
        // Arrange
        var result = new QueryAnalysisResult
        {
            Query = "SELECT * FROM Users WHERE Name = 'John'",
            Issues = new List<PerformanceIssue>
            {
                new PerformanceIssue
                {
                    IssueType = IssueType.MissingIndex,
                    Severity = IssueSeverity.Warning,
                    Description = "Missing index on Name column"
                }
            }
        };

        // Act
        var score = QueryComplexityScorer.ComputeScore(result);

        // Assert
        score.Should().Be(4); // 1 table + 3 for missing index
    }

    [Fact]
    public void ComputeScore_QueryWithNPlusOne_AddsTenPoints()
    {
        // Arrange
        var result = new QueryAnalysisResult
        {
            Query = "SELECT * FROM Users",
            Issues = new List<PerformanceIssue>
            {
                new PerformanceIssue
                {
                    IssueType = IssueType.NPlusOne,
                    Severity = IssueSeverity.Critical,
                    Description = "N+1 query pattern detected"
                }
            }
        };

        // Act
        var score = QueryComplexityScorer.ComputeScore(result);

        // Assert
        score.Should().Be(11); // 1 table + 10 for N+1
    }

    [Fact]
    public void ComputeScore_QueryWithSubquery_AddsTwoPoints()
    {
        // Arrange
        var result = new QueryAnalysisResult
        {
            Query = "SELECT * FROM Users WHERE Id IN (SELECT UserId FROM Orders)"
        };

        // Act
        var score = QueryComplexityScorer.ComputeScore(result);

        // Assert
        score.Should().Be(4); // 2 tables + 2 for subquery
    }

    [Fact]
    public void ComputeScore_QueryWithMultipleSubqueries_AddsMultiplePoints()
    {
        // Arrange
        var result = new QueryAnalysisResult
        {
            Query = "SELECT * FROM Users WHERE Id IN (SELECT UserId FROM Orders) AND DeptId IN (SELECT DeptId FROM Departments)"
        };

        // Act
        var score = QueryComplexityScorer.ComputeScore(result);

        // Assert
        score.Should().Be(7); // 3 tables + 4 for two subqueries (2 each)
    }

    [Fact]
    public void ComputeScore_QueryWithAllFactors_ReturnsCorrectTotal()
    {
        // Arrange
        var result = new QueryAnalysisResult
        {
            Query = "SELECT u.*, o.Total FROM Users u JOIN Orders o ON u.Id = o.UserId WHERE u.Name = 'John' AND o.Status = 'Active' AND u.Id IN (SELECT Id FROM ActiveUsers)",
            Issues = new List<PerformanceIssue>
            {
                new PerformanceIssue { IssueType = IssueType.TableScan, Severity = IssueSeverity.Warning, Description = "Full scan on Users" },
                new PerformanceIssue { IssueType = IssueType.MissingIndex, Severity = IssueSeverity.Warning, Description = "Missing index on Users.Name" }
            }
        };

        // Act
        var score = QueryComplexityScorer.ComputeScore(result);

        // Assert
        score.Should().Be(13); // 3 tables (Users, Orders, ActiveUsers) + 5 for table scan + 3 for missing index + 2 for subquery
    }

    [Fact]
    public void ComputeScore_QueryWithMultipleTablesAndAllIssueTypes_ReturnsCorrectTotal()
    {
        // Arrange
        var result = new QueryAnalysisResult
        {
            Query = "SELECT * FROM Users u JOIN Orders o ON u.Id = o.UserId JOIN Products p ON o.ProductId = p.Id",
            Issues = new List<PerformanceIssue>
            {
                new PerformanceIssue { IssueType = IssueType.TableScan, Severity = IssueSeverity.Warning, Description = "Full scan on Users" },
                new PerformanceIssue { IssueType = IssueType.TableScan, Severity = IssueSeverity.Warning, Description = "Full scan on Orders" },
                new PerformanceIssue { IssueType = IssueType.MissingIndex, Severity = IssueSeverity.Warning, Description = "Missing index on Products.CategoryId" },
                new PerformanceIssue { IssueType = IssueType.NPlusOne, Severity = IssueSeverity.Critical, Description = "N+1 pattern detected" },
                new PerformanceIssue { IssueType = IssueType.SubqueryOptimization, Severity = IssueSeverity.Info, Description = "Subquery could be optimized" }
            }
        };

        // Act
        var score = QueryComplexityScorer.ComputeScore(result);

        // Assert
        score.Should().Be(26); // 3 tables + 10 for two table scans + 3 for missing index + 10 for N+1
    }

    [Fact]
    public void ComputeScore_QueryWithNoIssues_ReturnsTableCountOnly()
    {
        // Arrange
        var result = new QueryAnalysisResult
        {
            Query = "SELECT Id, Name FROM Users WHERE Id > 100",
            Issues = new List<PerformanceIssue>()
        };

        // Act
        var score = QueryComplexityScorer.ComputeScore(result);

        // Assert
        score.Should().Be(1); // Only 1 table
    }

    [Fact]
    public void ComputeScore_ComplexQueryWithManyTables_ReturnsCorrectTableCount()
    {
        // Arrange
        var result = new QueryAnalysisResult
        {
            Query = "SELECT u.Name, o.Total, p.ProductName FROM Users u JOIN Orders o ON u.Id = o.UserId JOIN Products p ON o.ProductId = p.Id JOIN Categories c ON p.CategoryId = c.Id JOIN Customers cust ON o.CustomerId = cust.Id"
        };

        // Act
        var score = QueryComplexityScorer.ComputeScore(result);

        // Assert
        score.Should().Be(5); // 5 tables
    }

    [Fact]
    public void ComputeScore_QueryWithSubqueryInFromClause_AddsTwoPoints()
    {
        // Arrange
        var result = new QueryAnalysisResult
        {
            Query = "SELECT * FROM (SELECT Id, Name FROM Users) AS SubQuery"
        };

        // Act
        var score = QueryComplexityScorer.ComputeScore(result);

        // Assert
        score.Should().Be(3); // 1 table (SubQuery) + 2 for subquery
    }

    [Fact]
    public void ComputeScore_QueryWithInlineSubquery_AddsTwoPoints()
    {
        // Arrange
        var result = new QueryAnalysisResult
        {
            Query = "SELECT * FROM Users WHERE Id IN (SELECT Id FROM ActiveUsers)"
        };

        // Act
        var score = QueryComplexityScorer.ComputeScore(result);

        // Assert
        score.Should().Be(4); // 2 tables + 2 for subquery
    }
}
