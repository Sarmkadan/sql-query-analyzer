#nullable enable

using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using SqlQueryAnalyzer.Constants;
using SqlQueryAnalyzer.DTOs;
using SqlQueryAnalyzer.Models;
using ModelIndex = SqlQueryAnalyzer.Models.Index;
using Xunit;

namespace SqlQueryAnalyzer.Tests;

/// <summary>
/// Tests for DTO mapper functionality, specifically focusing on AnalysisRequestDto
/// and related DTO serialization/deserialization.
/// </summary>
public class DtoMapperTests
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    [Fact]
    public void AnalysisRequestDto_WithAllPropertiesPopulated_MapsCorrectly()
    {
        // Arrange
        var dto = new AnalysisRequestDto
        {
            QueryText = "SELECT * FROM Users WHERE Id = @Id",
            ApplicationName = "UserService",
            ProcedureName = "GetUserById",
            ModuleName = "UserModule",
            IncludeIndexSuggestions = false,
            AnalyzeFragmentation = false,
            AnalyzePlan = true,
            ExecutionPlanXml = "<Plan>...</Plan>"
        };

        // Act & Assert - Test property values
        dto.QueryText.Should().Be("SELECT * FROM Users WHERE Id = @Id");
        dto.ApplicationName.Should().Be("UserService");
        dto.ProcedureName.Should().Be("GetUserById");
        dto.ModuleName.Should().Be("UserModule");
        dto.IncludeIndexSuggestions.Should().BeFalse();
        dto.AnalyzeFragmentation.Should().BeFalse();
        dto.AnalyzePlan.Should().BeTrue();
        dto.ExecutionPlanXml.Should().Be("<Plan>...</Plan>");
    }

    [Fact]
    public void AnalysisRequestDto_WithNullOptionalFields_HasCorrectPropertyValues()
    {
        // Arrange
        var dto = new AnalysisRequestDto
        {
            QueryText = "SELECT COUNT(*) FROM Orders",
            ApplicationName = null,
            ProcedureName = null,
            ModuleName = null,
            IncludeIndexSuggestions = true,
            AnalyzeFragmentation = true,
            AnalyzePlan = false,
            ExecutionPlanXml = null
        };

        // Assert - test property values directly
        dto.QueryText.Should().Be("SELECT COUNT(*) FROM Orders");
        dto.ApplicationName.Should().BeNull();
        dto.ProcedureName.Should().BeNull();
        dto.ModuleName.Should().BeNull();
        dto.IncludeIndexSuggestions.Should().BeTrue();
        dto.AnalyzeFragmentation.Should().BeTrue();
        dto.AnalyzePlan.Should().BeFalse();
        dto.ExecutionPlanXml.Should().BeNull();
    }

    [Fact]
    public void AnalysisRequestDto_WithEmptyCollections_UsesEmptyCollectionsNotNull()
    {
        // Arrange - AnalysisRequestDto doesn't have collection properties, but we test the pattern
        var dto = new AnalysisRequestDto
        {
            QueryText = "SELECT 1"
        };

        // Act
        var json = dto.ToJson();
        var deserialized = JsonSerializer.Deserialize<AnalysisRequestDto>(json, _jsonOptions);

        // Assert - collections should be empty, not null
        deserialized.Should().NotBeNull();
        deserialized!.QueryText.Should().Be("SELECT 1");
    }

    [Fact]
    public void AnalysisRequestDto_DefaultConstructor_HasCorrectDefaultValues()
    {
        // Arrange
        var dto = new AnalysisRequestDto();

        // Assert
        dto.QueryText.Should().BeEmpty();
        dto.ApplicationName.Should().BeNull();
        dto.ProcedureName.Should().BeNull();
        dto.ModuleName.Should().BeNull();
        dto.IncludeIndexSuggestions.Should().BeTrue(); // Default value
        dto.AnalyzeFragmentation.Should().BeTrue();   // Default value
        dto.AnalyzePlan.Should().BeFalse();          // Default value
        dto.ExecutionPlanXml.Should().BeNull();
    }

    [Fact]
    public void AnalysisRequestDto_JsonRoundTrip_PreservesAllData()
    {
        // Arrange
        var original = new AnalysisRequestDto
        {
            QueryText = "SELECT u.Name, o.Total FROM Users u JOIN Orders o ON u.Id = o.UserId WHERE o.Date > @Date",
            ApplicationName = "OrderProcessing",
            ProcedureName = "GetRecentOrders",
            ModuleName = "SalesModule",
            IncludeIndexSuggestions = false,
            AnalyzeFragmentation = true,
            AnalyzePlan = true,
            ExecutionPlanXml = "<QueryPlan><RelOp><IndexScan/></RelOp></QueryPlan>"
        };

        // Act
        var json = original.ToJson();
        var deserialized = JsonSerializer.Deserialize<AnalysisRequestDto>(json, _jsonOptions);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.QueryText.Should().Be(original.QueryText);
        deserialized.ApplicationName.Should().Be(original.ApplicationName);
        deserialized.ProcedureName.Should().Be(original.ProcedureName);
        deserialized.ModuleName.Should().Be(original.ModuleName);
        deserialized.IncludeIndexSuggestions.Should().Be(original.IncludeIndexSuggestions);
        deserialized.AnalyzeFragmentation.Should().Be(original.AnalyzeFragmentation);
        deserialized.AnalyzePlan.Should().Be(original.AnalyzePlan);
        deserialized.ExecutionPlanXml.Should().Be(original.ExecutionPlanXml);
    }

    [Fact]
    public void AnalysisRequestDto_JsonRoundTrip_WithMinimalData_Succeeds()
    {
        // Arrange - minimal valid request
        var original = new AnalysisRequestDto
        {
            QueryText = "SELECT 1"
        };

        // Act
        var json = original.ToJson();
        var deserialized = JsonSerializer.Deserialize<AnalysisRequestDto>(json, _jsonOptions);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.QueryText.Should().Be("SELECT 1");
        deserialized.ApplicationName.Should().BeNull();
        deserialized.ProcedureName.Should().BeNull();
        deserialized.ModuleName.Should().BeNull();
        deserialized.IncludeIndexSuggestions.Should().BeTrue();
        deserialized.AnalyzeFragmentation.Should().BeTrue();
        deserialized.AnalyzePlan.Should().BeFalse();
        deserialized.ExecutionPlanXml.Should().BeNull();
    }

    [Fact]
    public void AnalysisRequestDto_JsonRoundTrip_WithCamelCaseProperties()
    {
        // Arrange
        var original = new AnalysisRequestDto
        {
            QueryText = "SELECT * FROM Products",
            IncludeIndexSuggestions = false
        };

        // Act
        var json = original.ToJson();

        // Assert - should use camelCase for property names
        json.Should().Contain("queryText");
        json.Should().Contain("includeIndexSuggestions");
        json.Should().NotContain("QueryText");
        json.Should().NotContain("IncludeIndexSuggestions");

        // Should not contain null properties
        json.Should().NotContain("applicationName");
        json.Should().NotContain("procedureName");
        json.Should().NotContain("moduleName");
        json.Should().NotContain("executionPlanXml");
    }

    [Fact]
    public void AnalysisRequestDto_FromJsonToAnalysisRequest_WithValidJson_Succeeds()
    {
        // Arrange
        var json = "{\r\n" +
                   "  \"queryText\": \"SELECT * FROM Customers WHERE Status = 'Active'\",\r\n" +
                   "  \"applicationName\": \"CustomerService\",\r\n" +
                   "  \"includeIndexSuggestions\": false,\r\n" +
                   "  \"analyzeFragmentation\": false,\r\n" +
                   "  \"analyzePlan\": true\r\n" +
                   "}";

        // Act
        var result = DtoMapperJsonExtensions.FromJsonToAnalysisRequest(json);

        // Assert
        result.Should().NotBeNull();
        result!.QueryText.Should().Be("SELECT * FROM Customers WHERE Status = 'Active'");
        result.ApplicationName.Should().Be("CustomerService");
        result.ProcedureName.Should().BeNull();
        result.ModuleName.Should().BeNull();
        result.IncludeIndexSuggestions.Should().BeFalse();
        result.AnalyzeFragmentation.Should().BeFalse();
        result.AnalyzePlan.Should().BeTrue();
        result.ExecutionPlanXml.Should().BeNull();
    }

    [Fact]
    public void AnalysisRequestDto_FromJsonToAnalysisRequest_WithInvalidJson_ThrowsJsonException()
    {
        // Arrange
        var invalidJson = "invalid json {{{";

        // Act
        var act = () => DtoMapperJsonExtensions.FromJsonToAnalysisRequest(invalidJson);

        // Assert - the non-throwing alternative is TryFromJsonToAnalysisRequest
        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void AnalysisRequestDto_TryFromJsonToAnalysisRequest_WithValidJson_ReturnsTrueAndValue()
    {
        // Arrange
        var json = "{\r\n" +
                   "  \"queryText\": \"UPDATE Products SET Price = Price * 1.1 WHERE Category = 'Electronics'\",\r\n" +
                   "  \"procedureName\": \"UpdateProductPrices\"\r\n" +
                   "}";

        // Act
        var success = DtoMapperJsonExtensions.TryFromJsonToAnalysisRequest(json, out var result);

        // Assert
        success.Should().BeTrue();
        result.Should().NotBeNull();
        result!.QueryText.Should().Be("UPDATE Products SET Price = Price * 1.1 WHERE Category = 'Electronics'");
        result.ProcedureName.Should().Be("UpdateProductPrices");
    }

    [Fact]
    public void AnalysisRequestDto_TryFromJsonToAnalysisRequest_WithInvalidJson_ReturnsFalseAndNull()
    {
        // Arrange
        var invalidJson = "not valid json";

        // Act
        var success = DtoMapperJsonExtensions.TryFromJsonToAnalysisRequest(invalidJson, out var result);

        // Assert
        success.Should().BeFalse();
        result.Should().BeNull();
    }

    [Fact]
    public void AnalysisRequestDto_ToJson_WithIndentedFormat_ProducesFormattedOutput()
    {
        // Arrange
        var dto = new AnalysisRequestDto
        {
            QueryText = "SELECT * FROM Table1",
            ApplicationName = "TestApp"
        };

        // Act
        var indentedJson = dto.ToJson(indented: true);
        var compactJson = dto.ToJson(indented: false);

        // Assert
        indentedJson.Should().Contain(Environment.NewLine);
        indentedJson.Should().Contain("queryText");
        indentedJson.Should().Contain("applicationName");

        compactJson.Should().NotContain(Environment.NewLine);
        compactJson.Should().Contain("queryText");
        compactJson.Should().Contain("applicationName");
    }

    [Fact]
    public void AnalysisRequestDto_QueryTextIsRequired_WhenEmptyOrWhitespace()
    {
        // Arrange & Act
        var emptyDto = new AnalysisRequestDto { QueryText = "" };
        var whitespaceDto = new AnalysisRequestDto { QueryText = "   " };

        // Assert - These should still be valid objects, but may fail validation elsewhere
        // The DTO itself doesn't validate, that's handled by the validation layer
        emptyDto.QueryText.Should().BeEmpty();
        whitespaceDto.QueryText.Should().Be("   ");
    }

    [Fact]
    public void AnalysisRequestDto_BooleanFlags_DefaultToCorrectValues()
    {
        // Arrange
        var dto = new AnalysisRequestDto();

        // Assert
        dto.IncludeIndexSuggestions.Should().BeTrue();
        dto.AnalyzeFragmentation.Should().BeTrue();
        dto.AnalyzePlan.Should().BeFalse();
    }

    [Fact]
    public void AnalysisRequestDto_PropertyAssignment_WorksCorrectly()
    {
        // Arrange
        var dto = new AnalysisRequestDto();

        // Act
        dto.QueryText = "SELECT * FROM Test";
        dto.ApplicationName = "TestApp";
        dto.IncludeIndexSuggestions = false;

        // Assert
        dto.QueryText.Should().Be("SELECT * FROM Test");
        dto.ApplicationName.Should().Be("TestApp");
        dto.IncludeIndexSuggestions.Should().BeFalse();
    }

    private readonly DtoMapper _mapper = new();

    [Fact]
    public void ToResponseDto_NullIssues_MapsToEmptyIssuesAndZeroCounts()
    {
        var analysis = new QueryAnalysisResult { Issues = null! };

        var dto = _mapper.ToResponseDto(analysis, 5);

        dto.Issues.Should().BeEmpty();
        dto.IssueCount.Should().Be(0);
        dto.CriticalIssueCount.Should().Be(0);
    }

    [Fact]
    public void ToResponseDto_NullIndexSuggestions_MapsToEmptySuggestions()
    {
        var analysis = new QueryAnalysisResult { IndexSuggestions = null! };

        var dto = _mapper.ToResponseDto(analysis, 5);

        dto.IndexSuggestions.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void ToResponseDto_NullEntriesInIssuesAndSuggestions_AreSkipped()
    {
        var analysis = new QueryAnalysisResult
        {
            Issues = [null!, new PerformanceIssue { Severity = IssueSeverity.Critical }],
            IndexSuggestions = [null!]
        };

        var dto = _mapper.ToResponseDto(analysis, 5);

        dto.IssueCount.Should().Be(1);
        dto.CriticalIssueCount.Should().Be(1);
        dto.Issues.Should().HaveCount(1);
        dto.IndexSuggestions.Should().BeEmpty();
    }

    [Fact]
    public void ToResponseDto_NullQueryId_MapsToEmptyString()
    {
        var analysis = new QueryAnalysisResult { QueryId = null! };

        var dto = _mapper.ToResponseDto(analysis, 5);

        dto.QueryId.Should().Be(string.Empty);
    }

    [Fact]
    public void ToResponseDto_NullIssuesAndSuggestions_SummaryReportsZeroCounts()
    {
        var analysis = new QueryAnalysisResult { Issues = null!, IndexSuggestions = null! };

        var dto = _mapper.ToResponseDto(analysis, 5);

        dto.Summary.Should().Be(
            "Score: 0/100 | Issues: 0 (0 critical, 0 warnings, 0 info) | Suggestions: 0 | Optimization Potential: 0.0%");
    }

    [Fact]
    public void ToResponseDto_WithAnalysisTimeMs_MapsAnalysisTimeMs()
    {
        var dto = _mapper.ToResponseDto(new QueryAnalysisResult(), 42);

        dto.AnalysisTimeMs.Should().Be(42);
    }

    [Fact]
    public void ToIssueDto_NullDescriptionAndRecommendedFix_MapToEmptyStrings()
    {
        var issue = new PerformanceIssue { Description = null!, RecommendedFix = null! };

        var dto = _mapper.ToIssueDto(issue);

        dto.Description.Should().Be(string.Empty);
        dto.RecommendedFix.Should().Be(string.Empty);
    }

    [Fact]
    public void ToIssueDto_NullIssue_ThrowsArgumentNullException()
    {
        var act = () => _mapper.ToIssueDto(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ToSuggestionDto_NullNames_MapToEmptyStrings()
    {
        var suggestion = new IndexSuggestion { TableName = null!, IndexName = null! };

        var dto = _mapper.ToSuggestionDto(suggestion);

        dto.TableName.Should().Be(string.Empty);
        dto.IndexName.Should().Be(string.Empty);
    }

    [Fact]
    public void ToSuggestionDto_NullColumnLists_MapToEmptyListsNotSharedReferences()
    {
        var suggestion = new IndexSuggestion { IndexColumns = null!, IncludeColumns = null! };

        var dto = _mapper.ToSuggestionDto(suggestion);

        dto.Columns.Should().NotBeNull().And.BeEmpty();
        dto.IncludeColumns.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void ToSuggestionDto_ColumnLists_AreCopiedNotShared()
    {
        var suggestion = new IndexSuggestion { IndexColumns = ["Id"], IncludeColumns = ["Name"] };

        var dto = _mapper.ToSuggestionDto(suggestion);

        dto.Columns.Should().Equal("Id");
        dto.Columns.Should().NotBeSameAs(suggestion.IndexColumns);
        dto.IncludeColumns.Should().NotBeSameAs(suggestion.IncludeColumns);
    }

    [Fact]
    public void ToSuggestionDto_NullCreateScript_MapsToEmptyString()
    {
        var suggestion = new IndexSuggestion { GeneratedCreateScript = null! };

        var dto = _mapper.ToSuggestionDto(suggestion);

        dto.CreateScript.Should().Be(string.Empty);
    }

    [Fact]
    public void ToIndexDetailDto_NullIndexName_MapsToEmptyString()
    {
        var index = new ModelIndex { IndexName = null! };

        var dto = _mapper.ToIndexDetailDto(index);

        dto.IndexName.Should().Be(string.Empty);
    }

    [Fact]
    public void ToIndexDetailDto_NullColumns_MapsToEmptyList()
    {
        var index = new ModelIndex { Columns = null! };

        var dto = _mapper.ToIndexDetailDto(index);

        dto.Columns.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void ToIndexDetailDto_NullColumnEntries_AreSkipped()
    {
        var index = new ModelIndex
        {
            Columns = [null!, new IndexColumn { ColumnName = "Id", KeyOrdinal = 1 }]
        };

        var dto = _mapper.ToIndexDetailDto(index);

        dto.Columns.Should().Equal("Id");
    }

    [Fact]
    public void ToIndexDetailDto_NullColumnName_MapsToEmptyString()
    {
        var index = new ModelIndex
        {
            Columns = [new IndexColumn { ColumnName = null!, KeyOrdinal = 1 }]
        };

        var dto = _mapper.ToIndexDetailDto(index);

        dto.Columns.Should().Equal(string.Empty);
    }

    [Fact]
    public void ToIndexAnalysisResponseDto_EmptyInputs_MapsAllCountsToZero()
    {
        var dto = _mapper.ToIndexAnalysisResponseDto("Orders", [], [], []);

        dto.TableName.Should().Be("Orders");
        dto.TotalIndexes.Should().Be(0);
        dto.UnusedCount.Should().Be(0);
        dto.FragmentedCount.Should().Be(0);
        dto.Indexes.Should().BeEmpty();
        dto.Suggestions.Should().BeEmpty();
        dto.MaintenanceScripts.Should().BeEmpty();
    }

    [Fact]
    public void ToIndexAnalysisResponseDto_NullEntries_AreSkipped()
    {
        var indexes = new List<ModelIndex> { null!, new ModelIndex { IndexName = "IX_Orders_Date" } };
        var suggestions = new List<IndexSuggestion> { null! };

        var dto = _mapper.ToIndexAnalysisResponseDto("Orders", indexes, suggestions, []);

        dto.TotalIndexes.Should().Be(1);
        dto.Indexes.Should().HaveCount(1);
        dto.Suggestions.Should().BeEmpty();
    }

    [Fact]
    public void ToQueryDetailDto_NullReferencedTables_MapsToEmptyTablesAndZeroCount()
    {
        var query = new DatabaseQuery { ReferencedTables = null! };

        var dto = _mapper.ToQueryDetailDto(query);

        dto.Tables.Should().NotBeNull().And.BeEmpty();
        dto.TableCount.Should().Be(0);
    }

    [Fact]
    public void ToQueryDetailDto_NullJoinConditions_MapsToZeroJoinCount()
    {
        var query = new DatabaseQuery { JoinConditions = null! };

        var dto = _mapper.ToQueryDetailDto(query);

        dto.JoinCount.Should().Be(0);
    }

    [Fact]
    public void ToQueryDetailDto_NullParameters_MapsToZeroParameterCount()
    {
        var query = new DatabaseQuery { Parameters = null! };

        var dto = _mapper.ToQueryDetailDto(query);

        dto.ParameterCount.Should().Be(0);
    }

    [Fact]
    public void ToQueryDetailDto_NullQueryIdAndText_MapToEmptyStrings()
    {
        var query = new DatabaseQuery { QueryId = null!, QueryText = null! };

        var dto = _mapper.ToQueryDetailDto(query);

        dto.QueryId.Should().Be(string.Empty);
        dto.QueryText.Should().Be(string.Empty);
    }

    [Fact]
    public void ToBatchResponseDto_NullEntriesInResults_AreSkipped()
    {
        var results = new List<QueryAnalysisResult> { null!, new QueryAnalysisResult { PerformanceScore = 80 } };

        var dto = _mapper.ToBatchResponseDto(results, [], 10);

        dto.TotalQueries.Should().Be(1);
        dto.SuccessfulAnalyses.Should().Be(1);
        dto.Results.Should().HaveCount(1);
        dto.AverageScore.Should().Be(80);
    }

    [Fact]
    public void ToBatchResponseDto_EmptyResults_MapsCountsAndAverageToZero()
    {
        var dto = _mapper.ToBatchResponseDto([], [], 10);

        dto.TotalQueries.Should().Be(0);
        dto.Results.Should().BeEmpty();
        dto.NPlusOnePatterns.Should().BeEmpty();
        dto.AverageScore.Should().Be(0);
        dto.TotalAnalysisTimeMs.Should().Be(10);
    }
}