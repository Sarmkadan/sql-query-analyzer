using System;
using SqlQueryAnalyzer.Utilities;

Console.WriteLine("Query1: SELECT * FROM Users WHERE Id IN (SELECT UserId FROM ActiveUsers)");
var tables1 = SqlPatternAnalyzer.ExtractTablesFromQuery("SELECT * FROM Users WHERE Id IN (SELECT UserId FROM ActiveUsers)");
Console.WriteLine($"Tables: {string.Join(", ", tables1)}");
Console.WriteLine($"Count: {tables1.Count}");

Console.WriteLine();
Console.WriteLine("Query2: SELECT * FROM Users WHERE Id IN (SELECT UserId FROM Orders) AND DeptId IN (SELECT DeptId FROM Departments)");
var tables2 = SqlPatternAnalyzer.ExtractTablesFromQuery("SELECT * FROM Users WHERE Id IN (SELECT UserId FROM Orders) AND DeptId IN (SELECT DeptId FROM Departments)");
Console.WriteLine($"Tables: {string.Join(", ", tables2)}");
Console.WriteLine($"Count: {tables2.Count}");

Console.WriteLine();
Console.WriteLine("Query3: SELECT u.Name, o.Total FROM Users u JOIN Orders o ON u.Id = o.UserId WHERE u.Name = 'John' AND o.Status = 'Active'");
var tables3 = SqlPatternAnalyzer.ExtractTablesFromQuery("SELECT u.Name, o.Total FROM Users u JOIN Orders o ON u.Id = o.UserId WHERE u.Name = 'John' AND o.Status = 'Active'");
Console.WriteLine($"Tables: {string.Join(", ", tables3)}");
Console.WriteLine($"Count: {tables3.Count}");
