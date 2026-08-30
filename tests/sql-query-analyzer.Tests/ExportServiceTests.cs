#nullable enable

using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using SqlQueryAnalyzer.Export;
using SqlQueryAnalyzer.Formatters;
using SqlQueryAnalyzer.Models;
using Xunit;

namespace SqlQueryAnalyzer.Tests;

/// <summary>
/// Unit tests for the <see cref="ExportService"/> class.
/// </summary>
public class ExportServiceTests
{
    [Fact]
    public async Task ExportAsync_CustomFormatter_SelectsFormatterCaseInsensitively()
    {
        // Arrange
        var service = CreateService();
        var formatter = new StubResultFormatter("custom output");
        var filePath = CreateTempFilePath("custom");
        service.RegisterFormatter("CuStOm", formatter);

        try
        {
            // Act
            await service.ExportAsync(CreateResult(), filePath, "cUsToM");

            // Assert
            formatter.FormatCallCount.Should().Be(1);
            (await File.ReadAllTextAsync(filePath)).Should().Be("custom output");
        }
        finally
        {
            DeleteIfExists(filePath);
        }
    }

    [Fact]
    public async Task ExportAsync_UnsupportedFormat_ThrowsArgumentException()
    {
        // Arrange
        var service = CreateService();
        var filePath = CreateTempFilePath("unsupported");

        try
        {
            // Act
            Func<Task> act = () => service.ExportAsync(CreateResult(), filePath, "unsupported");

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Unsupported format: unsupported");
        }
        finally
        {
            DeleteIfExists(filePath);
        }
    }

    [Theory]
    [InlineData("json", "\"queryId\": \"export-test\"")]
    [InlineData("csv", "QueryId,IssueType,Severity,Description,ImpactPercentage")]
    public async Task ExportAsync_SupportedFormat_WritesFile(string format, string expectedContent)
    {
        // Arrange
        var service = CreateService();
        var filePath = CreateTempFilePath(format);

        try
        {
            // Act
            await service.ExportAsync(CreateResult(), filePath, format);

            // Assert
            File.Exists(filePath).Should().BeTrue();
            (await File.ReadAllTextAsync(filePath)).Should().Contain(expectedContent);
        }
        finally
        {
            DeleteIfExists(filePath);
        }
    }

    [Fact]
    public async Task ExportAsync_ExistingFile_OverwritesContent()
    {
        // Arrange
        var service = CreateService();
        var formatter = new StubResultFormatter("replacement content");
        var filePath = CreateTempFilePath("overwrite");
        service.RegisterFormatter("custom", formatter);

        try
        {
            await File.WriteAllTextAsync(filePath, "original content that should be removed");

            // Act
            await service.ExportAsync(CreateResult(), filePath, "custom");

            // Assert
            (await File.ReadAllTextAsync(filePath)).Should().Be("replacement content");
        }
        finally
        {
            DeleteIfExists(filePath);
        }
    }

    private static ExportService CreateService() =>
        new(NullLogger<ExportService>.Instance);

    private static QueryAnalysisResult CreateResult() =>
        new()
        {
            QueryId = "export-test",
            Query = "SELECT id FROM Orders",
            PerformanceScore = 95
        };

    private static string CreateTempFilePath(string extension) =>
        Path.Combine(Path.GetTempPath(), $"export-service-{Guid.NewGuid():N}.{extension}");

    private static void DeleteIfExists(string filePath)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    private sealed class StubResultFormatter(string content) : IResultFormatter
    {
        public int FormatCallCount { get; private set; }

        public string Format(QueryAnalysisResult result)
        {
            FormatCallCount++;
            return content;
        }

        public string FormatBatch(IEnumerable<QueryAnalysisResult> results) => content;

        public string GetFormatType() => "custom";
    }
}
