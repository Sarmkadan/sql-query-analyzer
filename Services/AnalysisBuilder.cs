#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using SqlQueryAnalyzer.DTOs;
using SqlQueryAnalyzer.Models;
using SqlQueryAnalyzer.Utilities;

namespace SqlQueryAnalyzer.Services;

/// <summary>
/// Builder class for creating analysis requests and configuring analysis options
/// </summary>
public sealed class AnalysisBuilder
{
    private AnalysisRequestDto _request = new();
    private readonly List<string> _validationErrors = [];

    // Set query text
    public AnalysisBuilder WithQuery(string queryText)
    {
        if (string.IsNullOrWhiteSpace(queryText))
        {
            _validationErrors.Add("Query text cannot be empty");
        }
        else if (queryText.Length > 1048576) // 1MB
        {
            _validationErrors.Add("Query text exceeds maximum size of 1MB");
        }
        else
        {
            _request.QueryText = queryText;
        }

        return this;
    }

    // Set application context
    public AnalysisBuilder WithApplication(string applicationName)
    {
        if (!string.IsNullOrWhiteSpace(applicationName))
            _request.ApplicationName = applicationName;
        return this;
    }

    // Set procedure context
    public AnalysisBuilder WithProcedure(string procedureName)
    {
        if (!string.IsNullOrWhiteSpace(procedureName))
            _request.ProcedureName = procedureName;
        return this;
    }

    // Set module context
    public AnalysisBuilder WithModule(string moduleName)
    {
        if (!string.IsNullOrWhiteSpace(moduleName))
            _request.ModuleName = moduleName;
        return this;
    }

    // Enable/disable index suggestions
    public AnalysisBuilder IncludeIndexSuggestions(bool include = true)
    {
        _request.IncludeIndexSuggestions = include;
        return this;
    }

    // Enable/disable fragmentation analysis
    public AnalysisBuilder AnalyzeFragmentation(bool analyze = true)
    {
        _request.AnalyzeFragmentation = analyze;
        return this;
    }

    // Enable/disable execution plan analysis
    public AnalysisBuilder AnalyzePlan(bool analyze = true)
    {
        _request.AnalyzePlan = analyze;
        return this;
    }

    // Set execution plan XML
    public AnalysisBuilder WithExecutionPlan(string planXml)
    {
        if (!string.IsNullOrWhiteSpace(planXml))
            _request.ExecutionPlanXml = planXml;
        return this;
    }

    // Validate and build
    public AnalysisRequestDto Build()
    {
        if (string.IsNullOrWhiteSpace(_request.QueryText))
            throw new InvalidOperationException("Query text is required");

        if (!QueryValidator.IsValidQuery(_request.QueryText))
            throw new InvalidOperationException("Query text is not valid SQL");

        if (_validationErrors.Count > 0)
            throw new InvalidOperationException($"Validation errors: {string.Join(", ", _validationErrors)}");

        return _request;
    }

    // Reset builder
    public AnalysisBuilder Reset()
    {
        _request = new();
        _validationErrors.Clear();
        return this;
    }

    // Fluent convenience methods
    public AnalysisBuilder Full()
    {
        _request.IncludeIndexSuggestions = true;
        _request.AnalyzeFragmentation = true;
        _request.AnalyzePlan = true;
        return this;
    }

    public AnalysisBuilder Quick()
    {
        _request.IncludeIndexSuggestions = false;
        _request.AnalyzeFragmentation = false;
        _request.AnalyzePlan = false;
        return this;
    }

    // Get validation errors
    public List<string> GetErrors() => new(_validationErrors);

    // Check if builder is valid
    public bool IsValid() => _validationErrors.Count == 0 && !string.IsNullOrWhiteSpace(_request.QueryText);
}

/// <summary>
/// Builder for batch analysis requests
/// </summary>
public sealed class BatchAnalysisBuilder
{
    private BatchAnalysisRequestDto _request = new();
    private readonly List<string> _validationErrors = [];

    // Add single query
    public BatchAnalysisBuilder AddQuery(string queryText)
    {
        if (!string.IsNullOrWhiteSpace(queryText))
            _request.Queries.Add(queryText);
        return this;
    }

