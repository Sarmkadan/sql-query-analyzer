using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ModelIndex = SqlQueryAnalyzer.Models.Index;
using SqlQueryAnalyzer.Models;

namespace SqlQueryAnalyzer.Services
{
    /// <summary>
    /// Extension methods for <see cref="IndexAnalyzerService"/> that provide additional functionality
    /// for index analysis and maintenance operations.
    /// </summary>
    public static class IndexAnalyzerServiceExtensions
    {
        /// <summary>
        /// Gets the most fragmented indexes from the database, ordered by fragmentation percentage descending.
        /// </summary>
        /// <param name="service">The index analyzer service instance.</param>
        /// <param name="threshold">The minimum fragmentation percentage to consider (0-100). Default is 30.</param>
        /// <param name="limit">Maximum number of indexes to return. Default is 10.</param>
        /// <returns>An ordered list of the most fragmented indexes, or empty list if none found.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="service"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="threshold"/> is not between 0 and 100.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="limit"/> is less than or equal to 0.</exception>
        public static async Task<IReadOnlyList<ModelIndex>> GetMostFragmentedIndexesAsync(
            this IndexAnalyzerService service,
            int threshold = 30,
            int limit = 10)
        {
            ArgumentNullException.ThrowIfNull(service);
            ArgumentOutOfRangeException.ThrowIfLessThan(threshold, 0);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(threshold, 100);
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(limit, 0);

            var fragmentedIndexes = await service.GetFragmentedIndexesAsync();
            return fragmentedIndexes
                .Where(i => i is not null && i.FragmentationPercentage >= threshold)
                .OrderByDescending(i => i.FragmentationPercentage)
                .Take(limit)
                .ToList()
                .AsReadOnly();
        }

        /// <summary>
        /// Gets all unused indexes that are safe to drop based on their usage statistics.
        /// </summary>
        /// <param name="service">The index analyzer service instance.</param>
        /// <param name="daysOfInactivity">Minimum days without usage to consider an index unused. Default is 90 days.</param>
        /// <returns>A list of indexes that are safe to drop, or empty list if none found.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="service"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="daysOfInactivity"/> is negative.</exception>
        /// <remarks>
        /// This method uses the index's creation date as a proxy for when it became unused,
        /// since usage statistics may not be available or accurate for all indexes.
        /// </remarks>
        public static async Task<IReadOnlyList<ModelIndex>> GetSafeToDropUnusedIndexesAsync(
            this IndexAnalyzerService service,
            int daysOfInactivity = 90)
        {
            ArgumentNullException.ThrowIfNull(service);
            ArgumentOutOfRangeException.ThrowIfNegative(daysOfInactivity);

            var unusedIndexes = await service.GetUnusedIndexesAsync();

            // Filter by days of inactivity based on creation date as proxy for unused duration
            return unusedIndexes
                .Where(i => i is not null && (DateTime.UtcNow - i.CreatedDate).TotalDays >= daysOfInactivity)
                .ToList()
                .AsReadOnly();
        }

        /// <summary>
        /// Generates a prioritized maintenance script for all indexes that need attention,
        /// ordered by severity (fragmented indexes first, then unused indexes to drop).
        /// </summary>
        /// <param name="service">The index analyzer service instance.</param>
        /// <param name="fragmentationThreshold">Minimum fragmentation percentage for maintenance.</param>
        /// <returns>A prioritized list of maintenance script commands.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="service"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="fragmentationThreshold"/> is not between 0 and 100.</exception>
        public static async Task<IReadOnlyList<string>> GeneratePrioritizedMaintenanceScriptsAsync(
            this IndexAnalyzerService service,
            int fragmentationThreshold = 30)
        {
            ArgumentNullException.ThrowIfNull(service);
            ArgumentOutOfRangeException.ThrowIfLessThan(fragmentationThreshold, 0);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(fragmentationThreshold, 100);

            var scripts = new List<string>();

            // 1. Fragmented indexes that need rebuilding
            var fragmented = await service.GetFragmentedIndexesAsync();
            foreach (var index in fragmented.Where(i => i is not null && i.FragmentationPercentage >= fragmentationThreshold))
            {
                scripts.Add($"-- Rebuild index {index.SchemaName}.{index.TableName}.{index.IndexName} (fragmentation: {index.FragmentationPercentage:F1}%)\n" +
                          $"ALTER INDEX [{index.IndexName}] ON [{index.SchemaName}].[{index.TableName}] REBUILD;\n");
            }

            // 2. Unused indexes that can be dropped
            var unused = await service.GetUnusedIndexesAsync();
            foreach (var index in unused.Where(i => i is not null))
            {
                scripts.Add($"-- Drop unused index {index.SchemaName}.{index.TableName}.{index.IndexName}\n" +
                          $"DROP INDEX [{index.IndexName}] ON [{index.SchemaName}].[{index.TableName}];\n");
            }

            return scripts.AsReadOnly();
        }

        /// <summary>
        /// Gets a summary report of index statistics including counts and health status.
        /// </summary>
        /// <param name="service">The index analyzer service instance.</param>
        /// <returns>A dictionary containing index statistics and health information.
        /// The dictionary keys are case-insensitive for lookup convenience.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="service"/> is null.</exception>
        public static async Task<Dictionary<string, object>> GetIndexSummaryReportAsync(
            this IndexAnalyzerService service)
        {
            ArgumentNullException.ThrowIfNull(service);

            var report = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            // Get all indexes for statistics
            var allIndexes = await service.GetFragmentedIndexesAsync();

            // Basic counts
            report["TotalIndexes"] = allIndexes.Count;
            report["FragmentedIndexes"] = allIndexes.Count(i => i is not null && i.FragmentationPercentage > 0);
            report["UnusedIndexes"] = allIndexes.Count(i => i is not null && !i.IsUsed);
            report["AverageFragmentation"] = allIndexes.Any(i => i is not null)
                ? allIndexes.Where(i => i is not null).Average(i => i.FragmentationPercentage)
                : 0.0;

            // Health assessment - assess each index individually
            var healthIssues = new List<string>();
            foreach (var index in allIndexes.Where(i => i is not null))
            {
                var health = await service.AssessIndexHealthAsync(index);
                if (health != Models.IndexHealth.Healthy)
                {
                    healthIssues.Add($"{index.IndexName} - {health}");
                }
            }

            report["HealthStatus"] = healthIssues.Count == 0 ? "Healthy" : "Unhealthy";
            report["HealthIssues"] = healthIssues.Count;
            report["HealthDetails"] = healthIssues;

            // Maintenance recommendations
            var maintenanceScripts = await service.GenerateMaintenanceScriptsAsync();
            report["MaintenanceScriptsCount"] = maintenanceScripts.Count;
            report["RequiresImmediateAttention"] = maintenanceScripts.Count > 0;

            return report;
        }
    }
}