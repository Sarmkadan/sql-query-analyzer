#nullable enable

namespace SqlQueryAnalyzer.Testing;

/// <summary>
/// Provides validation helpers for <see cref="SampleQueryProvider"/>.
/// </summary>
public static class SampleQueryProviderValidation
{
    /// <summary>
    /// Validates the <see cref="SampleQueryProvider"/> class.
    /// </summary>
    /// <returns>A list of validation errors; empty if valid.</returns>
    public static IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();

        // Validate all query methods return non-null, non-empty strings
        if (string.IsNullOrWhiteSpace(SampleQueryProvider.GetOptimizedQuery()))
        {
            errors.Add("GetOptimizedQuery() returned null or whitespace");
        }

        if (string.IsNullOrWhiteSpace(SampleQueryProvider.GetSelectStarQuery()))
        {
            errors.Add("GetSelectStarQuery() returned null or whitespace");
        }

        if (string.IsNullOrWhiteSpace(SampleQueryProvider.GetNPlusOneQuery()))
        {
            errors.Add("GetNPlusOneQuery() returned null or whitespace");
        }

        if (string.IsNullOrWhiteSpace(SampleQueryProvider.GetImplicitConversionQuery()))
        {
            errors.Add("GetImplicitConversionQuery() returned null or whitespace");
        }

        if (string.IsNullOrWhiteSpace(SampleQueryProvider.GetNonSargableQuery()))
        {
            errors.Add("GetNonSargableQuery() returned null or whitespace");
        }

        if (string.IsNullOrWhiteSpace(SampleQueryProvider.GetComplexJoinQuery()))
        {
            errors.Add("GetComplexJoinQuery() returned null or whitespace");
        }

        if (string.IsNullOrWhiteSpace(SampleQueryProvider.GetLeadingWildcardQuery()))
        {
            errors.Add("GetLeadingWildcardQuery() returned null or whitespace");
        }

        if (string.IsNullOrWhiteSpace(SampleQueryProvider.GetOrConditionQuery()))
        {
            errors.Add("GetOrConditionQuery() returned null or whitespace");
        }

        if (string.IsNullOrWhiteSpace(SampleQueryProvider.GetSubqueryQuery()))
        {
            errors.Add("GetSubqueryQuery() returned null or whitespace");
        }

        if (string.IsNullOrWhiteSpace(SampleQueryProvider.GetDistinctQuery()))
        {
            errors.Add("GetDistinctQuery() returned null or whitespace");
        }

        if (string.IsNullOrWhiteSpace(SampleQueryProvider.GetSimpleQuery()))
        {
            errors.Add("GetSimpleQuery() returned null or whitespace");
        }

        if (string.IsNullOrWhiteSpace(SampleQueryProvider.GetAggregationQuery()))
        {
            errors.Add("GetAggregationQuery() returned null or whitespace");
        }

        if (string.IsNullOrWhiteSpace(SampleQueryProvider.GetCteQuery()))
        {
            errors.Add("GetCteQuery() returned null or whitespace");
        }

        if (string.IsNullOrWhiteSpace(SampleQueryProvider.GetVeryComplexQuery()))
        {
            errors.Add("GetVeryComplexQuery() returned null or whitespace");
        }

        // Validate GetAllSamples returns non-null dictionary with valid entries
        var allSamples = SampleQueryProvider.GetAllSamples();
        if (allSamples is null)
        {
            errors.Add("GetAllSamples() returned null");
        }
        else
        {
            foreach (var kvp in allSamples)
            {
                if (string.IsNullOrWhiteSpace(kvp.Key))
                {
                    errors.Add("GetAllSamples() contains entry with null or whitespace key");
                }

                if (string.IsNullOrWhiteSpace(kvp.Value))
                {
                    errors.Add($"GetAllSamples() contains entry with key '{kvp.Key}' that has null or whitespace value");
                }
            }
        }

        // Validate GetRandomSample returns non-null, non-empty string
        if (string.IsNullOrWhiteSpace(SampleQueryProvider.GetRandomSample()))
        {
            errors.Add("GetRandomSample() returned null or whitespace");
        }

        // Validate GetSamplesByIssueType returns non-null dictionary with valid entries
        var samplesByIssueType = SampleQueryProvider.GetSamplesByIssueType();
        if (samplesByIssueType is null)
        {
            errors.Add("GetSamplesByIssueType() returned null");
        }
        else
        {
            foreach (var kvp in samplesByIssueType)
            {
                if (string.IsNullOrWhiteSpace(kvp.Key))
                {
                    errors.Add("GetSamplesByIssueType() contains entry with null or whitespace key");
                }

                if (kvp.Value is null)
                {
                    errors.Add($"GetSamplesByIssueType() contains entry with key '{kvp.Key}' that has null list");
                }
                else
                {
                    foreach (var query in kvp.Value)
                    {
                        if (string.IsNullOrWhiteSpace(query))
                        {
                            errors.Add($"GetSamplesByIssueType() contains entry with key '{kvp.Key}' that has null or whitespace query string");
                        }
                    }
                }
            }
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the <see cref="SampleQueryProvider"/> is valid.
    /// </summary>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid() => Validate().Count == 0;

    /// <summary>
    /// Ensures that the <see cref="SampleQueryProvider"/> is valid.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if <see cref="SampleQueryProvider"/> is not valid, containing the validation errors.</exception>
    public static void EnsureValid()
    {
        var errors = Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"SampleQueryProvider is not valid. Validation errors: {string.Join("; ", errors)}");
        }
    }
}