    // Add multiple queries
    public BatchAnalysisBuilder AddQueries(params string[] queries)
    {
        foreach (var query in queries)
        {
            if (!string.IsNullOrWhiteSpace(query))
                _request.Queries.Add(query);
        }
        return this;
    }

    // Add queries from collection
    public BatchAnalysisBuilder AddQueries(IEnumerable<string> queries)
    {
        foreach (var query in queries)
        {
            if (!string.IsNullOrWhiteSpace(query))
                _request.Queries.Add(query);
        }
        return this;
    }

    // Set application context
    public BatchAnalysisBuilder WithApplication(string applicationName)
    {
        if (!string.IsNullOrWhiteSpace(applicationName))
            _request.ApplicationName = applicationName;
        return this;
    }

    // Enable pattern analysis
    public BatchAnalysisBuilder AnalyzePatterns(bool analyze = true)
    {
        _request.AnalyzePatterns = analyze;
        return this;
    }

    // Set timeout
    public BatchAnalysisBuilder WithTimeout(int seconds)
    {
        if (seconds > 0 && seconds <= 3600)
            _request.TimeoutSeconds = seconds;
        else
            _validationErrors.Add("Timeout must be between 1 and 3600 seconds");
        return this;
    }

    // Build with validation
    public BatchAnalysisRequestDto Build()
    {
        if (_request.Queries.Count == 0)
            throw new InvalidOperationException("At least one query is required");

        if (_request.Queries.Count > 100)
            throw new InvalidOperationException("Maximum 100 queries per batch");

        if (_validationErrors.Count > 0)
            throw new InvalidOperationException($"Validation errors: {string.Join(", ", _validationErrors)}");

        return _request;
    }

    // Reset builder
    public BatchAnalysisBuilder Reset()
    {
        _request = new();
        _validationErrors.Clear();
        return this;
    }

    // Get validation errors
    public List<string> GetErrors() => new(_validationErrors);

    // Check validity
    public bool IsValid() => _validationErrors.Count == 0 && _request.Queries.Count > 0;
}

/// <summary>
/// Builder for index analysis requests
/// </summary>
public sealed class IndexAnalysisBuilder
{
    private IndexAnalysisRequestDto _request = new();
    private readonly List<string> _validationErrors = [];

    // Set table name
    public IndexAnalysisBuilder ForTable(string tableName)
    {
        if (!string.IsNullOrWhiteSpace(tableName))
            _request.TableName = tableName;
        else
            _validationErrors.Add("Table name is required");
        return this;
    }

    // Include fragmentation analysis
    public IndexAnalysisBuilder IncludeFragmentation(bool include = true)
    {
        _request.IncludeFragmentation = include;
        return this;
    }

    // Include unused index detection
    public IndexAnalysisBuilder IncludeUnused(bool include = true)
    {
        _request.IncludeUnused = include;
        return this;
    }

    // Include script generation
    public IndexAnalysisBuilder GenerateScripts(bool generate = true)
    {
        _request.GenerateScripts = generate;
        return this;
    }

    // Full analysis
    public IndexAnalysisBuilder Full()
    {
        _request.IncludeFragmentation = true;
        _request.IncludeUnused = true;
        _request.GenerateScripts = true;
        return this;
    }

    // Quick analysis
    public IndexAnalysisBuilder Quick()
    {
        _request.IncludeFragmentation = false;
        _request.IncludeUnused = false;
        _request.GenerateScripts = false;
        return this;
    }

    // Build with validation
    public IndexAnalysisRequestDto Build()
    {
        if (string.IsNullOrWhiteSpace(_request.TableName))
            _validationErrors.Add("Table name is required");

        if (_validationErrors.Count > 0)
            throw new InvalidOperationException($"Validation errors: {string.Join(", ", _validationErrors)}");

        return _request;
    }

    // Reset builder
    public IndexAnalysisBuilder Reset()
    {
        _request = new();
        _validationErrors.Clear();
        return this;
    }

    // Get validation errors
    public List<string> GetErrors() => new(_validationErrors);

    // Check validity
    public bool IsValid() => _validationErrors.Count == 0 && !string.IsNullOrWhiteSpace(_request.TableName);
}
