#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;

namespace SqlQueryAnalyzer.Models;

/// <summary>
/// Provides validation helpers for <see cref="PlanVisualization"/> instances.
/// </summary>
public static class PlanVisualizationValidation
{
    /// <summary>
    /// Validates a <see cref="PlanVisualization"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The plan visualization to validate.</param>
    /// <returns>An enumerable of validation messages; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this PlanVisualization? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate TextTree
        if (string.IsNullOrWhiteSpace(value.TextTree))
        {
            errors.Add("TextTree cannot be null, empty, or whitespace.");
        }

        // Validate CostDistribution
        if (string.IsNullOrWhiteSpace(value.CostDistribution))
        {
            errors.Add("CostDistribution cannot be null, empty, or whitespace.");
        }

        // Validate Bottlenecks list
        if (value.Bottlenecks is null)
        {
            errors.Add("Bottlenecks collection cannot be null.");
        }
        else
        {
            // Validate each bottleneck
            foreach (var bottleneck in value.Bottlenecks)
            {
                if (bottleneck is null)
                {
                    errors.Add("Bottlenecks collection contains a null element.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(bottleneck.NodeId))
                {
                    errors.Add("Bottleneck.NodeId cannot be null, empty, or whitespace.");
                }

                if (string.IsNullOrWhiteSpace(bottleneck.NodeType))
                {
                    errors.Add("Bottleneck.NodeType cannot be null, empty, or whitespace.");
                }

                if (string.IsNullOrWhiteSpace(bottleneck.ObjectName))
                {
                    errors.Add("Bottleneck.ObjectName cannot be null, empty, or whitespace.");
                }

                if (bottleneck.EstimatedCost < 0)
                {
                    errors.Add("Bottleneck.EstimatedCost cannot be negative.");
                }

                if (bottleneck.Depth < 0)
                {
                    errors.Add("Bottleneck.Depth cannot be negative.");
                }

                if (string.IsNullOrWhiteSpace(bottleneck.Recommendation))
                {
                    errors.Add("Bottleneck.Recommendation cannot be null, empty, or whitespace.");
                }
            }
        }

        // Validate Stats dictionary
        if (value.Stats is null)
        {
            errors.Add("Stats dictionary cannot be null.");
        }

        // Validate RenderedAt (should not be default DateTime)
        if (value.RenderedAt == default)
        {
            errors.Add("RenderedAt cannot be the default DateTime value.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="PlanVisualization"/> instance is valid.
    /// </summary>
    /// <param name="value">The plan visualization to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this PlanVisualization? value)
    {
        return value is not null && Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="PlanVisualization"/> instance is valid, throwing an <see cref="ArgumentException"/>
    /// with a detailed message listing all validation failures if it is not.
    /// </summary>
    /// <param name="value">The plan visualization to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the plan visualization is invalid, with a message listing all problems.</exception>
    public static void EnsureValid(this PlanVisualization? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"PlanVisualization is invalid:{Environment.NewLine}  {string.Join($"{Environment.NewLine}  ", errors)}");
    }
}
