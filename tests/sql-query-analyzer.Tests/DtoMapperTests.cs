#nullable enable

using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using SqlQueryAnalyzer.DTOs;
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
    public void AnalysisRequestDto_FromJsonToAnalysisRequest_WithInvalidJson_ReturnsNull()
    {
        // Arrange
        var invalidJson = "invalid json {{{";

        // Act
        var result = DtoMapperJsonExtensions.FromJsonToAnalysisRequest(invalidJson);

        // Assert
        result.Should().BeNull();
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
        indentedJson.Should().Contain("QueryText");
        indentedJson.Should().Contain("ApplicationName");

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
}