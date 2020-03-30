#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SqlQueryAnalyzer.Services;
using Xunit;

namespace SqlQueryAnalyzer.Tests;

public class SlowQueryLogParserTests
{
    private readonly SlowQueryLogParser _sut;

    public SlowQueryLogParserTests()
    {
        var logger = new Mock<ILogger<SlowQueryLogParser>>();
        _sut = new SlowQueryLogParser(logger.Object);
    }

    [Fact]
    public async Task ParseMySqlLogAsync_WithValidLog_ParsesEntries()
    {
        const string log = """
# Time: 2024-01-15T10:30:00.000000Z
# User@Host: app[app] @ localhost []  Id:   123
# Query_time: 2.345678  Lock_time: 0.001234 Rows_sent: 5  Rows_examined: 50000
use mydb;
SELECT * FROM Orders WHERE CustomerId = 1;
""";

        var entries = await _sut.ParseMySqlLogAsync(log);

        entries.Should().ContainSingle();
        entries[0].Duration.TotalSeconds.Should().BeApproximately(2.345678, 0.000001);
        entries[0].RowsExamined.Should().Be(50000);
        entries[0].RowsSent.Should().Be(5);
    }

    [Fact]
    public async Task ParsePostgreSqlLogAsync_WithValidLog_ParsesDuration()
    {
        const string log = "2024-01-15 10:30:00.123 UTC [12345] app@mydb LOG:  duration: 1234.567 ms  statement: SELECT * FROM orders;";

        var entries = await _sut.ParsePostgreSqlLogAsync(log);

        entries.Should().ContainSingle();
        entries[0].Duration.TotalMilliseconds.Should().BeApproximately(1234.567, 0.001);
        entries[0].Database.Should().Be("mydb");
    }
}
