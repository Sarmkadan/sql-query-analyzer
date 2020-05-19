#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using SqlQueryAnalyzer.Configuration;
using SqlQueryAnalyzer.Models;
using SqlQueryAnalyzer.Visualization;
using Xunit;

namespace SqlQueryAnalyzer.Tests;

public class HtmlPlanVisualizerTests
{
    private readonly HtmlPlanVisualizer _sut = new(ProfilerSettings.ForDevelopment());

    [Fact]
    public void RenderHtml_WithValidPlan_ContainsExpectedHtmlElements()
    {
        var plan = new QueryPlan
        {
            Format = PlanFormat.SqlServer,
            TotalEstimatedCost = 15,
            TotalEstimatedRows = 500,
            RootNode = new PlanNode
            {
                NodeType = "Nested Loops",
                EstimatedCost = 15,
                EstimatedRows = 500,
                Children =
                [
                    new PlanNode
                    {
                        NodeType = "Table Scan",
                        ObjectName = "Orders",
                        EstimatedCost = 12,
                        EstimatedRows = 500,
                        Depth = 1
                    }
                ]
            }
        };
        plan.Initialize();

        var html = _sut.RenderHtml(plan);

        html.Should().Contain("<html");
        html.Should().Contain("<table>");
        html.Should().Contain("Bottleneck Highlights");
        html.Should().Contain("Orders");
    }

    [Fact]
    public void RenderHtml_WithEmptyPlan_ReturnsValidHtml()
    {
        var plan = new QueryPlan();

        var html = _sut.RenderHtml(plan);

        html.Should().Contain("<html");
        html.Should().Contain("No execution plan data available");
    }
}
