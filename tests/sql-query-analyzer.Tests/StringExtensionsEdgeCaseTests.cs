#nullable enable
using FluentAssertions;
using SqlQueryAnalyzer.Utilities;
using Xunit;

namespace SqlQueryAnalyzer.Tests;

public sealed class StringExtensionsEdgeCaseTests
{
    [Fact]
    public void NormalizeSqlWhitespace_NullInput_ReturnsNull()
    {
        string? input = null;
        input!.NormalizeSqlWhitespace().Should().BeNull();
    }

    [Fact]
    public void NormalizeSqlWhitespace_EmptyInput_ReturnsEmpty()
    {
        "".NormalizeSqlWhitespace().Should().BeEmpty();
    }

    [Fact]
    public void NormalizeSqlWhitespace_MultipleSpaces_NormalizesToSingle()
    {
        "SELECT  *   FROM    users".NormalizeSqlWhitespace()
            .Should().Be("SELECT * FROM users");
    }

    [Fact]
    public void NormalizeSqlWhitespace_TabsAndNewlines_NormalizedToSpaces()
    {
        "SELECT *\n\tFROM\r\n\tusers".NormalizeSqlWhitespace()
            .Should().Be("SELECT * FROM users");
    }

    [Fact]
    public void RemoveSqlComments_NullInput_ReturnsNull()
    {
        string? input = null;
        input!.RemoveSqlComments().Should().BeNull();
    }

    [Fact]
    public void RemoveSqlComments_EmptyInput_ReturnsEmpty()
    {
        "".RemoveSqlComments().Should().BeEmpty();
    }

    [Fact]
    public void RemoveSqlComments_LineComment_Removed()
    {
        "SELECT * FROM users -- get all users".RemoveSqlComments()
            .Should().NotContain("--")
            .And.Contain("SELECT");
    }

    [Fact]
    public void RemoveSqlComments_BlockComment_Removed()
    {
        "SELECT /* columns */ * FROM users".RemoveSqlComments()
            .Should().NotContain("/*")
            .And.Contain("SELECT")
            .And.Contain("FROM");
    }

    [Fact]
    public void Truncate_NullInput_ReturnsNull()
    {
        string? input = null;
        input!.Truncate(10).Should().BeNull();
    }

    [Fact]
    public void Truncate_ShortString_ReturnsUnchanged()
    {
        "hello".Truncate(10).Should().Be("hello");
    }

    [Fact]
    public void Truncate_LongString_AddsDots()
    {
        var result = "This is a long query".Truncate(10);
        result.Should().EndWith("...");
        result.Length.Should().BeLessThanOrEqualTo(10);
    }
}
