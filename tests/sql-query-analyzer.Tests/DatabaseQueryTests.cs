using System;
using System.Diagnostics;
using SqlQueryAnalyzer.Models;
using Xunit;

namespace SqlQueryAnalyzer.Tests;

public class DatabaseQueryTests
{
    [Fact]
    public void Parse_ShouldNotHangOnPathologicalInput()
    {
        // Pathological input for regex-based SQL parsing:
        // Long string of nested comments or repetitions to trigger backtracking if vulnerable.
        var pathologicalQuery = "SELECT * FROM Table1 /*" + new string('*', 10000) + "/";
        
        var dbQuery = new DatabaseQuery
        {
            QueryText = pathologicalQuery
        };

        var sw = Stopwatch.StartNew();
        
        // This should run quickly, either successfully or by timing out the regex.
        dbQuery.Parse();
        
        sw.Stop();

        Assert.True(sw.Elapsed < TimeSpan.FromSeconds(5), $"Parse took too long: {sw.Elapsed}");
    }
}
